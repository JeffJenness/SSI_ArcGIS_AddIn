using System;
using System.Collections.Generic;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Outcome of copying one dataset.
    /// </summary>
    internal sealed class DatasetCopyResult
    {
        internal string Name { get; init; }
        internal bool Created { get; init; }
        internal bool Skipped { get; init; }
        internal string SkipReason { get; init; }
        internal long RecordCount { get; init; }
    }

    /// <summary>
    /// Ports the legacy CopyOverFClassOrTable: copy a source feature class or
    /// table into the output geodatabase, keeping only rows that pass a keep-set
    /// filter, optionally trimming text-field widths to the longest value found,
    /// dropping geometry/OID/GlobalID/editor-tracking fields, and populating the
    /// keep-sets that later copies consume. Must run on the MCT (QueuedTask).
    /// </summary>
    internal static class DatasetCopier
    {
        private const int FlushInterval = 5000;
        private const int CancelCheckInterval = 5000;

        private static readonly HashSet<string> EditorTrackingFields =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "created_user", "created_date", "last_edited_user", "last_edited_date",
            };

        /// <summary>
        /// Copies the selected springs (op 0): honors the supplied selection
        /// (null/empty = all features) and builds the SiteIDs / Huc8s / Huc12s
        /// keep-sets. The output feature class keeps the springs geometry.
        /// </summary>
        internal static DatasetCopyResult CopySprings(
            Geodatabase sourceGdb,
            Geodatabase outputGdb,
            string springsFeatureClassName,
            string outputName,
            IReadOnlyList<long> selectedObjectIds,
            bool trimStrings,
            KeepSetStore store,
            CancelableProgressor progressor,
            IReadOnlyDictionary<string, int> keyFieldLengths = null,
            IReadOnlyDictionary<string, DomainDescription> domains = null)
        {
            var builds = new[]
            {
                new BuildSpec(KeepSet.SiteIDs, "SiteID"),
                new BuildSpec(KeepSet.Huc8s, "HUC"),
                new BuildSpec(KeepSet.Huc12s, "CastHUC_12"),
            };

            using FeatureClass source = OpenFeatureClassOrNull(sourceGdb, springsFeatureClassName);
            if (source == null)
            {
                return new DatasetCopyResult
                {
                    Name = outputName,
                    Skipped = true,
                    SkipReason = $"Springs feature class '{springsFeatureClassName}' not found.",
                };
            }

            QueryFilter filter = null;
            if (selectedObjectIds != null && selectedObjectIds.Count > 0)
            {
                filter = new QueryFilter { ObjectIDs = selectedObjectIds };
            }

            return CopyCore(source, isFeatureClass: true, outputGdb, outputName, filter,
                filterSet: null, filterField: null, builds, applyExclusions: false,
                trimStrings, store, progressor, keyFieldLengths, domains);
        }

        /// <summary>
        /// Copies one related table per a <see cref="CopyOperation"/>: filters by
        /// the operation's keep-set, applies the survey exclusions when requested,
        /// and populates any build keep-sets. Returns a skipped result if the
        /// source table is absent (tolerates source-schema drift).
        /// </summary>
        internal static DatasetCopyResult CopyRelated(
            Geodatabase sourceGdb,
            Geodatabase outputGdb,
            CopyOperation op,
            bool trimStrings,
            KeepSetStore store,
            CancelableProgressor progressor,
            IReadOnlyDictionary<string, int> keyFieldLengths = null,
            IReadOnlyDictionary<string, DomainDescription> domains = null)
        {
            using Table source = OpenTableOrNull(sourceGdb, op.SourceName);
            if (source == null)
            {
                return new DatasetCopyResult
                {
                    Name = op.SourceName,
                    Skipped = true,
                    SkipReason = $"Source table '{op.SourceName}' not found in the source geodatabase.",
                };
            }

            return CopyCore(source, isFeatureClass: false, outputGdb, op.SourceName, queryFilter: null,
                op.FilterSet, op.FilterField, op.Builds, op.ApplyExclusions,
                trimStrings, store, progressor, keyFieldLengths, domains,
                op.FilterSet2, op.FilterField2);
        }

        // ---------------------------------------------------------------------

        private static DatasetCopyResult CopyCore(
            Table source,
            bool isFeatureClass,
            Geodatabase outputGdb,
            string outputName,
            QueryFilter queryFilter,
            KeepSet? filterSet,
            string filterField,
            IReadOnlyList<BuildSpec> builds,
            bool applyExclusions,
            bool trimStrings,
            KeepSetStore store,
            CancelableProgressor progressor,
            IReadOnlyDictionary<string, int> keyFieldLengths,
            IReadOnlyDictionary<string, DomainDescription> domains,
            KeepSet? filterSet2 = null,
            string filterField2 = null)
        {
            TableDefinition sourceDef = source.GetDefinition();

            // Field plan: which source fields to recreate and copy values for.
            (List<FieldDescription> fieldDescriptions, List<string> copyFields) =
                PlanFields(sourceDef, trimStrings ? ScanMaxLengths(source, queryFilter, filterSet, filterField,
                                                                   applyExclusions, store, filterSet2, filterField2) : null,
                    keyFieldLengths, domains);

            // Create the output dataset.
            var schemaBuilder = new SchemaBuilder(outputGdb);
            string shapeFieldName = null;
            if (isFeatureClass)
            {
                var fcDef = (FeatureClassDefinition)sourceDef;
                var shapeDesc = new ShapeDescription(fcDef);
                schemaBuilder.Create(new FeatureClassDescription(outputName, fieldDescriptions, shapeDesc));
                shapeFieldName = fcDef.GetShapeField();
            }
            else
            {
                schemaBuilder.Create(new TableDescription(outputName, fieldDescriptions));
            }

            if (!schemaBuilder.Build())
            {
                string errors = string.Join("; ", schemaBuilder.ErrorMessages);
                throw new InvalidOperationException(
                    $"Failed to create '{outputName}' in the output geodatabase: {errors}");
            }

            // Copy rows.
            long copied;
            if (isFeatureClass)
            {
                using FeatureClass dest = outputGdb.OpenDataset<FeatureClass>(outputName);
                string destShape = dest.GetDefinition().GetShapeField();
                copied = CopyRows(source, dest, copyFields, queryFilter, filterSet, filterField,
                    applyExclusions, builds, store, progressor, shapeFieldName, destShape,
                    filterSet2, filterField2);
            }
            else
            {
                using Table dest = outputGdb.OpenDataset<Table>(outputName);
                copied = CopyRows(source, dest, copyFields, queryFilter, filterSet, filterField,
                    applyExclusions, builds, store, progressor, null, null,
                    filterSet2, filterField2);
            }

            return new DatasetCopyResult { Name = outputName, Created = true, RecordCount = copied };
        }

        /// <summary>
        /// Builds the field-description list for the output dataset, cloning every
        /// source field except geometry, OID, GlobalID, raster and the four
        /// editor-tracking fields. Coded-value domains are dropped in this version
        /// (a deferred enhancement); pass <paramref name="trimmedLengths"/> to
        /// shrink text fields to their longest actual value.
        /// </summary>
        private static (List<FieldDescription>, List<string>) PlanFields(
            TableDefinition sourceDef,
            IReadOnlyDictionary<string, int> trimmedLengths,
            IReadOnlyDictionary<string, int> keyFieldLengths,
            IReadOnlyDictionary<string, DomainDescription> domains)
        {
            var fieldDescriptions = new List<FieldDescription>();
            var copyFields = new List<string>();

            foreach (Field field in sourceDef.GetFields())
            {
                FieldType type = field.FieldType;
                if (type == FieldType.OID || type == FieldType.Geometry ||
                    type == FieldType.GlobalID || type == FieldType.Raster)
                {
                    continue;
                }

                if (EditorTrackingFields.Contains(field.Name))
                {
                    continue;
                }

                // Build the field fresh (rather than cloning the source Field) so
                // no coded-value domain is carried across. Domain replication is a
                // deferred enhancement; the fresh output geodatabase has no domains.
                var fd = new FieldDescription(field.Name, type)
                {
                    AliasName = field.AliasName,
                    IsNullable = field.IsNullable,
                    Precision = field.Precision,
                    Scale = field.Scale,
                };

                if (type == FieldType.String)
                {
                    if (keyFieldLengths != null && keyFieldLengths.TryGetValue(field.Name, out int keyLen))
                    {
                        // Relationship-key fields must match their paired field's
                        // length exactly, so they are never trimmed.
                        fd.Length = Math.Max(1, keyLen);
                    }
                    else if (trimmedLengths != null && trimmedLengths.TryGetValue(field.Name, out int maxLen))
                    {
                        fd.Length = Math.Max(1, maxLen);
                    }
                    else
                    {
                        fd.Length = Math.Max(1, field.Length);
                    }
                }

                // Re-attach the source field's coded-value / range domain (the
                // domains were replicated into the output geodatabase up front).
                if (domains != null)
                {
                    using Domain sourceDomain = field.GetDomain(null);
                    if (sourceDomain != null &&
                        domains.TryGetValue(sourceDomain.GetName(), out DomainDescription domainDescription))
                    {
                        fd.SetDomainDescription(domainDescription, null);
                    }
                }

                fieldDescriptions.Add(fd);
                copyFields.Add(field.Name);
            }

            return (fieldDescriptions, copyFields);
        }

        /// <summary>
        /// First pass for the "trim strings" option: finds the longest actual
        /// value of each text field over the rows that will be kept.
        /// </summary>
        private static IReadOnlyDictionary<string, int> ScanMaxLengths(
            Table source,
            QueryFilter queryFilter,
            KeepSet? filterSet,
            string filterField,
            bool applyExclusions,
            KeepSetStore store,
            KeepSet? filterSet2 = null,
            string filterField2 = null)
        {
            var maxLengths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var textFields = new List<string>();
            foreach (Field field in source.GetDefinition().GetFields())
            {
                if (field.FieldType == FieldType.String && !EditorTrackingFields.Contains(field.Name))
                {
                    textFields.Add(field.Name);
                    maxLengths[field.Name] = 0;
                }
            }

            if (textFields.Count == 0)
            {
                return maxLengths;
            }

            using RowCursor cursor = source.Search(queryFilter, true);
            while (cursor.MoveNext())
            {
                using Row row = cursor.Current;
                if (!Keep(row, filterSet, filterField, applyExclusions, store, filterSet2, filterField2))
                {
                    continue;
                }

                foreach (string name in textFields)
                {
                    if (row[name] is string s && s.Length > maxLengths[name])
                    {
                        maxLengths[name] = s.Length;
                    }
                }
            }

            return maxLengths;
        }

        private static long CopyRows(
            Table source,
            Table dest,
            IReadOnlyList<string> copyFields,
            QueryFilter queryFilter,
            KeepSet? filterSet,
            string filterField,
            bool applyExclusions,
            IReadOnlyList<BuildSpec> builds,
            KeepSetStore store,
            CancelableProgressor progressor,
            string sourceShapeField,
            string destShapeField,
            KeepSet? filterSet2 = null,
            string filterField2 = null)
        {
            long copied = 0;
            int sinceFlush = 0;
            int sinceCancelCheck = 0;

            using RowCursor cursor = source.Search(queryFilter, false);
            using InsertCursor insert = dest.CreateInsertCursor();

            while (cursor.MoveNext())
            {
                if (++sinceCancelCheck >= CancelCheckInterval)
                {
                    sinceCancelCheck = 0;
                    if (progressor != null && progressor.CancellationToken.IsCancellationRequested)
                    {
                        insert.Flush();
                        throw new OperationCanceledException();
                    }
                }

                using Row row = cursor.Current;
                if (!Keep(row, filterSet, filterField, applyExclusions, store, filterSet2, filterField2))
                {
                    continue;
                }

                using RowBuffer buffer = dest.CreateRowBuffer();
                foreach (string name in copyFields)
                {
                    object value = row[name];
                    if (value is DBNull)
                    {
                        value = null;
                    }

                    if (value == null && name.Equals("InventoryLevel", StringComparison.OrdinalIgnoreCase))
                    {
                        value = "Unverified";
                    }

                    buffer[name] = value;
                }

                if (destShapeField != null && row is Feature feature)
                {
                    buffer[destShapeField] = feature.GetShape();
                }

                insert.Insert(buffer);
                copied++;

                // Build the keep-sets that later datasets filter against.
                foreach (BuildSpec build in builds)
                {
                    foreach (string field in build.Fields)
                    {
                        string key = KeepSetStore.KeyString(row[field]);
                        if (key != null)
                        {
                            store.Add(build.Set, key);
                        }
                    }
                }

                if (++sinceFlush >= FlushInterval)
                {
                    insert.Flush();
                    sinceFlush = 0;
                }
            }

            insert.Flush();
            return copied;
        }

        /// <summary>
        /// Applies the keep-set filter and the survey exclusions to a single row.
        /// </summary>
        private static bool Keep(
            Row row,
            KeepSet? filterSet,
            string filterField,
            bool applyExclusions,
            KeepSetStore store,
            KeepSet? filterSet2 = null,
            string filterField2 = null)
        {
            if (filterSet.HasValue)
            {
                string key = KeepSetStore.KeyString(row[filterField]);
                if (key == null || !store.Contains(filterSet.Value, key))
                {
                    return false;
                }
            }

            // Optional second keep-set filter, AND-ed with the first.
            if (filterSet2.HasValue)
            {
                string key2 = KeepSetStore.KeyString(row[filterField2]);
                if (key2 == null || !store.Contains(filterSet2.Value, key2))
                {
                    return false;
                }
            }

            if (applyExclusions)
            {
                string site = KeepSetStore.KeyString(row["SiteID"]);
                if (site != null && store.ExcludeSiteIds.Contains(site))
                {
                    return false;
                }

                string survey = KeepSetStore.KeyString(row["SurveyID"]);
                if (survey != null && store.ExcludeSurveyIds.Contains(survey))
                {
                    return false;
                }
            }

            return true;
        }

        private static FeatureClass OpenFeatureClassOrNull(Geodatabase gdb, string name)
        {
            try
            {
                return gdb.OpenDataset<FeatureClass>(name);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Table OpenTableOrNull(Geodatabase gdb, string name)
        {
            try
            {
                return gdb.OpenDataset<Table>(name);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
