using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Orchestrates the "Export Subset of Springs" core operation (Phase 1):
    /// copy the selected springs and all related tables (cascade-filtered by the
    /// kept SiteIDs / SurveyIDs / HUC codes / flora TIDs) into a freshly created
    /// file geodatabase, rebuild the relationship classes, build attribute
    /// indexes, and return a run report. Port of the core of the legacy
    /// CopyAndDeleteNonSelectedSprings2. Must run on the MCT (QueuedTask).
    /// </summary>
    internal sealed class SpringsSubsetExporter
    {
        private readonly ExportSubsetParameters _p;

        internal SpringsSubsetExporter(ExportSubsetParameters parameters)
        {
            _p = parameters;
        }

        /// <summary>The catalog path of the geodatabase created by the last run.</summary>
        internal string OutputGeodatabasePath { get; private set; }

        internal string Run(CancelableProgressor progressor)
        {
            var stopwatch = Stopwatch.StartNew();
            var report = new StringBuilder();
            var store = new KeepSetStore();
            var created = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var sourceConnection = new FileGeodatabaseConnectionPath(
                new Uri(_p.SourceGeodatabasePath));

            using var sourceGdb = new Geodatabase(sourceConnection);

            string outputGdbPath = MakeUniqueGeodatabasePath(_p.OutputFolder, _p.OutputName);
            OutputGeodatabasePath = outputGdbPath;

            // Pre-flight: a file geodatabase requires every dataset's full catalog
            // path to be under 252 characters. Fail early (before creating the
            // geodatabase) with an actionable message instead of partway through.
            string pathError = CheckDatasetPathLengths(outputGdbPath, _p.OutputName, _p.CreateSummary);
            if (pathError != null)
            {
                throw new InvalidOperationException(pathError);
            }

            var outputConnection = new FileGeodatabaseConnectionPath(new Uri(outputGdbPath));

            using var outputGdb = SchemaBuilder.CreateGeodatabase(outputConnection);

            report.AppendLine("Export Subset of Springs");
            report.AppendLine("========================");
            report.AppendLine($"Output geodatabase: {outputGdbPath}");
            report.AppendLine($"Springs feature class: {_p.OutputName}");
            report.AppendLine();

            // 1) Exclusion sets from the two optional SQL clauses.
            BuildExclusionSets(sourceGdb, store, report);

            // Relationship-key text fields must share a length with their paired
            // field, so compute a common length per linked group up front.
            Dictionary<string, Dictionary<string, int>> keyFieldLengths =
                ComputeKeyFieldLengths(sourceGdb);

            // Replicate the source coded-value / range domains into the fresh
            // output geodatabase so the copied fields keep their domains.
            Dictionary<string, DomainDescription> domains =
                ReplicateDomains(sourceGdb, outputGdb, report);

            // Progress: springs + each related copy + relationship-class pass + index pass.
            uint totalSteps = (uint)(1 + SpringsExportSchema.CopyOperations.Count + 2);
            if (progressor != null)
            {
                progressor.Max = totalSteps;
                progressor.Value = 0;
            }

            // 2) Copy the selected springs (builds SiteIDs / Huc8s / Huc12s).
            SetProgress(progressor, "Copying springs...");
            DatasetCopyResult springs = DatasetCopier.CopySprings(
                sourceGdb, outputGdb, _p.SpringsFeatureClassName, _p.OutputName,
                _p.SelectedObjectIds, _p.TrimStrings, store, progressor,
                Overrides(keyFieldLengths, _p.SpringsFeatureClassName), domains);
            Step(progressor);
            RecordResult(report, springs, created);

            report.AppendLine(
                $"  (kept {store.Count(KeepSet.SiteIDs):N0} sites, " +
                $"{store.Count(KeepSet.Huc8s):N0} HUC8, {store.Count(KeepSet.Huc12s):N0} HUC12)");

            // 3) Copy related tables in order.
            foreach (CopyOperation op in SpringsExportSchema.CopyOperations)
            {
                ThrowIfCancelled(progressor);
                SetProgress(progressor, $"Copying {op.SourceName}...");
                DatasetCopyResult result = DatasetCopier.CopyRelated(
                    sourceGdb, outputGdb, op, _p.TrimStrings, store, progressor,
                    Overrides(keyFieldLengths, op.SourceName), domains);
                Step(progressor);
                RecordResult(report, result, created);
            }

            // 4) Relationship classes.
            ThrowIfCancelled(progressor);
            SetProgress(progressor, "Creating relationship classes...");
            CreateRelationshipClasses(outputGdb, created, report);
            Step(progressor);

            // 5) Attribute indexes.
            ThrowIfCancelled(progressor);
            SetProgress(progressor, "Building attribute indexes...");
            CreateIndexes(outputGdb, created, report);
            Step(progressor);

            // 6) Optional summary feature class (one row per spring).
            string summaryName = null;
            IReadOnlyList<string> summaryDatasets = null;
            if (_p.CreateSummary)
            {
                ThrowIfCancelled(progressor);
                SetProgress(progressor, "Building summary feature class...");
                summaryName = _p.OutputName + "_Summary";
                SpringsSummaryExporter.SummaryResult summary =
                    SpringsSummaryExporter.Export(outputGdb, _p.OutputName, summaryName, progressor);
                if (summary.Created)
                {
                    summaryDatasets = summary.CreatedDatasets;
                    report.AppendLine(
                        $"- {summary.Name}: {summary.RecordCount:N0} summary record(s)" +
                        $" + {summary.SupportingTableCount} supporting table(s)" +
                        $" + {summary.RelationshipClassCount} relationship class(es)");
                }
                else
                {
                    report.AppendLine($"- {summaryName}: skipped ({summary.SkipReason})");
                }
            }

            // 7) Optional dataset metadata, from the editable JSON templates.
            if (_p.WriteMetadata)
            {
                ThrowIfCancelled(progressor);
                SetProgress(progressor, "Writing dataset metadata...");
                var metadataDatasets = new List<string>(created);
                if (summaryDatasets != null)
                {
                    metadataDatasets.AddRange(summaryDatasets);
                }

                report.AppendLine(SpringsMetadataWriter.Write(
                    outputGdb, outputGdbPath, _p.OutputName, summaryName, metadataDatasets, progressor));
            }

            stopwatch.Stop();
            report.AppendLine();
            report.AppendLine($"Datasets created: {created.Count}");
            report.AppendLine($"Elapsed: {stopwatch.Elapsed:hh\\:mm\\:ss}");
            return report.ToString();
        }

        // ---------------------------------------------------------------------

        private static IReadOnlyDictionary<string, int> Overrides(
            Dictionary<string, Dictionary<string, int>> all, string datasetName) =>
            all.TryGetValue(datasetName, out var map) ? map : null;

        /// <summary>
        /// Copies every coded-value and range domain from the source geodatabase
        /// into the (fresh) output geodatabase and returns a map of domain name →
        /// description, so the copied dataset fields can re-reference them. Failure
        /// is non-fatal: the export continues without domains.
        /// </summary>
        private static Dictionary<string, DomainDescription> ReplicateDomains(
            Geodatabase sourceGdb, Geodatabase outputGdb, StringBuilder report)
        {
            var result = new Dictionary<string, DomainDescription>(StringComparer.OrdinalIgnoreCase);

            IReadOnlyList<Domain> sourceDomains = sourceGdb.GetDomains();
            if (sourceDomains == null || sourceDomains.Count == 0)
            {
                return result;
            }

            var schemaBuilder = new SchemaBuilder(outputGdb);
            var pending = new List<(string Name, DomainDescription Description)>();

            foreach (Domain domain in sourceDomains)
            {
                try
                {
                    DomainDescription description = null;
                    switch (domain)
                    {
                        case CodedValueDomain coded:
                            var codedDescription = new CodedValueDomainDescription(coded);
                            schemaBuilder.Create(codedDescription);
                            description = codedDescription;
                            break;
                        case RangeDomain range:
                            var rangeDescription = new RangeDomainDescription(range);
                            schemaBuilder.Create(rangeDescription);
                            description = rangeDescription;
                            break;
                    }

                    if (description != null)
                    {
                        pending.Add((domain.GetName(), description));
                    }
                }
                finally
                {
                    domain.Dispose();
                }
            }

            if (pending.Count == 0)
            {
                return result;
            }

            if (schemaBuilder.Build())
            {
                foreach (var (name, description) in pending)
                {
                    result[name] = description;
                }

                report.AppendLine($"Replicated {result.Count} coded-value/range domain(s).");
            }
            else
            {
                report.AppendLine(
                    "! domain replication failed (continuing without domains): " +
                    string.Join("; ", schemaBuilder.ErrorMessages));
            }

            return result;
        }

        /// <summary>
        /// Computes, for every text field used as a relationship-class key, a length
        /// shared by it and the field(s) it is paired with. Pro requires the origin
        /// primary key and origin foreign key of a relationship class to have the
        /// same length; fields linked (directly or transitively) through the 127
        /// relationship classes form a group that is all given the group's maximum
        /// source length. Returns a map: source dataset name → (field name → length).
        /// Numeric-keyed relationships need no normalization and are skipped.
        /// </summary>
        private Dictionary<string, Dictionary<string, int>> ComputeKeyFieldLengths(Geodatabase sourceGdb)
        {
            static string Node(string dataset, string field) => dataset + "" + field;

            var parent = new Dictionary<string, string>();
            var nodeInfo = new Dictionary<string, (string Dataset, string Field)>();

            string Find(string x)
            {
                string root = x;
                while (parent[root] != root)
                {
                    root = parent[root];
                }

                while (parent[x] != root)
                {
                    string next = parent[x];
                    parent[x] = root;
                    x = next;
                }

                return root;
            }

            foreach (RelationshipDef rc in SpringsExportSchema.RelationshipClasses)
            {
                string originDataset = rc.OriginDataset == SpringsExportSchema.SpringsToken
                    ? _p.SpringsFeatureClassName
                    : rc.OriginDataset;

                string a = Node(originDataset, rc.OriginPrimaryKey);
                string b = Node(rc.DestinationDataset, rc.OriginForeignKey);
                nodeInfo[a] = (originDataset, rc.OriginPrimaryKey);
                nodeInfo[b] = (rc.DestinationDataset, rc.OriginForeignKey);

                if (!parent.ContainsKey(a)) parent[a] = a;
                if (!parent.ContainsKey(b)) parent[b] = b;
                parent[Find(a)] = Find(b);
            }

            // Cache of source field (type, length) per dataset.
            var defCache = new Dictionary<string, Dictionary<string, (FieldType Type, int Length)>>(
                StringComparer.OrdinalIgnoreCase);

            Dictionary<string, (FieldType Type, int Length)> FieldsFor(string dataset)
            {
                if (defCache.TryGetValue(dataset, out var cached))
                {
                    return cached;
                }

                var map = new Dictionary<string, (FieldType, int)>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    bool isSprings = string.Equals(dataset, _p.SpringsFeatureClassName, StringComparison.OrdinalIgnoreCase);
                    TableDefinition def;
                    if (isSprings)
                    {
                        using FeatureClass fc = sourceGdb.OpenDataset<FeatureClass>(dataset);
                        def = fc.GetDefinition();
                        foreach (Field f in def.GetFields()) map[f.Name] = (f.FieldType, f.Length);
                    }
                    else
                    {
                        using Table table = sourceGdb.OpenDataset<Table>(dataset);
                        def = table.GetDefinition();
                        foreach (Field f in def.GetFields()) map[f.Name] = (f.FieldType, f.Length);
                    }
                }
                catch (Exception)
                {
                    defCache[dataset] = null;
                    return null;
                }

                defCache[dataset] = map;
                return map;
            }

            var result = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

            foreach (var group in nodeInfo.Keys.GroupBy(Find))
            {
                var members = group.Select(n => nodeInfo[n]).ToList();
                bool allString = true;
                int maxLength = 1;

                foreach (var (dataset, field) in members)
                {
                    var fields = FieldsFor(dataset);
                    if (fields == null || !fields.TryGetValue(field, out var info) || info.Type != FieldType.String)
                    {
                        allString = false;
                        break;
                    }

                    maxLength = Math.Max(maxLength, info.Length);
                }

                if (!allString)
                {
                    continue;
                }

                foreach (var (dataset, field) in members)
                {
                    if (!result.TryGetValue(dataset, out var fieldMap))
                    {
                        fieldMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        result[dataset] = fieldMap;
                    }

                    fieldMap[field] = maxLength;
                }
            }

            return result;
        }

        private void BuildExclusionSets(Geodatabase sourceGdb, KeepSetStore store, StringBuilder report)
        {
            if (!string.IsNullOrWhiteSpace(_p.ExcludeSitesWhereClause))
            {
                CollectKeys(sourceGdb, _p.SpringsFeatureClassName, "SiteID",
                    _p.ExcludeSitesWhereClause, store.ExcludeSiteIds);
                report.AppendLine(
                    $"Excluding surveys for {store.ExcludeSiteIds.Count:N0} site(s) " +
                    $"[{_p.ExcludeSitesWhereClause}]");
            }

            if (!string.IsNullOrWhiteSpace(_p.ExcludeSurveysWhereClause))
            {
                CollectKeys(sourceGdb, "tbl_Surveys", "SurveyID",
                    _p.ExcludeSurveysWhereClause, store.ExcludeSurveyIds);
                report.AppendLine(
                    $"Excluding {store.ExcludeSurveyIds.Count:N0} survey(s) " +
                    $"[{_p.ExcludeSurveysWhereClause}]");
            }
        }

        private static void CollectKeys(
            Geodatabase gdb, string datasetName, string keyField, string whereClause, HashSet<string> target)
        {
            using Table table = gdb.OpenDataset<Table>(datasetName);
            var filter = new QueryFilter { WhereClause = whereClause, SubFields = keyField };
            using RowCursor cursor = table.Search(filter, true);
            while (cursor.MoveNext())
            {
                using Row row = cursor.Current;
                string key = KeepSetStore.KeyString(row[keyField]);
                if (key != null)
                {
                    target.Add(key);
                }
            }
        }

        private void CreateRelationshipClasses(Geodatabase outputGdb, HashSet<string> created, StringBuilder report)
        {
            int ok = 0, skipped = 0, failed = 0;
            foreach (RelationshipDef rc in SpringsExportSchema.RelationshipClasses)
            {
                string origin = rc.OriginDataset == SpringsExportSchema.SpringsToken
                    ? _p.OutputName
                    : rc.OriginDataset;

                if (!created.Contains(origin) || !created.Contains(rc.DestinationDataset))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    var schemaBuilder = new SchemaBuilder(outputGdb);
                    var description = new RelationshipClassDescription(
                        rc.Name,
                        OpenDescription(outputGdb, origin),
                        OpenDescription(outputGdb, rc.DestinationDataset),
                        rc.Cardinality,
                        rc.OriginPrimaryKey,
                        rc.OriginForeignKey)
                    {
                        RelationshipMessageDirection = RelationshipMessageDirection.Forward,
                        ForwardPathLabel = rc.DestinationDataset,
                        BackwardPathLabel = origin,
                    };

                    schemaBuilder.Create(description);
                    if (schemaBuilder.Build())
                    {
                        ok++;
                    }
                    else
                    {
                        failed++;
                        report.AppendLine($"  ! relationship '{rc.Name}': {string.Join("; ", schemaBuilder.ErrorMessages)}");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    report.AppendLine($"  ! relationship '{rc.Name}': {ex.Message}");
                }
            }

            report.AppendLine($"Relationship classes: {ok} created, {skipped} skipped, {failed} failed.");
        }

        private void CreateIndexes(Geodatabase outputGdb, HashSet<string> created, StringBuilder report)
        {
            var indexFields = new HashSet<string>(SpringsExportSchema.IndexFieldNames, StringComparer.OrdinalIgnoreCase);
            int datasetsIndexed = 0, failed = 0;

            foreach (string name in created)
            {
                List<string> present;
                using (Table table = outputGdb.OpenDataset<Table>(name))
                {
                    present = table.GetDefinition().GetFields()
                        .Select(f => f.Name)
                        .Where(indexFields.Contains)
                        .ToList();
                }

                if (present.Count == 0)
                {
                    continue;
                }

                try
                {
                    var schemaBuilder = new SchemaBuilder(outputGdb);
                    TableDescription tableDescription = OpenDescription(outputGdb, name);
                    foreach (string field in present)
                    {
                        schemaBuilder.Create(new AttributeIndexDescription(
                            $"IX_{field}", tableDescription, new[] { field }));
                    }

                    if (schemaBuilder.Build())
                    {
                        datasetsIndexed++;
                    }
                    else
                    {
                        failed++;
                        report.AppendLine($"  ! indexes on '{name}': {string.Join("; ", schemaBuilder.ErrorMessages)}");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    report.AppendLine($"  ! indexes on '{name}': {ex.Message}");
                }
            }

            report.AppendLine($"Indexed {datasetsIndexed} dataset(s)" + (failed > 0 ? $", {failed} failed." : "."));
        }

        /// <summary>
        /// Builds a DDL description of an existing output dataset for use as a
        /// relationship-class endpoint or index target. The springs dataset is a
        /// feature class; everything else is a table.
        /// </summary>
        private TableDescription OpenDescription(Geodatabase outputGdb, string name)
        {
            if (name.Equals(_p.OutputName, StringComparison.OrdinalIgnoreCase))
            {
                using FeatureClass fc = outputGdb.OpenDataset<FeatureClass>(name);
                return new FeatureClassDescription(fc.GetDefinition());
            }

            using Table table = outputGdb.OpenDataset<Table>(name);
            return new TableDescription(table.GetDefinition());
        }

        private static void RecordResult(StringBuilder report, DatasetCopyResult result, HashSet<string> created)
        {
            if (result.Skipped)
            {
                report.AppendLine($"- {result.Name}: skipped ({result.SkipReason})");
                return;
            }

            created.Add(result.Name);
            report.AppendLine($"- {result.Name}: {result.RecordCount:N0} record(s)");
        }

        // File geodatabases require every dataset's full catalog path
        // (gdb path + "\" + dataset name) to be under this many characters.
        private const int MaxFileGdbDatasetPathLength = 252;

        /// <summary>
        /// Checks that the longest dataset this export will create still has a full
        /// catalog path under the file-geodatabase limit. Returns a user-facing
        /// error message (naming the offending path and how much to trim), or null
        /// if everything fits.
        /// </summary>
        private static string CheckDatasetPathLengths(string outputGdbPath, string outputName, bool createSummary)
        {
            string longestName = LongestCreatedDatasetName(outputName, createSummary);
            string longestPath = outputGdbPath + "\\" + longestName;
            if (longestPath.Length < MaxFileGdbDatasetPathLength)
            {
                return null;
            }

            int excess = longestPath.Length - (MaxFileGdbDatasetPathLength - 1);
            return
                "The output location is too long." + Environment.NewLine + Environment.NewLine +
                "A file geodatabase requires every dataset's full path to be under " +
                $"{MaxFileGdbDatasetPathLength} characters, but the longest dataset this export would " +
                $"create is {longestPath.Length} characters:" + Environment.NewLine + Environment.NewLine +
                longestPath + Environment.NewLine + Environment.NewLine +
                $"Shorten the output folder and/or geodatabase name by at least {excess} character(s)" +
                (createSummary
                    ? ", or turn off \"Create summary\" (its datasets have the longest names)."
                    : ".");
        }

        /// <summary>
        /// The longest dataset name this export will create: the springs feature
        /// class (renamed to the output name), the copied datasets, the relationship
        /// classes, and — when requested — the summary datasets.
        /// </summary>
        private static string LongestCreatedDatasetName(string outputName, bool createSummary)
        {
            string longest = outputName ?? string.Empty; // springs FC is renamed to the output name
            foreach (CopyOperation op in SpringsExportSchema.CopyOperations)
            {
                if (op.SourceName.Length > longest.Length) { longest = op.SourceName; }
            }
            foreach (RelationshipDef rc in SpringsExportSchema.RelationshipClasses)
            {
                if (rc.Name.Length > longest.Length) { longest = rc.Name; }
            }
            if (createSummary)
            {
                foreach (string name in SpringsSummaryExporter.PredictedDatasetNames(outputName + "_Summary"))
                {
                    if (name.Length > longest.Length) { longest = name; }
                }
            }
            return longest;
        }

        private static string MakeUniqueGeodatabasePath(string folder, string baseName)
        {
            string candidate = Path.Combine(folder, baseName + ".gdb");
            int suffix = 1;
            while (Directory.Exists(candidate))
            {
                candidate = Path.Combine(folder, $"{baseName}_{suffix}.gdb");
                suffix++;
            }

            return candidate;
        }

        private static void SetProgress(CancelableProgressor progressor, string message)
        {
            if (progressor != null)
            {
                progressor.Message = message;
            }
        }

        private static void Step(CancelableProgressor progressor)
        {
            if (progressor != null)
            {
                progressor.Value += 1;
            }
        }

        private static void ThrowIfCancelled(CancelableProgressor progressor)
        {
            if (progressor != null && progressor.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }
    }
}
