using System.Collections.Generic;
using ArcGIS.Core.Data;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Names of the in-memory "keep sets" of key values that are built while
    /// copying one dataset and used to filter later datasets. These mirror the
    /// collections the legacy VB6 tool (CopyAndDeleteNonSelectedSprings2) passed
    /// between its CopyOverFClassOrTable calls.
    /// </summary>
    internal enum KeepSet
    {
        /// <summary>SiteID values of the springs that were copied (built from the selection).</summary>
        SiteIDs,

        /// <summary>SurveyID values copied into tbl_Surveys.</summary>
        SurveyIDs,

        /// <summary>8-digit HUC codes (field "HUC") on the copied springs.</summary>
        Huc8s,

        /// <summary>12-digit HUC codes (field "CastHUC_12") on the copied springs.</summary>
        Huc12s,

        /// <summary>Flora taxon IDs gathered from the five species-TID fields of gde_domveglevel1.</summary>
        FloraTIDs,

        /// <summary>SurveyPolygonAutoID (PK) values of the kept tbl_PolygonSurvey rows; filters tbl_PolygonFlora.SurveyPolygonID.</summary>
        PolygonAutoIDs,

        /// <summary>SitePolygonID values of the kept tbl_PolygonSurvey rows; filters tbl_PolygonSite_Survey.PolygonID.</summary>
        SitePolygonIDs,
    }

    /// <summary>
    /// Describes a keep-set that a copy operation populates as it copies rows:
    /// for each copied row, the values of <see cref="Fields"/> are added to the
    /// set named <see cref="Set"/>. Most builds read a single field; the
    /// gde_domveglevel1 build reads five comma-listed species-TID fields.
    /// </summary>
    internal sealed class BuildSpec
    {
        internal BuildSpec(KeepSet set, params string[] fields)
        {
            Set = set;
            Fields = fields;
        }

        internal KeepSet Set { get; }
        internal IReadOnlyList<string> Fields { get; }
    }

    /// <summary>
    /// One dataset-copy operation: copy <see cref="SourceName"/> from the source
    /// geodatabase into the output geodatabase, keeping only rows whose
    /// <see cref="FilterField"/> value is in keep-set <see cref="FilterSet"/>
    /// (or all rows when <see cref="FilterSet"/> is null), populating the
    /// <see cref="Builds"/> keep-sets along the way. Ported one-to-one from the
    /// ordered CopyOverFClassOrTable calls in the legacy tool.
    /// </summary>
    internal sealed class CopyOperation
    {
        internal CopyOperation(
            string sourceName,
            KeepSet? filterSet,
            string filterField,
            IReadOnlyList<BuildSpec> builds = null,
            bool applyExclusions = false,
            KeepSet? filterSet2 = null,
            string filterField2 = null)
        {
            SourceName = sourceName;
            FilterSet = filterSet;
            FilterField = filterField;
            Builds = builds ?? System.Array.Empty<BuildSpec>();
            ApplyExclusions = applyExclusions;
            FilterSet2 = filterSet2;
            FilterField2 = filterField2;
        }

        internal string SourceName { get; }

        /// <summary>Keep-set to filter against; null means copy every row (a lookup table).</summary>
        internal KeepSet? FilterSet { get; }

        /// <summary>Field in this dataset tested against <see cref="FilterSet"/>.</summary>
        internal string FilterField { get; }

        /// <summary>
        /// Optional second keep-set filter, AND-ed with the first. A row is kept
        /// only if it also passes this one. Mirrors the legacy two-criteria deletes
        /// (e.g. tbl_PolygonSite_Survey: SiteID in sites AND PolygonID in kept
        /// polygon ids).
        /// </summary>
        internal KeepSet? FilterSet2 { get; }

        /// <summary>Field tested against <see cref="FilterSet2"/>.</summary>
        internal string FilterField2 { get; }

        internal IReadOnlyList<BuildSpec> Builds { get; }

        /// <summary>
        /// When true (only tbl_Surveys), also drop rows whose SiteID is in the
        /// exclude-sites set or whose SurveyID is in the exclude-surveys set
        /// (the two optional SQL exclusions from the dialog).
        /// </summary>
        internal bool ApplyExclusions { get; }
    }

    /// <summary>
    /// One relationship class to (re)create in the output geodatabase. Ported
    /// from the varRCWorkOrder array in the legacy tool. None of the SSI
    /// relationship classes are many-to-many, so only OneToOne / OneToMany
    /// cardinalities occur and no attributed/intermediate table is needed.
    /// </summary>
    internal sealed class RelationshipDef
    {
        internal RelationshipDef(
            string name,
            string originDataset,
            string destinationDataset,
            string originPrimaryKey,
            string originForeignKey,
            RelationshipCardinality cardinality)
        {
            Name = name;
            OriginDataset = originDataset;
            DestinationDataset = destinationDataset;
            OriginPrimaryKey = originPrimaryKey;
            OriginForeignKey = originForeignKey;
            Cardinality = cardinality;
        }

        internal string Name { get; }

        /// <summary>Origin dataset name, or <see cref="SpringsExportSchema.SpringsToken"/> for the springs feature class.</summary>
        internal string OriginDataset { get; }

        internal string DestinationDataset { get; }
        internal string OriginPrimaryKey { get; }
        internal string OriginForeignKey { get; }
        internal RelationshipCardinality Cardinality { get; }
    }

    /// <summary>
    /// Hard-coded schema configuration for the "Export Subset of Springs" tool,
    /// transcribed from the legacy VB6 tool's CopyAndDeleteNonSelectedSprings2
    /// (SSI_Functions_2.bas) and ReturnArrayOfFieldNamesToIndex (SSI_Functions.bas).
    /// This is pure data; the logic lives in <see cref="DatasetCopier"/> and
    /// <see cref="SpringsSubsetExporter"/>.
    /// </summary>
    internal static class SpringsExportSchema
    {
        /// <summary>
        /// Placeholder used in <see cref="RelationshipDef.OriginDataset"/> for the
        /// springs feature class, whose real name is chosen by the user at runtime.
        /// </summary>
        internal const string SpringsToken = "<SPRINGS>";

        private const RelationshipCardinality OneToOne = RelationshipCardinality.OneToOne;
        private const RelationshipCardinality OneToMany = RelationshipCardinality.OneToMany;

        // --- Ordered related-dataset copy operations -------------------------
        // The springs feature class itself is copied separately by the engine
        // (it honors the map selection and builds SiteIDs/Huc8s/Huc12s). This
        // list is the ~100 related tables, in the exact legacy order; order
        // matters because some entries build sets that later entries consume.

        internal static readonly IReadOnlyList<CopyOperation> CopyOperations = new[]
        {
            // Surveys: filter by kept SiteIDs, build the SurveyIDs set, apply the two SQL exclusions.
            new CopyOperation("tbl_Surveys", KeepSet.SiteIDs, "SiteID",
                new[] { new BuildSpec(KeepSet.SurveyIDs, "SurveyID") }, applyExclusions: true),

            // Polygon surveys: filter by kept SurveyIDs and build the two polygon-id
            // sets (SurveyPolygonAutoID and SitePolygonID) that tbl_PolygonFlora and
            // tbl_PolygonSite_Survey filter against. Must run before those two so the
            // sets exist when they are consumed.
            new CopyOperation("tbl_PolygonSurvey", KeepSet.SurveyIDs, "SurveyID",
                new[]
                {
                    new BuildSpec(KeepSet.PolygonAutoIDs, "SurveyPolygonAutoID"),
                    new BuildSpec(KeepSet.SitePolygonIDs, "SitePolygonID"),
                }),

            // Site-level tables (filter by SiteID).
            new CopyOperation("tbl_PolygonSite", KeepSet.SiteIDs, "SiteID"),
            new CopyOperation("tbl_Solar", KeepSet.SiteIDs, "SiteID"),
            // Legacy keeps rows for kept sites AND whose PolygonID is a kept
            // tbl_PolygonSurvey.SitePolygonID (a two-criteria delete), so AND a
            // second keep-set filter on PolygonID.
            new CopyOperation("tbl_PolygonSite_Survey", KeepSet.SiteIDs, "SiteID",
                filterSet2: KeepSet.SitePolygonIDs, filterField2: "PolygonID"),

            // Survey-level tables (filter by SurveyID).
            new CopyOperation("tbl_reports", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_images", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_qtyvolume", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_VertSurvey", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_WQData", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_WQData_Location", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_SEAP_Scores", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_InvertSampling", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_flow", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_HydroQuality", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_SadaProtocols", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("gde_disturbance", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("gde_mgmtindicators", KeepSet.SurveyIDs, "SurveyID"),

            // Polygon flora: keep only rows whose SurveyPolygonID is a kept
            // tbl_PolygonSurvey.SurveyPolygonAutoID (legacy copies all, then deletes
            // the rest). Was previously an unfiltered full copy — the bug that left
            // the Pro output with every flora row instead of the subset.
            new CopyOperation("tbl_PolygonFlora", KeepSet.PolygonAutoIDs, "SurveyPolygonID"),

            // Lookup tables copied in full (no filter).
            new CopyOperation("tlu_TaxaVert", null, null),
            new CopyOperation("tlu_TaxaInvert", null, null),
            new CopyOperation("tlu_flowrate", null, null),
            new CopyOperation("tlu_flowpersistence", null, null),
            new CopyOperation("tlu_flowconsistency", null, null),
            new CopyOperation("tlu_flowvariability", null, null),
            new CopyOperation("tlu_TaxaFlora", null, null),
            new CopyOperation("tlu_covercodes", null, null),
            new CopyOperation("tlu_endemism_Vert", null, null),
            new CopyOperation("tlu_springlifehistory_Vert", null, null),
            new CopyOperation("tlu_aquaticstatus_Vert", null, null),
            new CopyOperation("tlu_esastatus_Vert", null, null),
            new CopyOperation("tlu_iucnstatus_Vert", null, null),
            new CopyOperation("tlu_nativestatuscodes_Invert", null, null),
            new CopyOperation("tlu_protectedarea_Invert", null, null),
            new CopyOperation("tlu_endemism_Invert", null, null),
            new CopyOperation("tlu_springlifehistory_Invert", null, null),
            new CopyOperation("tlu_aquaticstatus_Invert", null, null),
            new CopyOperation("tlu_esastatus_Invert", null, null),
            new CopyOperation("tlu_iucnstatus_Invert", null, null),
            new CopyOperation("tlu_nativestatuscodes", null, null),
            new CopyOperation("tlu_protectedarea", null, null),
            new CopyOperation("tlu_wetlandstatus_les", null, null),
            new CopyOperation("tlu_endemism", null, null),
            new CopyOperation("tlu_springmicrohabitatuse", null, null),
            new CopyOperation("tlu_springlifehistory", null, null),
            new CopyOperation("tlu_aquaticstatus", null, null),
            new CopyOperation("tlu_esastatus", null, null),
            new CopyOperation("tlu_iucnstatus", null, null),
            new CopyOperation("tlu_covercodes_TaxaFlora", null, null),

            // Water-quality location chain.
            new CopyOperation("tbl_wqlocation", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tlu_wqlocation", null, null),
            new CopyOperation("tlu_wqparameters", null, null),
            new CopyOperation("tlu_wqparameters_location", null, null),

            // Site attribute lookups.
            new CopyOperation("tlu_lithoprimary", null, null),
            new CopyOperation("tlu_lithosecondary", null, null),
            new CopyOperation("tlu_emergenceenvironment", null, null),
            new CopyOperation("tlu_proclaimednf", null, null),

            // HUC lookups (filter by the kept HUC codes).
            new CopyOperation("tlu_huc", KeepSet.Huc8s, "HUC_ID"),
            new CopyOperation("tlu_huc12", KeepSet.Huc12s, "HUC12"),

            new CopyOperation("tlu_weather", null, null),
            new CopyOperation("tlu_sfcwateroccur", null, null),

            // SEAP score tables (filter by SurveyID).
            new CopyOperation("tbl_seapcultscores", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_seapscore", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("tbl_seapsummary", KeepSet.SurveyIDs, "SurveyID"),

            // Site treatment (filter by SiteID) + its lookup.
            new CopyOperation("tbl_site_treatment", KeepSet.SiteIDs, "SiteID"),
            new CopyOperation("tlu_treatmentareas", null, null),

            new CopyOperation("tlu_seapcodes", null, null),
            new CopyOperation("tlu_seapcondoptions", null, null),
            new CopyOperation("tlu_seapriskoptions", null, null),
            new CopyOperation("tlu_sensitive", null, null),
            new CopyOperation("tlu_globalconservationstatus", null, null),
            new CopyOperation("tlu_globalconservationstatus_Invert", null, null),
            new CopyOperation("tlu_globalconservationstatus_Vert", null, null),
            new CopyOperation("tlu_ntnlconservationstatus", null, null),
            new CopyOperation("tlu_ntnlconservationstatus_Invert", null, null),
            new CopyOperation("tlu_ntnlconservationstatus_Vert", null, null),
            new CopyOperation("tlu_sada_disturbance", null, null),

            // GDE survey chain. gde_domveglevel1 builds the FloraTIDs set from five fields.
            new CopyOperation("gde_domveglevel1", KeepSet.SurveyIDs, "SurveyID",
                new[]
                {
                    new BuildSpec(KeepSet.FloraTIDs,
                        "TreeSpeciesTID", "ShrubSpeciesTID", "GraminoidSpeciesTID",
                        "ForbSpeciesTID", "AquaticSpeciesTID"),
                }),
            new CopyOperation("gde_otherveg", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("gde_soil", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("gde_surveys", KeepSet.SurveyIDs, "SurveyID"),
            new CopyOperation("gde_watertable", KeepSet.SurveyIDs, "SurveyID"),

            // GDE lookups copied in full.
            new CopyOperation("gdelu_areamethod", null, null),
            new CopyOperation("gdelu_bryoabundance", null, null),
            new CopyOperation("gdelu_cutlevel", null, null),
            new CopyOperation("gdelu_flowpatternin", null, null),
            new CopyOperation("gdelu_flowpatternout", null, null),
            new CopyOperation("gdelu_lifeformrank", null, null),
            new CopyOperation("gdelu_purpose", null, null),
            new CopyOperation("gdelu_soillocation", null, null),
            new CopyOperation("gdelu_soilmethod", null, null),
            new CopyOperation("gdelu_veg_surr", null, null),
            new CopyOperation("gdelu_watertabletype", null, null),
            new CopyOperation("gdelu_wtlocation", null, null),
            new CopyOperation("gdelu_wtsource", null, null),

            new CopyOperation("tlu_surveyprotocol", null, null),

            // Flora-GDE taxon lookup (filter by the FloraTIDs gathered above) + its lookups.
            new CopyOperation("tlu_TaxaFlora_GDE", KeepSet.FloraTIDs, "TID"),
            new CopyOperation("tlu_globalconservationstatus_GDE", null, null),
            new CopyOperation("tlu_nativestatuscodes_GDE", null, null),
            new CopyOperation("tlu_protectedarea_GDE", null, null),
            // The remaining Flora-GDE lookups, copied in full like the three above
            // (legacy "COPY ENTIRE … TABLE" calls). These were missing, so their
            // by_Flora_GDE relationship classes (defined below) had no destination
            // table and were silently skipped.
            new CopyOperation("tlu_wetlandstatus_les_GDE", null, null),
            new CopyOperation("tlu_endemism_GDE", null, null),
            new CopyOperation("tlu_springmicrohabitatuse_GDE", null, null),
            new CopyOperation("tlu_springlifehistory_GDE", null, null),
            new CopyOperation("tlu_aquaticstatus_GDE", null, null),
            new CopyOperation("tlu_esastatus_GDE", null, null),
            new CopyOperation("tlu_covercodes_TaxaFlora_GDE", null, null),
            new CopyOperation("tlu_iucnstatus_GDE", null, null),
            new CopyOperation("tlu_ntnlconservationstatus_GDE", null, null),
        };

        // --- Relationship classes -------------------------------------------
        // Transcribed from varRCWorkOrder (origin PK = element 12, origin FK =
        // element 14, cardinality = element 7 where 1=OneToOne, 2=OneToMany).

        internal static readonly IReadOnlyList<RelationshipDef> RelationshipClasses = new[]
        {
            new RelationshipDef("Surveys_by_Site", SpringsToken, "tbl_Surveys", "SiteID", "SiteID", OneToMany),
            new RelationshipDef("PolygonSites_by_Site", SpringsToken, "tbl_PolygonSite", "SiteID", "SiteID", OneToMany),
            new RelationshipDef("Solar_by_Site", SpringsToken, "tbl_Solar", "SiteID", "SiteID", OneToOne),
            new RelationshipDef("Polygons_by_Survey", "tbl_Surveys", "tbl_PolygonSurvey", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("Vertebrates_by_Survey", "tbl_Surveys", "tbl_VertSurvey", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("InvertSampling_by_Survey", "tbl_Surveys", "tbl_InvertSampling", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("Flow_by_Survey", "tbl_Surveys", "tbl_flow", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("WaterQuality_by_Survey", "tbl_Surveys", "tbl_WQData", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("SEAP_by_Survey", "tbl_Surveys", "tbl_SEAP_Scores", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("Images_by_Survey", "tbl_Surveys", "tbl_images", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("qtyVolume_by_Survey", "tbl_Surveys", "tbl_qtyvolume", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("Reports_by_Survey", "tbl_Surveys", "tbl_reports", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("Sada_by_Survey", "tbl_Surveys", "tbl_SadaProtocols", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("HydroQual_by_Survey", "tbl_Surveys", "tbl_HydroQuality", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("Flora_by_Polygon", "tbl_PolygonSurvey", "tbl_PolygonFlora", "SurveyPolygonAutoID", "SurveyPolygonID", OneToMany),
            new RelationshipDef("PolygonSites_by_Survey", "tbl_PolygonSurvey", "tbl_PolygonSite_Survey", "SitePolygonID", "PolygonID", OneToOne),
            new RelationshipDef("Taxonomy_by_Vertebrates", "tbl_VertSurvey", "tlu_TaxaVert", "tid", "TID", OneToOne),
            new RelationshipDef("Taxonomy_by_Invertebrates", "tbl_InvertSampling", "tlu_TaxaInvert", "tid", "TID", OneToOne),
            new RelationshipDef("Flow_Rate_by_Flow", "tbl_flow", "tlu_flowrate", "FlowRateMean", "CastFlowRate", OneToOne),
            new RelationshipDef("Flow_Persistence_by_Flow", "tbl_flow", "tlu_flowpersistence", "Persistence", "Persistence", OneToOne),
            new RelationshipDef("Flow_Consistency_by_Flow", "tbl_flow", "tlu_flowconsistency", "FlowConsistency", "FlowConsistency", OneToOne),
            new RelationshipDef("Flow_Variability_by_Flow", "tbl_flow", "tlu_flowvariability", "FlowVariability", "FlowVariability", OneToOne),
            new RelationshipDef("Taxonomy_by_Flora", "tbl_PolygonFlora", "tlu_TaxaFlora", "tid", "TID", OneToOne),
            new RelationshipDef("CoverCode_by_Polygon", "tbl_PolygonFlora", "tlu_covercodes", "FloraCoverCode", "CoverCode", OneToOne),
            new RelationshipDef("Endemism_by_Vert", "tlu_TaxaVert", "tlu_endemism_Vert", "EndemismLevel", "EndemismLevel", OneToOne),
            new RelationshipDef("SpLifeHist_by_Vert", "tlu_TaxaVert", "tlu_springlifehistory_Vert", "SpringLifeHistory", "CastSpringLifeHistory", OneToOne),
            new RelationshipDef("AquaticStatus_by_Vert", "tlu_TaxaVert", "tlu_aquaticstatus_Vert", "AquaticStatus", "AquaticStatus", OneToOne),
            new RelationshipDef("ESA_by_Vert", "tlu_TaxaVert", "tlu_esastatus_Vert", "ESAstatus", "ESAstatus", OneToOne),
            new RelationshipDef("IUCN_by_Vert", "tlu_TaxaVert", "tlu_iucnstatus_Vert", "IUCNstatus", "IUCNstatus", OneToOne),
            new RelationshipDef("NativeStatus_by_Invert", "tlu_TaxaInvert", "tlu_nativestatuscodes_Invert", "DefaultNativeStatus", "NativeStatusCode", OneToOne),
            new RelationshipDef("Protected_Area_by_Invert", "tlu_TaxaInvert", "tlu_protectedarea_Invert", "DefaultProtectedArea", "ProtectedArea", OneToOne),
            new RelationshipDef("Endemism_by_Invert", "tlu_TaxaInvert", "tlu_endemism_Invert", "EndemismLevel", "EndemismLevel", OneToOne),
            new RelationshipDef("SpLifeHist_by_Invert", "tlu_TaxaInvert", "tlu_springlifehistory_Invert", "SpringLifeHistory", "CastSpringLifeHistory", OneToOne),
            new RelationshipDef("Aquatic_Status_by_Invert", "tlu_TaxaInvert", "tlu_aquaticstatus_Invert", "AquaticStatus", "AquaticStatus", OneToOne),
            new RelationshipDef("ESA_by_Invert", "tlu_TaxaInvert", "tlu_esastatus_Invert", "ESAstatus", "ESAstatus", OneToOne),
            new RelationshipDef("IUCN_by_Invert", "tlu_TaxaInvert", "tlu_iucnstatus_Invert", "IUCNstatus", "IUCNstatus", OneToOne),
            new RelationshipDef("NativeStat_by_Flora", "tlu_TaxaFlora", "tlu_nativestatuscodes", "DefaultNativeStatus", "NativeStatusCode", OneToOne),
            new RelationshipDef("Protected_by_Flora", "tlu_TaxaFlora", "tlu_protectedarea", "DefaultProtectedArea", "ProtectedArea", OneToOne),
            new RelationshipDef("Wetland_by_Flora", "tlu_TaxaFlora", "tlu_wetlandstatus_les", "DefaultWetlandStatus", "WetlandCode", OneToOne),
            new RelationshipDef("Endemism_by_Flora", "tlu_TaxaFlora", "tlu_endemism", "EndemismLevel", "EndemismLevel", OneToOne),
            new RelationshipDef("SpringHab_by_Flora", "tlu_TaxaFlora", "tlu_springmicrohabitatuse", "SpringHabitatUse", "SpringUse", OneToOne),
            new RelationshipDef("SpLifeHist_by_Flora", "tlu_TaxaFlora", "tlu_springlifehistory", "SpringLifeHistory", "CastSpringLifeHistory", OneToOne),
            new RelationshipDef("Aquatic_by_Flora", "tlu_TaxaFlora", "tlu_aquaticstatus", "AquaticStatus", "AquaticStatus", OneToOne),
            new RelationshipDef("ESA_by_Flora", "tlu_TaxaFlora", "tlu_esastatus", "ESAstatus", "ESAstatus", OneToOne),
            new RelationshipDef("IUCN_by_Flora", "tlu_TaxaFlora", "tlu_iucnstatus", "IUCNstatus", "IUCNstatus", OneToOne),
            new RelationshipDef("CoverCodes_by_Flora", "tlu_TaxaFlora", "tlu_covercodes_TaxaFlora", "DefaultCoverCode", "CoverCode", OneToOne),
            new RelationshipDef("GDE_gdesurvey_by_survey", "tbl_Surveys", "gde_surveys", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("GDE_Purpose_by_GDE_Survey", "gde_surveys", "gdelu_purpose", "Purpose", "Purpose", OneToOne),
            new RelationshipDef("GDE_WtrTblTyp_by_GDE_Srvy", "gde_surveys", "gdelu_watertabletype", "WaterTableType", "WaterTableType", OneToOne),
            new RelationshipDef("GDE_FlwPttrnIn_by_GDE_Srvys", "gde_surveys", "gdelu_flowpatternin", "FlowPatternIn", "FlowPatternIn", OneToOne),
            new RelationshipDef("GDE_FlwPttrnOt_by_GDE_Srvys", "gde_surveys", "gdelu_flowpatternout", "FlowPatternOut", "FlowPatternOut", OneToOne),
            new RelationshipDef("GDE_OtherVeg_by_GDE_Surveys", "gde_surveys", "gde_otherveg", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("GDE_Soil_by_GDE_Surveys", "gde_surveys", "gde_soil", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("GDE_SoilLocation_by_GDE_Soil", "gde_soil", "gdelu_soillocation", "SoilLocation", "SoilLocation", OneToOne),
            new RelationshipDef("GDE_SoilMethod_by_GDE_Soil", "gde_soil", "gdelu_soilmethod", "Method", "Method", OneToOne),
            new RelationshipDef("GDE_WaterTable_by_GDE_Surveys", "gde_surveys", "gde_watertable", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("GDE_WTSource_by_GDE_WaterTable", "gde_watertable", "gdelu_wtsource", "Source", "WTSource", OneToOne),
            new RelationshipDef("GDE_WTLctn_by_GDE_WtrTbl", "gde_watertable", "gdelu_wtlocation", "MsrmtLocation", "WTLocation", OneToOne),
            new RelationshipDef("GDE_DomVegLevel_by_GDE_Survey", "gde_surveys", "gde_domveglevel1", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("GDE_Disturbance_by_GDE_Survey", "gde_surveys", "gde_disturbance", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("GDE_MgmtInd_by_GDE_Surveys", "gde_surveys", "gde_mgmtindicators", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("GDE_VegSurr_by_GDE_DomVegLevel", "gde_domveglevel1", "gdelu_veg_surr", "Veg_Surr", "Veg_Surr", OneToOne),
            new RelationshipDef("GDE_BryAbndnc_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_bryoabundance", "BryoAbundance", "BryoAbundance", OneToOne),
            new RelationshipDef("GDE_CtLvl_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_cutlevel", "CutLevelCode", "CutLevelCode", OneToOne),
            new RelationshipDef("GDE_TrRnk_by_GDE_DmVglvl", "gde_domveglevel1", "gdelu_lifeformrank", "TreeRank", "Rank", OneToOne),
            new RelationshipDef("GDE_ShrbRnk_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_lifeformrank", "ShrubRank", "Rank", OneToOne),
            new RelationshipDef("GDE_GrmndRnk_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_lifeformrank", "GraminoidRank", "Rank", OneToOne),
            new RelationshipDef("GDE_FrbRnk_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_lifeformrank", "ForbRank", "Rank", OneToOne),
            new RelationshipDef("GDE_AqtcRnk_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_lifeformrank", "AquaticRank", "RankDescription", OneToOne),
            new RelationshipDef("GDE_UnknwnRnk_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_lifeformrank", "UnknownRank", "Rank", OneToOne),
            new RelationshipDef("GDE_BryphytRnk_by_GDE_DmVgLvl", "gde_domveglevel1", "gdelu_lifeformrank", "BryophyteRank", "Rank", OneToOne),
            new RelationshipDef("GDE_AreaMethod_by_Survey", "tbl_Surveys", "gdelu_areamethod", "Area_Method", "Area_Method", OneToOne),
            new RelationshipDef("NtnlConsStatus_by_Flora", "tlu_TaxaFlora", "tlu_ntnlconservationstatus", "NtnlConservationStatus", "CastNtnlConservationStatus", OneToOne),
            new RelationshipDef("GlobalConsStatus_by_Flora", "tlu_TaxaFlora", "tlu_globalconservationstatus", "GlobalConservationStatus", "CastGlobalConservationStatus", OneToOne),
            new RelationshipDef("NtnlConsStatus_by_Vert", "tlu_TaxaVert", "tlu_ntnlconservationstatus_Vert", "NtnlConservationStatus", "CastNtnlConservationStatus", OneToOne),
            new RelationshipDef("GlobalConsStats_by_Vert", "tlu_TaxaVert", "tlu_globalconservationstatus_Vert", "GlobalConservationStatus", "CastGlobalConservationStatus", OneToOne),
            new RelationshipDef("NtnlConsStatus_by_Invert", "tlu_TaxaInvert", "tlu_ntnlconservationstatus_Invert", "NtnlConservationStatus", "CastNtnlConservationStatus", OneToOne),
            new RelationshipDef("GlobalConsStatus_by_Invert", "tlu_TaxaInvert", "tlu_globalconservationstatus_Invert", "GlobalConservationStatus", "CastGlobalConservationStatus", OneToOne),
            new RelationshipDef("SEAP_Original_by_Survey", "tbl_Surveys", "tbl_seapscore", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("SEAP_CultureScores_by_Survey", "tbl_Surveys", "tbl_seapcultscores", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("SEAP_Summaries_by_Survey", "tbl_Surveys", "tbl_seapsummary", "SurveyID", "SurveyID", OneToOne),
            new RelationshipDef("Weather_by_Survey", "tbl_Surveys", "tlu_weather", "Weather", "Weather", OneToOne),
            new RelationshipDef("SFCWaterOccurance_by_Survey", "tbl_Surveys", "tlu_sfcwateroccur", "SfcWaterOccur", "SfcWaterOccur", OneToOne),
            new RelationshipDef("LithoPrimary_by_Site", SpringsToken, "tlu_lithoprimary", "LithoPrimary", "LithoPrimary", OneToOne),
            new RelationshipDef("LithoSecondary_by_Site", SpringsToken, "tlu_lithosecondary", "LithoSecondary", "LithoSecondary", OneToOne),
            new RelationshipDef("EmergenceEnvironment_by_Site", SpringsToken, "tlu_emergenceenvironment", "EmergenceEnvironment", "EmergenceEnvironment", OneToOne),
            new RelationshipDef("ProclaimedNatForest_by_Site", SpringsToken, "tlu_proclaimednf", "ProclaimedNF", "ProclaimedNF", OneToOne),
            new RelationshipDef("HUC8_by_Site", SpringsToken, "tlu_huc", "HUC", "HUC_ID", OneToOne),
            new RelationshipDef("HUC12_by_Site", SpringsToken, "tlu_huc12", "CastHUC_12", "HUC12", OneToOne),
            new RelationshipDef("SiteTreatment_by_Site", SpringsToken, "tbl_site_treatment", "SiteID", "SiteID", OneToMany),
            new RelationshipDef("Sensitivity_by_Site", SpringsToken, "tlu_sensitive", "Sensitivity", "Sensitive", OneToOne),
            new RelationshipDef("WQLocation_by_Survey", "tbl_Surveys", "tbl_wqlocation", "SurveyID", "SurveyID", OneToMany),
            new RelationshipDef("WQData_location_by_WQLocation", "tbl_wqlocation", "tbl_WQData_Location", "WQLocation_ID", "WQLocation_ID", OneToMany),
            new RelationshipDef("WQLocationLU_by_WQLocation", "tbl_wqlocation", "tlu_wqlocation", "LocationWQ", "Location", OneToOne),
            new RelationshipDef("WQParameters_by_WQData", "tbl_WQData", "tlu_wqparameters", "WQParameter_ID", "WQParameter_ID", OneToOne),
            new RelationshipDef("WQParams_by_WQData_Location", "tbl_WQData_Location", "tlu_wqparameters_location", "WQParameter_ID", "WQParameter_ID", OneToOne),
            new RelationshipDef("TreatmentArea_by_SiteTreatment", "tbl_site_treatment", "tlu_treatmentareas", "TreatmentAreaID", "TreatmentAreaID", OneToOne),
            new RelationshipDef("Protocol_by_Survey", "tbl_Surveys", "tlu_surveyprotocol", "SurveyProtocol", "CastProtocolID", OneToOne),
            new RelationshipDef("TxFlr_by_GDE_DmVgLvl1_Tr", "gde_domveglevel1", "tlu_TaxaFlora_GDE", "TreeSpeciesTID", "TID", OneToOne),
            new RelationshipDef("TxFlr_by_GDE_DmVgLvl1_Aqtc", "gde_domveglevel1", "tlu_TaxaFlora_GDE", "AquaticSpeciesTID", "TID", OneToOne),
            new RelationshipDef("TxFlr_by_GDE_DmVgLvl1_Shrb", "gde_domveglevel1", "tlu_TaxaFlora_GDE", "ShrubSpeciesTID", "TID", OneToOne),
            new RelationshipDef("TxFlr_by_GDE_DmVgLvl1_Grm", "gde_domveglevel1", "tlu_TaxaFlora_GDE", "GraminoidSpeciesTID", "TID", OneToOne),
            new RelationshipDef("TxFlr_by_GDE_DmVgLvl1_Frb", "gde_domveglevel1", "tlu_TaxaFlora_GDE", "ForbSpeciesTID", "TID", OneToOne),
            new RelationshipDef("GlobalConsStatus_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_globalconservationstatus_GDE", "GlobalConservationStatus", "CastGlobalConservationStatus", OneToOne),
            new RelationshipDef("NativeStat_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_nativestatuscodes_GDE", "DefaultNativeStatus", "NativeStatusCode", OneToOne),
            new RelationshipDef("Protected_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_protectedarea_GDE", "DefaultProtectedArea", "ProtectedArea", OneToOne),
            new RelationshipDef("Wetland_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_wetlandstatus_les_GDE", "DefaultWetlandStatus", "WetlandCode", OneToOne),
            new RelationshipDef("Endemism_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_endemism_GDE", "EndemismLevel", "EndemismLevel", OneToOne),
            new RelationshipDef("SpringHab_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_springmicrohabitatuse_GDE", "SpringHabitatUse", "SpringUse", OneToOne),
            new RelationshipDef("SpLifeHist_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_springlifehistory_GDE", "SpringLifeHistory", "CastSpringLifeHistory", OneToOne),
            new RelationshipDef("Aquatic_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_aquaticstatus_GDE", "AquaticStatus", "AquaticStatus", OneToOne),
            new RelationshipDef("ESA_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_esastatus_GDE", "ESAstatus", "ESAstatus", OneToOne),
            new RelationshipDef("CoverCodes_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_covercodes_TaxaFlora_GDE", "DefaultCoverCode", "CoverCode", OneToOne),
            new RelationshipDef("IUCN_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_iucnstatus_GDE", "IUCNstatus", "IUCNstatus", OneToOne),
            new RelationshipDef("NtnlConsStatus_by_Flora_GDE", "tlu_TaxaFlora_GDE", "tlu_ntnlconservationstatus_GDE", "NtnlConservationStatus", "CastNtnlConservationStatus", OneToOne),
            new RelationshipDef("Sd_Dst_by_Prtcl_Avlnch", "tbl_SadaProtocols", "tlu_sada_disturbance", "AvalancheDisturbance", "distID", OneToOne),
            new RelationshipDef("Sada_Dist_by_Protocol_Fire", "tbl_SadaProtocols", "tlu_sada_disturbance", "FireDisturbance", "distID", OneToOne),
            new RelationshipDef("Sada_Dist_by_Protocol_Flood", "tbl_SadaProtocols", "tlu_sada_disturbance", "FloodDisturbance", "distID", OneToOne),
            new RelationshipDef("Sd_Dst_by_Prtcl_Dvrsn", "tbl_SadaProtocols", "tlu_sada_disturbance", "DiversionDisturbance", "distID", OneToOne),
            new RelationshipDef("Sd_Dst_by_Prtcl_HrsBrr", "tbl_SadaProtocols", "tlu_sada_disturbance", "HorseBurroDisturbance", "distID", OneToOne),
            new RelationshipDef("Sada_Dist_by_Protocol_Cattle", "tbl_SadaProtocols", "tlu_sada_disturbance", "CattleDisturbance", "distID", OneToOne),
            new RelationshipDef("Sd_Dst_by_Prtcl_Rcrtn", "tbl_SadaProtocols", "tlu_sada_disturbance", "RecreationDisturbance", "distID", OneToOne),
            new RelationshipDef("Sada_Dist_by_Protocol_Dredging", "tbl_SadaProtocols", "tlu_sada_disturbance", "DredgingDisturbance", "distID", OneToOne),
            new RelationshipDef("Sd_Dst_by_Prtcl_Rstrtn", "tbl_SadaProtocols", "tlu_sada_disturbance", "RestorationDisturbance", "distID", OneToOne),
            new RelationshipDef("Sada_Dist_by_Protocol_Other", "tbl_SadaProtocols", "tlu_sada_disturbance", "OtherDisturbance", "distID", OneToOne),
            new RelationshipDef("Sada_Dist_by_Protocol_Drought", "tbl_SadaProtocols", "tlu_sada_disturbance", "DroughtDisturbance", "distID", OneToOne),
            new RelationshipDef("Sd_Dst_by_Prtcl_Rsdnc", "tbl_SadaProtocols", "tlu_sada_disturbance", "ResidenceDisturbance", "distID", OneToOne),
        };

        // --- Attribute index field names ------------------------------------
        // From ReturnArrayOfFieldNamesToIndex (SSI_Functions.bas). The three
        // "Placeholder*" slots (FeatureID / OBJECTID / Object_ID) are omitted:
        // they were deliberate non-matches in the legacy and are never indexed.
        // Index creation is "where present" — names absent from a dataset are skipped.

        internal static readonly IReadOnlyList<string> IndexFieldNames = new[]
        {
            "AC1Risk", "AC2Risk", "AC3Risk", "AC4Risk", "AC5Risk", "AC6Risk", "AC7Risk", "AC8Risk", "AC9Risk",
            "AFWQ0Risk", "AFWQ1Risk", "AFWQ2Risk", "AFWQ3Risk", "AFWQ4Risk", "AFWQ5Risk", "AFWQ6Risk",
            "Abbr", "AquaticStatus",
            "BIO1aRisk", "BIO1bRisk", "BIO2aRisk", "BIO2bRisk", "BIO3aRisk", "BIO3bRisk", "BIO4aRisk", "BIO4bRisk",
            "ChannelDynamics", "Country", "County", "CoverCode", "Cultural_ID", "DataChange_ID", "Datum",
            "DefaultCoverCode", "DetectionType", "DischargeSphere", "DisturbanceID", "ESAstatus",
            "EmergenceEnvNote", "EmergenceEnvironment", "EndemismLevel",
            "FHI1Risk", "FHI2Risk", "FHI3Risk", "FHI4Risk", "FHI5Risk", "FHI6Risk", "FHI7Risk", "FHI8Risk", "FHI9Risk",
            "FaunaCommonName", "FaunaFullName", "FaunaObs_ID", "FaunaSpecies", "FaunaSpecies_ID",
            "FieldMsrmntTech", "FillTimeSec", "FilterBy", "FloraCommonName", "FloraSpecies", "FloraSpeciesCode",
            "Flora_ID", "FlowConsistency", "FlowForceMechanism", "FlowID", "FlowRate", "FlowTechnique",
            "FlowVariability", "FlumeMeasure", "Flume_ID", "FullName",
            "GEO1Risk", "GEO2Risk", "GEO3Risk", "GEO4Risk", "GEO5Risk", "GPS_Unit", "GeoLayer", "Georef_Source",
            "GlobalConservationStatus",
            "HAB1Risk", "HAB2Risk", "HAB3Risk", "HAB4Risk", "HAB5Risk", "HUC", "HUC_ID", "ID", "IUCNstatus",
            "IdentificationRef", "Image_ID", "InfoSourceCode", "InvertSample_ID", "Invert_ID",
            "LandUnit", "LandUnitDetail", "LandUnitDetailFull", "LandUnitDetailID", "Lifestage",
            "LithoPrimary", "LithoSecondary", "MeasureRelative", "MeasurementDevice", "Method", "Mgmt_ID",
            "NativeStatusCode", "NearestSpring", "NtnlConservationStatus", "OldFloraSpeciesCode", "OtherVegID",
            "Persistence", "PointNumber", "PolygonID", "Polygon_Code", "ProtectedArea", "ProtocolID", "PurposeID",
            "Quality_ID", "RefGUID", "RefID", "Ref_Author", "Ref_Title", "RepID", "ReportOption", "Report_ID",
            "RiskScore", "SEAPCode", "SEAPScore_ID", "SEAPSummary_ID", "SPFID", "SPFRatio", "SciName", "Sensitive",
            "SiteCode", "SiteID", "SitePolygonID", "SlopeVariability", "SoilMoistureCode", "SourceGeomorphology",
            "SpringLifeHistory", "SpringUse", "State", "StateProvince", "StatusID", "SurfCode", "SurfType",
            "SurveyDate", "SurveyID", "SurveyPolygonAutoID", "TID", "TreatmentAreaID", "UTM_Zone", "UnitName1",
            "UnitName2", "Units", "Unmeasurable", "Volume_ID", "WQLocation_ID", "WQParameter_ID", "WQ_ID",
            "WaterTableID", "WeatherID", "WeirMeasurement", "Weir_ID", "WetlandCode", "distID", "email",
            "lastname", "lccid", "luid", "pid", "pk", "pname", "uid", "username", "HUC8", "HUC12", "Purpose",
            "CastWQParameter", "ATTACHMENTID", "AquaticRank", "AquaticSpeciesTID", "Area_Method",
            "AvalancheDisturbance", "BryoAbundance", "BryophyteRank", "CastFlowRate", "CastGlobalConservationStatus",
            "CastHUC_12", "CastNtnlConservationStatus", "CastProtocolID", "CastSpringLifeHistory",
            "CattleDisturbance", "CutLevelCode", "DefaultNativeStatus", "DefaultProtectedArea", "DefaultWetlandStatus",
            "DiversionDisturbance", "DredgingDisturbance", "DroughtDisturbance", "FireDisturbance", "FloodDisturbance",
            "FloraCoverCode", "FlowPatternIn", "FlowPatternOut", "FlowRateMean", "ForbRank", "ForbSpeciesTID",
            "GlobalID", "GraminoidRank", "GraminoidSpeciesTID", "HorseBurroDisturbance", "Location", "LocationWQ",
            "MsrmtLocation", "NativeStatus", "OtherDisturbance", "Ownership", "ProclaimedNF", "Project",
            "REL_GLOBALID", "REL_OBJECTID", "Rank", "RankDescription", "RecreationDisturbance", "ResidenceDisturbance",
            "RestorationDisturbance", "ResultValueUnits", "SecondaryDischargeSphere", "Sensitivity", "SfcWaterOccur",
            "ShrubRank", "ShrubSpeciesTID", "SoilLocation", "Source", "SpringHabitatUse", "SpringType1", "SpringType2",
            "StateList", "SurveyPolygonID", "SurveyProtocol", "TreeRank", "TreeSpeciesTID", "UnknownRank", "Veg_Surr",
            "WTLocation", "WTSource", "WaterTableType", "Weather", "WetlandStatus", "ludID",
        };
    }
}
