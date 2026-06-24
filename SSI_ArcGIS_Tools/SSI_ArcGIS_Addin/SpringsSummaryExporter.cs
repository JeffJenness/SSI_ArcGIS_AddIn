using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Builds the denormalized "summary" springs feature class (one row per
    /// spring) from a subset geodatabase: copies key spring attributes and rolls
    /// up related survey data — flow statistics + trend regression, water-quality
    /// statistics (specific conductance, pH, water temperature, alkalinity),
    /// taxon counts (vert/invert/flora at species/genus/family/order), mean
    /// surveyed area, and a representative photo. Port of the Phase-2a core of
    /// the legacy RunCreateFeatureServiceFiles_OnDemand. Runs on the MCT.
    /// </summary>
    internal static class SpringsSummaryExporter
    {
        // Water-quality parameter codes (CastWQParameter), matched exactly.
        private const string AlkalinityCode = "Alkalinity, Total (mg/L)";
        private const string SpecCondFieldCode = "Specific conductance (field) (uS/cm)";
        private const string SpecCondLabCode = "Specific conductance (lab) umhos/cm";
        private const string PhFieldCode = "pH (field)";
        private const string PhLabCode = "pH (lab)";
        private const string WaterTempCode = "Temperature, water C";

        // Source spring fields copied straight into the summary feature class.
        private static readonly string[] SourceFields =
        {
            "SiteID", "ShortName", "CastAccessDescript", "CastSiteDescription", "SpringType1",
            "SpringType2", "InventoryLevel", "County", "StateProvince", "USGS_Quad", "LandUnit",
            "LandUnitDetail", "SurveyCount", "LatitudeDD", "LongitudeDD", "ElevationM",
            "CastImageHyperlink", "CastSketchHyperlink",
        };

        // Full taxon field sets for the by_Site / by_Survey taxa tables. Counting
        // indices are unchanged from the lookup loads: vert/invert order=2,family=3,
        // genus=4,species=5; flora order=-1,family=6,genus=8,species=1 (FloraSpecies -
        // the legacy counts on "FloraSpecies", which is populated, not the bare
        // "Species" epithet at index 9 which is frequently blank).
        private static readonly string[] TaxaVertFields =
        {
            "TID", "FaunaCommonName", "FaunaOrder", "FaunaFamily", "FaunaGenus", "FaunaSpecies",
            "FaunaFullName", "EndemismLevel", "SpringLifeHistory", "AquaticStatus",
        };

        private static readonly string[] TaxaInvertFields =
        {
            "TID", "Fullname", "Order_", "Family", "Genus", "Species", "Subspecies", "CommonName",
            "EndemismLevel", "SpringLifeHistory", "AquaticStatus",
        };

        private static readonly string[] TaxaFloraFields =
        {
            "TID", "FloraSpecies", "FloraCommonName", "GrowthHabit_USDA", "CastStateAndProvince",
            "Category", "Family", "FamilyCommonName", "Genus", "Species", "Subspecies", "Variety",
            "EndemismLevel", "SpringHabitatUse", "SpringLifeHistory", "AquaticStatus",
        };

        private static readonly string[] SolarFields =
        {
            "SiteID", "Latitude", "Ratio",
            "Jan_Rise", "Feb_Rise", "Mar_Rise", "Apr_Rise", "May_Rise", "Jun_Rise", "Jul_Rise",
            "Aug_Rise", "Sep_Rise", "Oct_Rise", "Nov_Rise", "Dec_Rise",
            "Jan_Set", "Feb_Set", "Mar_Set", "Apr_Set", "May_Set", "Jun_Set", "Jul_Set",
            "Aug_Set", "Sep_Set", "Oct_Set", "Nov_Set", "Dec_Set",
            "Potential_Solar_Spring", "Potential_Solar_Summer", "Potential_Solar_Fall",
            "Potential_Solar_Winter", "Potential_Solar_Total",
            "Percent_Solar_Spring", "Percent_Solar_Summer", "Percent_Solar_Fall",
            "Percent_Solar_Winter", "Percent_Solar_Total",
        };

        private static readonly string[] SurveySourceFields =
        {
            "SiteID", "SurveyID", "SurveyDate", "Project", "TotalAreaSQM", "CastSurveyors",
        };

        internal sealed class SummaryResult
        {
            internal string Name { get; init; }
            internal bool Created { get; init; }
            internal string SkipReason { get; init; }
            internal long RecordCount { get; init; }
            internal int SupportingTableCount { get; init; }
            internal int RelationshipClassCount { get; init; }
            internal IReadOnlyList<string> CreatedDatasets { get; init; }
        }

        internal static SummaryResult Export(
            Geodatabase gdb,
            string springsFeatureClassName,
            string summaryFeatureClassName,
            CancelableProgressor progressor)
        {
            FeatureClass springs = TryOpenFeatureClass(gdb, springsFeatureClassName);
            if (springs == null)
            {
                return new SummaryResult
                {
                    Name = summaryFeatureClassName,
                    SkipReason = $"Springs feature class '{springsFeatureClassName}' not found.",
                };
            }

            using (springs)
            {
                // Load the related tables (each tolerant of an absent table).
                var surveysBySite = LoadGrouped(gdb, "tbl_Surveys", "SiteID",
                    "SiteID", "SurveyID", "SurveyDate", "Project", "TotalAreaSQM", "CastSurveyors");
                var flowBySurvey = LoadGrouped(gdb, "tbl_flow", "SurveyID", "MeasuredFlowLS", "SurveyID");
                var wqBySurvey = LoadGrouped(gdb, "tbl_WQData", "SurveyID", "CastWQParameter", "WQMeasurement", "SurveyID");
                var vertBySurvey = LoadGrouped(gdb, "tbl_VertSurvey", "SurveyID", "TID", "SurveyID");
                var invertBySurvey = LoadGrouped(gdb, "tbl_InvertSampling", "SurveyID", "TID", "SurveyID");
                var polySurveyBySurvey = LoadGrouped(gdb, "tbl_PolygonSurvey", "SurveyID", "SurveyPolygonAutoID", "SurveyID");
                var polyFloraByPoly = LoadGrouped(gdb, "tbl_PolygonFlora", "SurveyPolygonID", "SurveyPolygonID", "TID");
                var gdeDomVegBySurvey = LoadGrouped(gdb, "gde_domveglevel1", "SurveyID",
                    "SurveyID", "TreeSpeciesTID", "ShrubSpeciesTID", "GraminoidSpeciesTID", "ForbSpeciesTID", "AquaticSpeciesTID");
                var gdeOtherVegBySurvey = LoadGrouped(gdb, "gde_otherveg", "SurveyID", "SurveyID", "TID");
                var imagesBySurvey = LoadGrouped(gdb, "tbl_images", "SurveyID", "SurveyID", "CastImageHyperlink", "CastImageType");

                var taxaVert = LoadSingle(gdb, "tlu_TaxaVert", "TID",
                    "TID", "FaunaCommonName", "FaunaOrder", "FaunaFamily", "FaunaGenus", "FaunaSpecies");
                var taxaInvert = LoadSingle(gdb, "tlu_TaxaInvert", "TID",
                    "TID", "Fullname", "Order_", "Family", "Genus", "Species");
                var taxaFlora = LoadSingle(gdb, "tlu_TaxaFlora", "TID",
                    "TID", "FloraSpecies", "FloraCommonName", "GrowthHabit_USDA", "CastStateAndProvince",
                    "Category", "Family", "FamilyCommonName", "Genus", "Species");

                // Create the output feature class.
                CreateSummarySchema(gdb, springs, summaryFeatureClassName);

                var context = new RollupContext(
                    surveysBySite, flowBySurvey, wqBySurvey, vertBySurvey, invertBySurvey,
                    polySurveyBySurvey, polyFloraByPoly, gdeDomVegBySurvey, gdeOtherVegBySurvey,
                    imagesBySurvey, taxaVert, taxaInvert, taxaFlora);

                long written = WriteSummaryRows(gdb, springs, summaryFeatureClassName, context, progressor);

                // Phase 2b: the supporting tables (_Surveys, _Solar_by_Site, and the
                // six taxa _by_Site / _by_Survey tables), linked back to the springs
                // and surveys via Site_GlobalID / Survey_GlobalID.
                var supporting = BuildSupportingTables(gdb, summaryFeatureClassName, context, progressor);

                string b = summaryFeatureClassName;
                var createdDatasets = new List<string>
                {
                    b,
                    b + "_Surveys",
                    b + "_Solar_by_Site_Append",
                    b + "_TaxaVert_by_Site", b + "_TaxaVert_by_Survey",
                    b + "_TaxaInvert_by_Site", b + "_TaxaInvert_by_Survey",
                    b + "_TaxaFlora_by_Site", b + "_TaxaFlora_by_Survey",
                };

                return new SummaryResult
                {
                    Name = summaryFeatureClassName,
                    Created = true,
                    RecordCount = written,
                    SupportingTableCount = supporting.Tables,
                    RelationshipClassCount = supporting.RelationshipClasses,
                    CreatedDatasets = createdDatasets,
                };
            }
        }

        /// <summary>
        /// Every dataset name this summary build will create for a given summary
        /// feature class name: the summary feature class, its eight supporting
        /// tables, and the eight relationship classes. Used by the Export
        /// Geodatabase pre-flight path-length check. MUST stay in sync with
        /// <see cref="BuildSupportingTables"/> and
        /// <see cref="CreateSummaryRelationshipClasses"/>.
        /// </summary>
        internal static IReadOnlyList<string> PredictedDatasetNames(string summaryName)
        {
            return new[]
            {
                summaryName,
                summaryName + "_Surveys",
                summaryName + "_Solar_by_Site_Append",
                summaryName + "_TaxaVert_by_Site", summaryName + "_TaxaVert_by_Survey",
                summaryName + "_TaxaInvert_by_Site", summaryName + "_TaxaInvert_by_Survey",
                summaryName + "_TaxaFlora_by_Site", summaryName + "_TaxaFlora_by_Survey",
                summaryName + "_RC_Surveys_by_Site",
                summaryName + "_RC_Solar_by_Site",
                summaryName + "_RC_Sites_to_Verts",
                summaryName + "_RC_Sites_to_Inverts",
                summaryName + "_RC_Sites_to_Flora",
                summaryName + "_RC_Surveys_to_Verts",
                summaryName + "_RC_Surveys_to_Inverts",
                summaryName + "_RC_Surveys_to_Flora",
            };
        }

        // --- schema ----------------------------------------------------------

        private static void CreateSummarySchema(Geodatabase gdb, FeatureClass springs, string name)
        {
            FeatureClassDefinition springsDef = springs.GetDefinition();
            var fields = new List<FieldDescription>();

            // Copied source fields (those present on the springs FC).
            foreach (string fieldName in SourceFields)
            {
                int idx = springsDef.FindField(fieldName);
                if (idx >= 0)
                {
                    fields.Add(CloneField(springsDef.GetFields()[idx]));
                }
            }

            // Computed rollup fields. Within each measurement block the Count
            // field is placed before the Mean (per request).
            var computed = new (string Name, FieldType Type)[]
            {
                ("Flow_Count", FieldType.Integer), ("Flow_Mean", FieldType.Double), ("Flow_Stand_Dev", FieldType.Double),
                ("Flow_Sample_Range_Days", FieldType.Integer), ("Flow_Regression_Slope", FieldType.Double),
                ("Flow_Regression_R2", FieldType.Double), ("Flow_Regression_AdjR2", FieldType.Double),
                ("Spec_Cond_Count", FieldType.Integer), ("Spec_Cond_Mean", FieldType.Double), ("Spec_Cond_Stand_Dev", FieldType.Double),
                ("pH_Count", FieldType.Integer), ("pH_Mean", FieldType.Double), ("pH_Stand_Dev", FieldType.Double),
                ("Water_Temp_Count", FieldType.Integer), ("Water_Temp_Mean", FieldType.Double), ("Water_Temp_Stand_Dev", FieldType.Double),
                ("Alkalinity_Count", FieldType.Integer), ("Alkalinity_Mean", FieldType.Double), ("Alkalinity_Stand_Dev", FieldType.Double),
                ("Vert_Species_Count", FieldType.Integer), ("Vert_Genus_Count", FieldType.Integer),
                ("Vert_Family_Count", FieldType.Integer), ("Vert_Order_Count", FieldType.Integer),
                ("Invert_Species_Count", FieldType.Integer), ("Invert_Genus_Count", FieldType.Integer),
                ("Invert_Family_Count", FieldType.Integer), ("Invert_Order_Count", FieldType.Integer),
                ("Flora_Species_Count", FieldType.Integer), ("Flora_Genus_Count", FieldType.Integer),
                ("Flora_Family_Count", FieldType.Integer),
                ("TotalAreaSQM", FieldType.Double),
            };

            foreach (var (fieldName, fieldType) in computed)
            {
                fields.Add(new FieldDescription(fieldName, fieldType) { IsNullable = true });
            }

            fields.Add(FieldDescription.CreateStringField("Image_Link", 500));
            fields.Add(FieldDescription.CreateStringField("Image_Caption", 1000));
            fields.Add(FieldDescription.CreateGlobalIDField());

            var shape = new ShapeDescription(springsDef);
            var schemaBuilder = new SchemaBuilder(gdb);
            schemaBuilder.Create(new FeatureClassDescription(name, fields, shape));
            if (!schemaBuilder.Build())
            {
                throw new InvalidOperationException(
                    $"Failed to create summary feature class '{name}': {string.Join("; ", schemaBuilder.ErrorMessages)}");
            }
        }

        private static FieldDescription CloneField(Field field)
        {
            var fd = new FieldDescription(field.Name, field.FieldType)
            {
                AliasName = field.AliasName,
                IsNullable = field.IsNullable,
                Precision = field.Precision,
                Scale = field.Scale,
            };
            if (field.FieldType == FieldType.String)
            {
                fd.Length = Math.Max(1, field.Length);
            }

            return fd;
        }

        // --- writing ---------------------------------------------------------

        private static long WriteSummaryRows(
            Geodatabase gdb, FeatureClass springs, string summaryName,
            RollupContext ctx, CancelableProgressor progressor)
        {
            long written = 0;
            FeatureClassDefinition springsDef = springs.GetDefinition();
            string shapeField = springsDef.GetShapeField();

            // Source fields actually present, for value copy.
            List<string> presentSourceFields = SourceFields.Where(f => springsDef.FindField(f) >= 0).ToList();

            if (progressor != null)
            {
                progressor.Max = (uint)Math.Max(1, springs.GetCount());
                progressor.Value = 0;
            }

            using FeatureClass summary = gdb.OpenDataset<FeatureClass>(summaryName);
            using InsertCursor insert = summary.CreateInsertCursor();
            using RowCursor cursor = springs.Search(null, false);

            while (cursor.MoveNext())
            {
                if (progressor != null && progressor.CancellationToken.IsCancellationRequested)
                {
                    insert.Flush();
                    throw new OperationCanceledException();
                }

                using var feature = (Feature)cursor.Current;
                using RowBuffer buffer = summary.CreateRowBuffer();

                foreach (string f in presentSourceFields)
                {
                    object v = feature[f];
                    buffer[f] = v is DBNull ? null : v;
                }

                buffer[shapeField] = feature.GetShape();

                string siteKey = KeepSetStore.KeyString(feature["SiteID"]);
                // The image caption uses the full site name (legacy strRunningSiteName),
                // not the abbreviated ShortName.
                string siteName = AsString(SafeGet(feature, "SiteName"));
                ComputeAndWrite(buffer, siteKey, siteKey, siteName, ctx);

                insert.Insert(buffer);
                written++;

                if (progressor != null)
                {
                    progressor.Value += 1;
                }
            }

            insert.Flush();
            return written;
        }

        private static void ComputeAndWrite(
            RowBuffer buffer, string siteKey, string siteIdText, string siteName, RollupContext ctx)
        {
            List<object[]> surveys = ctx.SurveysBySite.TryGetValue(siteKey ?? string.Empty, out var s)
                ? s : new List<object[]>();

            buffer["SurveyCount"] = surveys.Count;

            var flowMeans = new List<double>();
            var regressionX = new List<double>();
            var regressionY = new List<double>();
            var specCond = new List<double>();
            var ph = new List<double>();
            var temp = new List<double>();
            var alk = new List<double>();
            double areaSum = 0;
            int areaCount = 0;

            var siteVerts = new Dictionary<string, object[]>();
            var siteInverts = new Dictionary<string, object[]>();
            var siteFlora = new Dictionary<string, object[]>();

            string imageLink = string.Empty;
            string imageCaption = string.Empty;

            foreach (object[] survey in surveys)
            {
                string surveyKey = KeepSetStore.KeyString(survey[1]); // SurveyID

                // Flow: mean of this survey's measurements; track date for regression.
                double? surveyFlow = SurveyMean(ctx.FlowBySurvey, surveyKey, 0);
                if (surveyFlow.HasValue)
                {
                    flowMeans.Add(surveyFlow.Value);
                    if (survey[2] is DateTime date)
                    {
                        regressionX.Add(date.ToOADate());
                        regressionY.Add(surveyFlow.Value);
                    }
                }

                // Water quality per parameter.
                AddWaterQuality(ctx.WqBySurvey, surveyKey, specCond, ph, temp, alk);

                // Area (survey TotalAreaSQM, index 4); 0 treated as missing.
                if (TryDouble(survey[4], out double area) && area != 0)
                {
                    areaSum += area;
                    areaCount++;
                }

                // Taxa.
                CollectVerts(ctx, surveyKey, siteVerts);
                CollectInverts(ctx, surveyKey, siteInverts);
                CollectFlora(ctx, surveyKey, siteFlora);

                // Representative image (last one wins).
                if (ctx.ImagesBySurvey.TryGetValue(surveyKey ?? string.Empty, out var imgs))
                {
                    foreach (object[] img in imgs)
                    {
                        string type = AsString(img[2]);
                        if (type != null && type.Equals("representative", StringComparison.OrdinalIgnoreCase))
                        {
                            imageLink = AsString(img[1]) ?? string.Empty;
                            imageCaption = BuildCaption(siteName, siteIdText, AsString(survey[5]), survey[2]);
                        }
                    }
                }
            }

            WriteMeanBlock(buffer, flowMeans, "Flow_Mean", "Flow_Stand_Dev", "Flow_Count");
            WriteFlowRegression(buffer, flowMeans.Count, regressionX, regressionY);
            WriteMeanBlock(buffer, specCond, "Spec_Cond_Mean", "Spec_Cond_Stand_Dev", "Spec_Cond_Count");
            WriteMeanBlock(buffer, ph, "pH_Mean", "pH_Stand_Dev", "pH_Count");
            WriteMeanBlock(buffer, temp, "Water_Temp_Mean", "Water_Temp_Stand_Dev", "Water_Temp_Count");
            WriteMeanBlock(buffer, alk, "Alkalinity_Mean", "Alkalinity_Stand_Dev", "Alkalinity_Count");

            buffer["TotalAreaSQM"] = areaCount > 0 ? areaSum / areaCount : (object)null;

            var vertCounts = SpeciesCounter.Count(siteVerts.Values.ToList(), 2, 3, 4, 5);
            var invertCounts = SpeciesCounter.Count(siteInverts.Values.ToList(), 2, 3, 4, 5);
            var floraCounts = SpeciesCounter.Count(siteFlora.Values.ToList(), -1, 6, 8, 1);

            buffer["Vert_Species_Count"] = vertCounts.Species;
            buffer["Vert_Genus_Count"] = vertCounts.Genus;
            buffer["Vert_Family_Count"] = vertCounts.Family;
            buffer["Vert_Order_Count"] = vertCounts.Order;
            buffer["Invert_Species_Count"] = invertCounts.Species;
            buffer["Invert_Genus_Count"] = invertCounts.Genus;
            buffer["Invert_Family_Count"] = invertCounts.Family;
            buffer["Invert_Order_Count"] = invertCounts.Order;
            buffer["Flora_Species_Count"] = floraCounts.Species;
            buffer["Flora_Genus_Count"] = floraCounts.Genus;
            buffer["Flora_Family_Count"] = floraCounts.Family;

            buffer["Image_Link"] = imageLink;
            buffer["Image_Caption"] = imageCaption;
        }

        private static void WriteMeanBlock(
            RowBuffer buffer, List<double> values, string meanField, string sdField, string countField)
        {
            if (values.Count > 0)
            {
                SummaryStats.MeanResult r = SummaryStats.MeanAndStdDev(values);
                buffer[meanField] = r.Mean;
                buffer[sdField] = r.SampleStdDev;
                buffer[countField] = r.Count;
            }
            else
            {
                buffer[meanField] = null;
                buffer[sdField] = null;
                buffer[countField] = null;
            }
        }

        private static void WriteFlowRegression(RowBuffer buffer, int flowCount, List<double> x, List<double> y)
        {
            if (flowCount <= 0 || x.Count < 2)
            {
                // Legacy: null when there is NO flow data at all, but 0 when flow
                // exists yet the range is uncomputable (<2 dated samples). Regression
                // stats stay null either way.
                buffer["Flow_Sample_Range_Days"] = flowCount <= 0 ? (object)null : 0;
                buffer["Flow_Regression_Slope"] = null;
                buffer["Flow_Regression_R2"] = null;
                buffer["Flow_Regression_AdjR2"] = null;
                return;
            }

            // Sort the (day, flow) pairs by day so the range is correct.
            var pairs = Enumerable.Range(0, x.Count).Select(i => (X: x[i], Y: y[i]))
                .OrderBy(p => p.X).ToList();
            var xs = pairs.Select(p => p.X).ToList();
            var ys = pairs.Select(p => p.Y).ToList();

            buffer["Flow_Sample_Range_Days"] = (int)Math.Round(xs[xs.Count - 1] - xs[0]);

            SummaryStats.RegressionResult reg = SummaryStats.Regression(xs, ys);
            buffer["Flow_Regression_Slope"] = reg.Slope.HasValue ? reg.Slope.Value : (object)null;
            buffer["Flow_Regression_R2"] = reg.RSquared;
            buffer["Flow_Regression_AdjR2"] = reg.AdjustedRSquared.HasValue ? reg.AdjustedRSquared.Value : (object)null;
        }

        // --- per-survey gather ----------------------------------------------

        private static double? SurveyMean(
            Dictionary<string, List<object[]>> bySurvey, string surveyKey, int valueIndex)
        {
            if (surveyKey == null || !bySurvey.TryGetValue(surveyKey, out var rows))
            {
                return null;
            }

            var values = new List<double>();
            foreach (object[] row in rows)
            {
                if (TryDouble(row[valueIndex], out double v))
                {
                    values.Add(v);
                }
            }

            return values.Count > 0 ? SummaryStats.MeanAndStdDev(values).Mean : (double?)null;
        }

        /// <summary>
        /// Normalizes a CastWQParameter for matching: the specific-conductance code
        /// is stored with the micro sign (U+00B5) or Greek mu (U+03BC) in the data
        /// (e.g. "…(µS/cm)"), but the SpecCond*Code constants use an ASCII 'u'.
        /// Folding both micro variants to 'u' makes the match succeed regardless of
        /// which character the source uses. (Other parameters contain no µ.)
        /// </summary>
        private static string NormalizeWqParameter(string parameter)
        {
            return parameter?.Replace((char)0x00B5, 'u').Replace((char)0x03BC, 'u');
        }

        private static void AddWaterQuality(
            Dictionary<string, List<object[]>> wqBySurvey, string surveyKey,
            List<double> specCond, List<double> ph, List<double> temp, List<double> alk)
        {
            if (surveyKey == null || !wqBySurvey.TryGetValue(surveyKey, out var rows))
            {
                return;
            }

            var sc = new List<double>();
            var p = new List<double>();
            var t = new List<double>();
            var a = new List<double>();

            foreach (object[] row in rows)
            {
                string param = AsString(row[0]); // CastWQParameter (exact match)
                if (param == null || !TryDouble(row[1], out double measure))
                {
                    continue;
                }

                switch (NormalizeWqParameter(param))
                {
                    case AlkalinityCode: a.Add(measure); break;
                    case SpecCondFieldCode:
                    case SpecCondLabCode: sc.Add(measure); break;
                    case PhFieldCode:
                    case PhLabCode: p.Add(measure); break;
                    case WaterTempCode: t.Add(measure); break;
                }
            }

            if (sc.Count > 0) specCond.Add(SummaryStats.MeanAndStdDev(sc).Mean);
            if (p.Count > 0) ph.Add(SummaryStats.MeanAndStdDev(p).Mean);
            if (t.Count > 0) temp.Add(SummaryStats.MeanAndStdDev(t).Mean);
            if (a.Count > 0) alk.Add(SummaryStats.MeanAndStdDev(a).Mean);
        }

        private static void CollectVerts(RollupContext ctx, string surveyKey, Dictionary<string, object[]> site)
        {
            if (surveyKey == null || !ctx.VertBySurvey.TryGetValue(surveyKey, out var rows))
            {
                return;
            }

            foreach (object[] row in rows)
            {
                AddTaxon(ctx.TaxaVert, row[0], site);
            }
        }

        private static void CollectInverts(RollupContext ctx, string surveyKey, Dictionary<string, object[]> site)
        {
            if (surveyKey == null || !ctx.InvertBySurvey.TryGetValue(surveyKey, out var rows))
            {
                return;
            }

            foreach (object[] row in rows)
            {
                AddTaxon(ctx.TaxaInvert, row[0], site);
            }
        }

        private static void CollectFlora(RollupContext ctx, string surveyKey, Dictionary<string, object[]> site)
        {
            if (surveyKey == null)
            {
                return;
            }

            // Polygon flora: survey -> polygon(s) -> flora TID(s).
            if (ctx.PolySurveyBySurvey.TryGetValue(surveyKey, out var polys))
            {
                foreach (object[] poly in polys)
                {
                    string polyKey = KeepSetStore.KeyString(poly[0]); // SurveyPolygonAutoID
                    if (polyKey != null && ctx.PolyFloraByPoly.TryGetValue(polyKey, out var floraRows))
                    {
                        foreach (object[] fr in floraRows)
                        {
                            AddTaxon(ctx.TaxaFlora, fr[1], site); // TID
                        }
                    }
                }
            }

            // GDE dominant vegetation: five species TID columns (indices 1..5).
            if (ctx.GdeDomVegBySurvey.TryGetValue(surveyKey, out var domVeg))
            {
                foreach (object[] dv in domVeg)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        AddTaxon(ctx.TaxaFlora, dv[i], site);
                    }
                }
            }

            // GDE other vegetation.
            if (ctx.GdeOtherVegBySurvey.TryGetValue(surveyKey, out var otherVeg))
            {
                foreach (object[] ov in otherVeg)
                {
                    AddTaxon(ctx.TaxaFlora, ov[1], site); // TID
                }
            }
        }

        private static void AddTaxon(Dictionary<string, object[]> taxa, object tidValue, Dictionary<string, object[]> site)
        {
            string tid = KeepSetStore.KeyString(tidValue);
            if (tid == null || site.ContainsKey(tid))
            {
                return;
            }

            if (taxa.TryGetValue(tid, out object[] record))
            {
                site[tid] = record;
            }
        }

        private static string BuildCaption(string siteName, string siteId, string surveyors, object surveyDate)
        {
            string caption =
                "Image from Springs Stewardship Institute Online Database (Springs Online, https://springsdata.org/).  " +
                $"Survey conducted at {siteName} [Site ID {siteId}]";

            if (!string.IsNullOrWhiteSpace(surveyors))
            {
                caption += $", by {surveyors}";
            }

            // Long date, matching the legacy Format(date, "long date") -> e.g.
            // "Thursday, September 21, 2017".
            if (surveyDate is DateTime date && date.Year != 1000)
            {
                caption += " on " + date.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
            }

            return caption + ".";
        }

        // --- supporting tables (Phase 2b) ------------------------------------

        /// <summary>
        /// Creates and populates the eight supporting tables, linking each back to
        /// its parent spring/survey via Site_GlobalID / Survey_GlobalID (read from
        /// the parents' auto-assigned GlobalIDs). Returns the number of tables made.
        /// </summary>
        private static (int Tables, int RelationshipClasses) BuildSupportingTables(
            Geodatabase gdb, string baseName, RollupContext ctx, CancelableProgressor progressor)
        {
            // Full taxon records for output (the 2a loads carry only the counting fields).
            var taxaVert = LoadSingle(gdb, "tlu_TaxaVert", "TID", TaxaVertFields);
            var taxaInvert = LoadSingle(gdb, "tlu_TaxaInvert", "TID", TaxaInvertFields);
            var taxaFlora = LoadSingle(gdb, "tlu_TaxaFlora", "TID", TaxaFloraFields);
            var solarBySite = LoadGrouped(gdb, "tbl_Solar", "SiteID", SolarFields);

            if (progressor != null && progressor.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            // SiteID -> springs GlobalID (the parent key for site-level children).
            Dictionary<string, object> siteGuid = ReadGuidMap(gdb, baseName, "SiteID");

            int tables = 0;

            // _Surveys (one row per survey, with per-survey stats).
            string surveysName = baseName + "_Surveys";
            CreateSurveysSchema(gdb, surveysName);
            PopulateSurveys(gdb, surveysName, ctx, siteGuid, taxaVert, taxaInvert, taxaFlora);
            tables++;

            Dictionary<string, object> surveyGuid = ReadGuidMap(gdb, surveysName, "SurveyID");

            // _Solar_by_Site.
            CreateSolarSchema(gdb, baseName + "_Solar_by_Site_Append");
            PopulateSolar(gdb, baseName + "_Solar_by_Site_Append", solarBySite, siteGuid);
            tables++;

            // Six taxa tables. by_Site lists each taxon once per spring; by_Survey
            // lists each taxon per survey.
            tables += BuildTaxaPair(gdb, baseName, "TaxaVert", "tlu_TaxaVert", TaxaVertFields,
                taxaVert, ctx, siteGuid, surveyGuid, (c, sk) => VertTids(c, sk));
            tables += BuildTaxaPair(gdb, baseName, "TaxaInvert", "tlu_TaxaInvert", TaxaInvertFields,
                taxaInvert, ctx, siteGuid, surveyGuid, (c, sk) => InvertTids(c, sk));
            tables += BuildTaxaPair(gdb, baseName, "TaxaFlora", "tlu_TaxaFlora", TaxaFloraFields,
                taxaFlora, ctx, siteGuid, surveyGuid, (c, sk) => FloraTids(c, sk));

            // Relationship classes between the summary datasets (GlobalID-keyed).
            int relationshipClasses = CreateSummaryRelationshipClasses(gdb, baseName);

            return (tables, relationshipClasses);
        }

        /// <summary>
        /// Creates the eight GlobalID-keyed relationship classes that link the
        /// summary datasets, matching the legacy varRCWorkOrder
        /// (SSI_FeatureServiceFunctions.bas:1734). All are one-to-many with the
        /// parent's GlobalID as origin primary key and the child's
        /// Site_GlobalID / Survey_GlobalID as origin foreign key.
        /// </summary>
        private static int CreateSummaryRelationshipClasses(Geodatabase gdb, string baseName)
        {
            string surveys = baseName + "_Surveys";
            var specs = new (string Name, bool OriginIsFeatureClass, string Origin, string Destination, string ForeignKey)[]
            {
                ($"{baseName}_RC_Surveys_by_Site", true, baseName, surveys, "Site_GlobalID"),
                ($"{baseName}_RC_Solar_by_Site", true, baseName, baseName + "_Solar_by_Site_Append", "Site_GlobalID"),
                ($"{baseName}_RC_Sites_to_Verts", true, baseName, baseName + "_TaxaVert_by_Site", "Site_GlobalID"),
                ($"{baseName}_RC_Sites_to_Inverts", true, baseName, baseName + "_TaxaInvert_by_Site", "Site_GlobalID"),
                ($"{baseName}_RC_Sites_to_Flora", true, baseName, baseName + "_TaxaFlora_by_Site", "Site_GlobalID"),
                ($"{baseName}_RC_Surveys_to_Verts", false, surveys, baseName + "_TaxaVert_by_Survey", "Survey_GlobalID"),
                ($"{baseName}_RC_Surveys_to_Inverts", false, surveys, baseName + "_TaxaInvert_by_Survey", "Survey_GlobalID"),
                ($"{baseName}_RC_Surveys_to_Flora", false, surveys, baseName + "_TaxaFlora_by_Survey", "Survey_GlobalID"),
            };

            var schemaBuilder = new SchemaBuilder(gdb);
            foreach (var spec in specs)
            {
                var description = new RelationshipClassDescription(
                    spec.Name,
                    OpenDescription(gdb, spec.Origin, spec.OriginIsFeatureClass),
                    OpenDescription(gdb, spec.Destination, isFeatureClass: false),
                    RelationshipCardinality.OneToMany,
                    "GlobalID",
                    spec.ForeignKey)
                {
                    RelationshipMessageDirection = RelationshipMessageDirection.Forward,
                    ForwardPathLabel = spec.Destination,
                    BackwardPathLabel = spec.Origin,
                };
                schemaBuilder.Create(description);
            }

            if (!schemaBuilder.Build())
            {
                throw new InvalidOperationException(
                    "Failed to create summary relationship classes: " +
                    string.Join("; ", schemaBuilder.ErrorMessages));
            }

            return specs.Length;
        }

        private static TableDescription OpenDescription(Geodatabase gdb, string name, bool isFeatureClass)
        {
            if (isFeatureClass)
            {
                using FeatureClass fc = gdb.OpenDataset<FeatureClass>(name);
                return new FeatureClassDescription(fc.GetDefinition());
            }

            using Table table = gdb.OpenDataset<Table>(name);
            return new TableDescription(table.GetDefinition());
        }

        private static int BuildTaxaPair(
            Geodatabase gdb, string baseName, string label, string sourceTable, string[] fields,
            Dictionary<string, object[]> taxa, RollupContext ctx,
            Dictionary<string, object> siteGuid, Dictionary<string, object> surveyGuid,
            Func<RollupContext, string, List<object>> tidsForSurvey)
        {
            string bySiteName = $"{baseName}_{label}_by_Site";
            string bySurveyName = $"{baseName}_{label}_by_Survey";

            CreateTaxaSchema(gdb, bySiteName, sourceTable, fields, bySite: true);
            CreateTaxaSchema(gdb, bySurveyName, sourceTable, fields, bySite: false);

            PopulateTaxa(gdb, bySiteName, true, fields, taxa, ctx, siteGuid, surveyGuid, tidsForSurvey);
            PopulateTaxa(gdb, bySurveyName, false, fields, taxa, ctx, siteGuid, surveyGuid, tidsForSurvey);
            return 2;
        }

        // --- schema creators -------------------------------------------------

        private static void CreateSurveysSchema(Geodatabase gdb, string name)
        {
            List<FieldDescription> fields = CloneFieldsFromSource(gdb, "tbl_Surveys", SurveySourceFields);

            // TotalAreaSQM is stored as text in the source; the summary uses Double.
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Name.Equals("TotalAreaSQM", StringComparison.OrdinalIgnoreCase))
                {
                    fields[i] = new FieldDescription("TotalAreaSQM", FieldType.Double) { IsNullable = true };
                }
            }

            // Within each measurement block the Count precedes the Mean (per request).
            var computed = new (string Name, FieldType Type)[]
            {
                ("Flow_Count", FieldType.Integer), ("Flow_Mean", FieldType.Double),
                ("Flow_Sum", FieldType.Double), ("Flow_Stand_Dev", FieldType.Double),
                ("Spec_Cond_Count", FieldType.Integer), ("Spec_Cond_Mean", FieldType.Double), ("Spec_Cond_Stand_Dev", FieldType.Double),
                ("pH_Count", FieldType.Integer), ("pH_Mean", FieldType.Double), ("pH_Stand_Dev", FieldType.Double),
                ("Water_Temp_Count", FieldType.Integer), ("Water_Temp_Mean", FieldType.Double), ("Water_Temp_Stand_Dev", FieldType.Double),
                ("Alkalinity_Count", FieldType.Integer), ("Alkalinity_Mean", FieldType.Double), ("Alkalinity_Stand_Dev", FieldType.Double),
                ("Vert_Species_Count", FieldType.Integer), ("Vert_Genus_Count", FieldType.Integer),
                ("Vert_Family_Count", FieldType.Integer), ("Vert_Order_Count", FieldType.Integer),
                ("Invert_Species_Count", FieldType.Integer), ("Invert_Genus_Count", FieldType.Integer),
                ("Invert_Family_Count", FieldType.Integer), ("Invert_Order_Count", FieldType.Integer),
                ("Flora_Species_Count", FieldType.Integer), ("Flora_Genus_Count", FieldType.Integer),
                ("Flora_Family_Count", FieldType.Integer),
            };

            foreach (var (fieldName, fieldType) in computed)
            {
                fields.Add(new FieldDescription(fieldName, fieldType) { IsNullable = true });
            }

            fields.Add(new FieldDescription("Site_GlobalID", FieldType.GUID) { IsNullable = true });
            fields.Add(FieldDescription.CreateGlobalIDField());

            BuildTable(gdb, name, fields);
        }

        private static void CreateSolarSchema(Geodatabase gdb, string name)
        {
            List<FieldDescription> fields = CloneFieldsFromSource(gdb, "tbl_Solar", SolarFields);
            fields.Add(new FieldDescription("Site_GlobalID", FieldType.GUID) { IsNullable = true });
            fields.Add(FieldDescription.CreateGlobalIDField());
            BuildTable(gdb, name, fields);
        }

        private static void CreateTaxaSchema(
            Geodatabase gdb, string name, string sourceTable, string[] taxaFields, bool bySite)
        {
            var fields = new List<FieldDescription>
            {
                new(bySite ? "SiteID" : "SurveyID", FieldType.Integer) { IsNullable = true },
                new(bySite ? "Site_GlobalID" : "Survey_GlobalID", FieldType.GUID) { IsNullable = true },
            };
            fields.AddRange(CloneFieldsFromSource(gdb, sourceTable, taxaFields));
            fields.Add(FieldDescription.CreateGlobalIDField());
            BuildTable(gdb, name, fields);
        }

        private static void BuildTable(Geodatabase gdb, string name, List<FieldDescription> fields)
        {
            var schemaBuilder = new SchemaBuilder(gdb);
            schemaBuilder.Create(new TableDescription(name, fields));
            if (!schemaBuilder.Build())
            {
                throw new InvalidOperationException(
                    $"Failed to create '{name}': {string.Join("; ", schemaBuilder.ErrorMessages)}");
            }
        }

        /// <summary>Clones the named fields from a source table (String(255) fallback if absent).</summary>
        private static List<FieldDescription> CloneFieldsFromSource(
            Geodatabase gdb, string sourceTable, string[] fieldNames)
        {
            var result = new List<FieldDescription>();
            Table table = TryOpenTable(gdb, sourceTable);
            TableDefinition def = table?.GetDefinition();
            try
            {
                foreach (string fieldName in fieldNames)
                {
                    int idx = def?.FindField(fieldName) ?? -1;
                    if (idx >= 0)
                    {
                        result.Add(CloneField(def.GetFields()[idx]));
                    }
                    else
                    {
                        result.Add(FieldDescription.CreateStringField(fieldName, 255));
                    }
                }
            }
            finally
            {
                table?.Dispose();
            }

            return result;
        }

        // --- population ------------------------------------------------------

        private static void PopulateSurveys(
            Geodatabase gdb, string name, RollupContext ctx, Dictionary<string, object> siteGuid,
            Dictionary<string, object[]> taxaVert, Dictionary<string, object[]> taxaInvert,
            Dictionary<string, object[]> taxaFlora)
        {
            using Table table = gdb.OpenDataset<Table>(name);
            using InsertCursor insert = table.CreateInsertCursor();

            foreach (var entry in ctx.SurveysBySite)
            {
                object guid = siteGuid.TryGetValue(entry.Key, out var g) ? g : null;
                foreach (object[] survey in entry.Value)
                {
                    using RowBuffer buffer = table.CreateRowBuffer();
                    buffer["SiteID"] = survey[0];
                    buffer["SurveyID"] = survey[1];
                    buffer["SurveyDate"] = survey[2];
                    buffer["Project"] = survey[3];
                    // 0 (or unparseable) area is treated as missing -> null, matching
                    // the feature-class rollup and the legacy.
                    buffer["TotalAreaSQM"] = TryDouble(survey[4], out double area) && area != 0 ? area : (object)null;
                    buffer["CastSurveyors"] = survey[5];
                    buffer["Site_GlobalID"] = guid;

                    string surveyKey = KeepSetStore.KeyString(survey[1]);
                    WriteSurveyStats(buffer, surveyKey, ctx, taxaVert, taxaInvert, taxaFlora);

                    insert.Insert(buffer);
                }
            }

            insert.Flush();
        }

        private static void WriteSurveyStats(
            RowBuffer buffer, string surveyKey, RollupContext ctx,
            Dictionary<string, object[]> taxaVert, Dictionary<string, object[]> taxaInvert,
            Dictionary<string, object[]> taxaFlora)
        {
            // Flow (mean / sum / count / sample stdev across this survey's measurements).
            var flow = CollectValues(ctx.FlowBySurvey, surveyKey, 0);
            if (flow.Count > 0)
            {
                SummaryStats.MeanResult r = SummaryStats.MeanAndStdDev(flow);
                buffer["Flow_Mean"] = r.Mean;
                buffer["Flow_Sum"] = flow.Sum();
                buffer["Flow_Count"] = r.Count;
                buffer["Flow_Stand_Dev"] = r.SampleStdDev;
            }

            // Water quality, per parameter.
            var sc = new List<double>();
            var ph = new List<double>();
            var temp = new List<double>();
            var alk = new List<double>();
            if (ctx.WqBySurvey.TryGetValue(surveyKey ?? string.Empty, out var wqRows))
            {
                foreach (object[] row in wqRows)
                {
                    string param = AsString(row[0]);
                    if (param == null || !TryDouble(row[1], out double m))
                    {
                        continue;
                    }

                    switch (NormalizeWqParameter(param))
                    {
                        case AlkalinityCode: alk.Add(m); break;
                        case SpecCondFieldCode:
                        case SpecCondLabCode: sc.Add(m); break;
                        case PhFieldCode:
                        case PhLabCode: ph.Add(m); break;
                        case WaterTempCode: temp.Add(m); break;
                    }
                }
            }

            WriteStatTriple(buffer, sc, "Spec_Cond_Mean", "Spec_Cond_Stand_Dev", "Spec_Cond_Count");
            WriteStatTriple(buffer, ph, "pH_Mean", "pH_Stand_Dev", "pH_Count");
            WriteStatTriple(buffer, temp, "Water_Temp_Mean", "Water_Temp_Stand_Dev", "Water_Temp_Count");
            WriteStatTriple(buffer, alk, "Alkalinity_Mean", "Alkalinity_Stand_Dev", "Alkalinity_Count");

            // Per-survey taxon counts.
            var verts = CollectTaxa(VertTids(ctx, surveyKey), taxaVert);
            var inverts = CollectTaxa(InvertTids(ctx, surveyKey), taxaInvert);
            var flora = CollectTaxa(FloraTids(ctx, surveyKey), taxaFlora);

            var vc = SpeciesCounter.Count(verts, 2, 3, 4, 5);
            var ic = SpeciesCounter.Count(inverts, 2, 3, 4, 5);
            var fc = SpeciesCounter.Count(flora, -1, 6, 8, 1);

            buffer["Vert_Species_Count"] = vc.Species;
            buffer["Vert_Genus_Count"] = vc.Genus;
            buffer["Vert_Family_Count"] = vc.Family;
            buffer["Vert_Order_Count"] = vc.Order;
            buffer["Invert_Species_Count"] = ic.Species;
            buffer["Invert_Genus_Count"] = ic.Genus;
            buffer["Invert_Family_Count"] = ic.Family;
            buffer["Invert_Order_Count"] = ic.Order;
            buffer["Flora_Species_Count"] = fc.Species;
            buffer["Flora_Genus_Count"] = fc.Genus;
            buffer["Flora_Family_Count"] = fc.Family;
        }

        private static void WriteStatTriple(
            RowBuffer buffer, List<double> values, string meanField, string sdField, string countField)
        {
            if (values.Count > 0)
            {
                SummaryStats.MeanResult r = SummaryStats.MeanAndStdDev(values);
                buffer[meanField] = r.Mean;
                buffer[sdField] = r.SampleStdDev;
                buffer[countField] = r.Count;
            }
        }

        private static void PopulateSolar(
            Geodatabase gdb, string name, Dictionary<string, List<object[]>> solarBySite,
            Dictionary<string, object> siteGuid)
        {
            using Table table = gdb.OpenDataset<Table>(name);
            using InsertCursor insert = table.CreateInsertCursor();

            foreach (var entry in solarBySite)
            {
                object guid = siteGuid.TryGetValue(entry.Key, out var g) ? g : null;
                foreach (object[] record in entry.Value)
                {
                    using RowBuffer buffer = table.CreateRowBuffer();
                    for (int i = 0; i < SolarFields.Length; i++)
                    {
                        buffer[SolarFields[i]] = record[i];
                    }

                    buffer["Site_GlobalID"] = guid;
                    insert.Insert(buffer);
                }
            }

            insert.Flush();
        }

        private static void PopulateTaxa(
            Geodatabase gdb, string name, bool bySite, string[] fields,
            Dictionary<string, object[]> taxa, RollupContext ctx,
            Dictionary<string, object> siteGuid, Dictionary<string, object> surveyGuid,
            Func<RollupContext, string, List<object>> tidsForSurvey)
        {
            using Table table = gdb.OpenDataset<Table>(name);
            using InsertCursor insert = table.CreateInsertCursor();

            string keyField = bySite ? "SiteID" : "SurveyID";
            string guidField = bySite ? "Site_GlobalID" : "Survey_GlobalID";

            foreach (var entry in ctx.SurveysBySite)
            {
                List<object[]> surveys = entry.Value;
                if (bySite)
                {
                    object siteId = surveys.Count > 0 ? surveys[0][0] : null;
                    object guid = siteGuid.TryGetValue(entry.Key, out var g) ? g : null;
                    var seen = new HashSet<string>();
                    foreach (object[] survey in surveys)
                    {
                        string surveyKey = KeepSetStore.KeyString(survey[1]);
                        foreach (object[] record in ResolveTaxa(tidsForSurvey(ctx, surveyKey), taxa, seen))
                        {
                            WriteTaxonRow(table, insert, fields, record, keyField, siteId, guidField, guid);
                        }
                    }
                }
                else
                {
                    foreach (object[] survey in surveys)
                    {
                        string surveyKey = KeepSetStore.KeyString(survey[1]);
                        object guid = surveyGuid.TryGetValue(surveyKey ?? string.Empty, out var g) ? g : null;
                        var seen = new HashSet<string>();
                        foreach (object[] record in ResolveTaxa(tidsForSurvey(ctx, surveyKey), taxa, seen))
                        {
                            WriteTaxonRow(table, insert, fields, record, keyField, survey[1], guidField, guid);
                        }
                    }
                }
            }

            insert.Flush();
        }

        private static void WriteTaxonRow(
            Table table, InsertCursor insert, string[] fields, object[] record,
            string keyField, object keyValue, string guidField, object guid)
        {
            using RowBuffer buffer = table.CreateRowBuffer();
            for (int i = 0; i < fields.Length; i++)
            {
                buffer[fields[i]] = record[i];
            }

            buffer[keyField] = keyValue;
            buffer[guidField] = guid;
            insert.Insert(buffer);
        }

        private static IEnumerable<object[]> ResolveTaxa(
            List<object> tids, Dictionary<string, object[]> taxa, HashSet<string> seen)
        {
            foreach (object tidObj in tids)
            {
                string tid = KeepSetStore.KeyString(tidObj);
                if (tid == null || !seen.Add(tid))
                {
                    continue;
                }

                if (taxa.TryGetValue(tid, out object[] record))
                {
                    yield return record;
                }
            }
        }

        private static List<object> VertTids(RollupContext ctx, string surveyKey) =>
            TidsFromGrouped(ctx.VertBySurvey, surveyKey, 0);

        private static List<object> InvertTids(RollupContext ctx, string surveyKey) =>
            TidsFromGrouped(ctx.InvertBySurvey, surveyKey, 0);

        private static List<object> FloraTids(RollupContext ctx, string surveyKey)
        {
            var tids = new List<object>();
            if (surveyKey == null)
            {
                return tids;
            }

            if (ctx.PolySurveyBySurvey.TryGetValue(surveyKey, out var polys))
            {
                foreach (object[] poly in polys)
                {
                    string polyKey = KeepSetStore.KeyString(poly[0]);
                    if (polyKey != null && ctx.PolyFloraByPoly.TryGetValue(polyKey, out var floraRows))
                    {
                        foreach (object[] fr in floraRows)
                        {
                            tids.Add(fr[1]);
                        }
                    }
                }
            }

            if (ctx.GdeDomVegBySurvey.TryGetValue(surveyKey, out var domVeg))
            {
                foreach (object[] dv in domVeg)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        tids.Add(dv[i]);
                    }
                }
            }

            if (ctx.GdeOtherVegBySurvey.TryGetValue(surveyKey, out var otherVeg))
            {
                foreach (object[] ov in otherVeg)
                {
                    tids.Add(ov[1]);
                }
            }

            return tids;
        }

        private static List<object> TidsFromGrouped(
            Dictionary<string, List<object[]>> bySurvey, string surveyKey, int tidIndex)
        {
            var tids = new List<object>();
            if (surveyKey != null && bySurvey.TryGetValue(surveyKey, out var rows))
            {
                foreach (object[] row in rows)
                {
                    tids.Add(row[tidIndex]);
                }
            }

            return tids;
        }

        private static List<object[]> CollectTaxa(List<object> tids, Dictionary<string, object[]> taxa)
        {
            var records = new List<object[]>();
            var seen = new HashSet<string>();
            foreach (object tidObj in tids)
            {
                string tid = KeepSetStore.KeyString(tidObj);
                if (tid != null && seen.Add(tid) && taxa.TryGetValue(tid, out object[] record))
                {
                    records.Add(record);
                }
            }

            return records;
        }

        private static List<double> CollectValues(
            Dictionary<string, List<object[]>> bySurvey, string surveyKey, int valueIndex)
        {
            var values = new List<double>();
            if (surveyKey != null && bySurvey.TryGetValue(surveyKey, out var rows))
            {
                foreach (object[] row in rows)
                {
                    if (TryDouble(row[valueIndex], out double v))
                    {
                        values.Add(v);
                    }
                }
            }

            return values;
        }

        /// <summary>Reads a populated table's key field → its auto GlobalID value.</summary>
        private static Dictionary<string, object> ReadGuidMap(Geodatabase gdb, string tableName, string keyField)
        {
            var map = new Dictionary<string, object>();
            using Table table = gdb.OpenDataset<Table>(tableName);
            TableDefinition def = table.GetDefinition();
            int keyIndex = def.FindField(keyField);
            int gidIndex = -1;
            var fieldList = def.GetFields();
            for (int i = 0; i < fieldList.Count; i++)
            {
                if (fieldList[i].FieldType == FieldType.GlobalID)
                {
                    gidIndex = i;
                    break;
                }
            }

            if (keyIndex < 0 || gidIndex < 0)
            {
                return map;
            }

            using RowCursor cursor = table.Search(null, false);
            while (cursor.MoveNext())
            {
                using Row row = cursor.Current;
                string key = KeepSetStore.KeyString(row[keyIndex]);
                if (key != null && !map.ContainsKey(key))
                {
                    map[key] = row[gidIndex];
                }
            }

            return map;
        }

        // --- data loading ----------------------------------------------------

        /// <summary>Loads a table into a dictionary keyed by <paramref name="keyField"/> → list of field-value arrays.</summary>
        private static Dictionary<string, List<object[]>> LoadGrouped(
            Geodatabase gdb, string tableName, string keyField, params string[] fields)
        {
            var result = new Dictionary<string, List<object[]>>();
            Table table = TryOpenTable(gdb, tableName);
            if (table == null)
            {
                return result;
            }

            using (table)
            {
                int[] indexes = ResolveIndexes(table.GetDefinition(), fields);
                int keyIndex = table.GetDefinition().FindField(keyField);
                if (keyIndex < 0)
                {
                    return result;
                }

                using RowCursor cursor = table.Search(null, false);
                while (cursor.MoveNext())
                {
                    using Row row = cursor.Current;
                    string key = KeepSetStore.KeyString(row[keyIndex]);
                    if (key == null)
                    {
                        continue;
                    }

                    if (!result.TryGetValue(key, out var list))
                    {
                        list = new List<object[]>();
                        result[key] = list;
                    }

                    list.Add(ReadValues(row, indexes));
                }
            }

            return result;
        }

        /// <summary>Loads a lookup table into a dictionary keyed by <paramref name="keyField"/> → one field-value array.</summary>
        private static Dictionary<string, object[]> LoadSingle(
            Geodatabase gdb, string tableName, string keyField, params string[] fields)
        {
            var result = new Dictionary<string, object[]>();
            Table table = TryOpenTable(gdb, tableName);
            if (table == null)
            {
                return result;
            }

            using (table)
            {
                int[] indexes = ResolveIndexes(table.GetDefinition(), fields);
                int keyIndex = table.GetDefinition().FindField(keyField);
                if (keyIndex < 0)
                {
                    return result;
                }

                using RowCursor cursor = table.Search(null, false);
                while (cursor.MoveNext())
                {
                    using Row row = cursor.Current;
                    string key = KeepSetStore.KeyString(row[keyIndex]);
                    if (key != null && !result.ContainsKey(key))
                    {
                        result[key] = ReadValues(row, indexes);
                    }
                }
            }

            return result;
        }

        private static int[] ResolveIndexes(TableDefinition def, string[] fields)
        {
            var indexes = new int[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                indexes[i] = def.FindField(fields[i]);
            }

            return indexes;
        }

        private static object[] ReadValues(Row row, int[] indexes)
        {
            var values = new object[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] >= 0)
                {
                    object v = row[indexes[i]];
                    values[i] = v is DBNull ? null : v;
                }
            }

            return values;
        }

        // --- small helpers ---------------------------------------------------

        private static bool TryDouble(object value, out double result)
        {
            switch (value)
            {
                case null:
                case DBNull:
                    result = 0;
                    return false;
                case double d:
                    result = d;
                    return true;
                case float f:
                    result = f;
                    return true;
                case int i:
                    result = i;
                    return true;
                case short s:
                    result = s;
                    return true;
                case long l:
                    result = l;
                    return true;
                case decimal m:
                    result = (double)m;
                    return true;
                default:
                    return double.TryParse(
                        value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            }
        }

        private static string AsString(object value)
        {
            if (value == null || value is DBNull)
            {
                return null;
            }

            string s = value.ToString().Trim();
            return s.Length == 0 ? null : s;
        }

        private static object SafeGet(Row row, string field)
        {
            try
            {
                return row[field];
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static FeatureClass TryOpenFeatureClass(Geodatabase gdb, string name)
        {
            try { return gdb.OpenDataset<FeatureClass>(name); }
            catch (Exception) { return null; }
        }

        private static Table TryOpenTable(Geodatabase gdb, string name)
        {
            try { return gdb.OpenDataset<Table>(name); }
            catch (Exception) { return null; }
        }

        /// <summary>Holds the loaded related-table indexes for the rollup pass.</summary>
        private sealed class RollupContext
        {
            internal RollupContext(
                Dictionary<string, List<object[]>> surveysBySite,
                Dictionary<string, List<object[]>> flowBySurvey,
                Dictionary<string, List<object[]>> wqBySurvey,
                Dictionary<string, List<object[]>> vertBySurvey,
                Dictionary<string, List<object[]>> invertBySurvey,
                Dictionary<string, List<object[]>> polySurveyBySurvey,
                Dictionary<string, List<object[]>> polyFloraByPoly,
                Dictionary<string, List<object[]>> gdeDomVegBySurvey,
                Dictionary<string, List<object[]>> gdeOtherVegBySurvey,
                Dictionary<string, List<object[]>> imagesBySurvey,
                Dictionary<string, object[]> taxaVert,
                Dictionary<string, object[]> taxaInvert,
                Dictionary<string, object[]> taxaFlora)
            {
                SurveysBySite = surveysBySite;
                FlowBySurvey = flowBySurvey;
                WqBySurvey = wqBySurvey;
                VertBySurvey = vertBySurvey;
                InvertBySurvey = invertBySurvey;
                PolySurveyBySurvey = polySurveyBySurvey;
                PolyFloraByPoly = polyFloraByPoly;
                GdeDomVegBySurvey = gdeDomVegBySurvey;
                GdeOtherVegBySurvey = gdeOtherVegBySurvey;
                ImagesBySurvey = imagesBySurvey;
                TaxaVert = taxaVert;
                TaxaInvert = taxaInvert;
                TaxaFlora = taxaFlora;
            }

            internal Dictionary<string, List<object[]>> SurveysBySite { get; }
            internal Dictionary<string, List<object[]>> FlowBySurvey { get; }
            internal Dictionary<string, List<object[]>> WqBySurvey { get; }
            internal Dictionary<string, List<object[]>> VertBySurvey { get; }
            internal Dictionary<string, List<object[]>> InvertBySurvey { get; }
            internal Dictionary<string, List<object[]>> PolySurveyBySurvey { get; }
            internal Dictionary<string, List<object[]>> PolyFloraByPoly { get; }
            internal Dictionary<string, List<object[]>> GdeDomVegBySurvey { get; }
            internal Dictionary<string, List<object[]>> GdeOtherVegBySurvey { get; }
            internal Dictionary<string, List<object[]>> ImagesBySurvey { get; }
            internal Dictionary<string, object[]> TaxaVert { get; }
            internal Dictionary<string, object[]> TaxaInvert { get; }
            internal Dictionary<string, object[]> TaxaFlora { get; }
        }
    }
}
