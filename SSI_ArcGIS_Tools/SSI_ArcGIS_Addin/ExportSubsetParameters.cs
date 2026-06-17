using System.Collections.Generic;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Immutable, WPF-independent inputs for a subset export, produced by the
    /// parameter dialog and consumed by <see cref="SpringsSubsetExporter"/>.
    /// </summary>
    internal sealed class ExportSubsetParameters
    {
        /// <summary>Path to the source file geodatabase (the springs layer's geodatabase).</summary>
        internal string SourceGeodatabasePath { get; init; }

        /// <summary>Name of the springs feature class within the source geodatabase.</summary>
        internal string SpringsFeatureClassName { get; init; }

        /// <summary>
        /// ObjectIDs of the springs to export (the layer's selection). Null or
        /// empty means export every spring.
        /// </summary>
        internal IReadOnlyList<long> SelectedObjectIds { get; init; }

        /// <summary>Optional SQL where-clause (against the springs FC) selecting SiteIDs whose surveys are excluded.</summary>
        internal string ExcludeSitesWhereClause { get; init; }

        /// <summary>Optional SQL where-clause (against tbl_Surveys) selecting SurveyIDs to exclude.</summary>
        internal string ExcludeSurveysWhereClause { get; init; }

        /// <summary>Existing folder in which the new output file geodatabase is created.</summary>
        internal string OutputFolder { get; init; }

        /// <summary>
        /// Base name (no spaces, no extension) for the output geodatabase and the
        /// springs feature class inside it.
        /// </summary>
        internal string OutputName { get; init; }

        /// <summary>When true, shrink text fields to the longest actual value found.</summary>
        internal bool TrimStrings { get; init; }

        /// <summary>
        /// When true, also build the denormalized summary feature class
        /// (<see cref="OutputName"/> + "_Summary") in the output geodatabase.
        /// </summary>
        internal bool CreateSummary { get; init; }

        /// <summary>
        /// When true, also export the subset springs to a GPX file
        /// (<see cref="OutputName"/> + ".gpx") next to the output geodatabase.
        /// </summary>
        internal bool CreateGpx { get; init; }
    }
}
