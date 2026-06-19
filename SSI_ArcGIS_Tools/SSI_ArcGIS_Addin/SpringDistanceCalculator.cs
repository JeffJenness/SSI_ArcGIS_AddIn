using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using JennessentOps;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Thread-agnostic inputs for the Nearest Spring Distances analysis. The
    /// <see cref="Layer"/> is read on the MCT inside <see cref="SpringDistanceCalculator"/>.
    /// </summary>
    internal sealed class SpringDistanceParameters
    {
        public FeatureLayer Layer { get; set; }
        public string SiteIdField { get; set; }
        public string InvLevelField { get; set; }

        /// <summary>Analyze every feature (true) or only the layer's current selection (false).</summary>
        public bool AnalyzeAll { get; set; }

        public string OutputCsvPath { get; set; }

        public bool IncludeNames { get; set; }
        public bool IncludeElevations { get; set; }
        public bool IncludeDate { get; set; }
        public bool IncludeInfoSource { get; set; }
        public bool IncludeInvLevel { get; set; }
    }

    /// <summary>
    /// Calculates, for each spring (focal point), the geodesic (Vincenty) distance
    /// to the nearest <em>other</em> spring whose Inventory Level is not "No Spring",
    /// writing the results to a CSV file. Faithful C# port of the legacy VB6
    /// <c>SSI_Functions.CalculateDistanceToSprings</c>: it unprojects to geographic
    /// coordinates, sorts the comparison set by latitude, and sweeps north/south
    /// from each focal point, bailing out once the vertical separation exceeds the
    /// best planar distance found so far. Runs on the MCT.
    /// </summary>
    internal sealed class SpringDistanceCalculator
    {
        private const string NotASpringValue = "No Spring";

        private readonly SpringDistanceParameters _p;

        internal SpringDistanceCalculator(SpringDistanceParameters parameters)
        {
            _p = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>Number of focal springs actually analyzed (for the report).</summary>
        internal long AnalyzedCount { get; private set; }

        /// <summary>
        /// Runs the analysis and writes the CSV. Returns a plain-text completion
        /// report. Runs on the MCT.
        /// </summary>
        internal string Run(CancelableProgressor progressor)
        {
            DateTime started = DateTime.Now;
            bool includeAdditional = _p.IncludeNames || _p.IncludeElevations ||
                                     _p.IncludeDate || _p.IncludeInfoSource || _p.IncludeInvLevel;

            using FeatureClass featureClass = _p.Layer.GetFeatureClass();
            FeatureClassDefinition definition = featureClass.GetDefinition();

            int siteIdIndex = definition.FindField(_p.SiteIdField);
            int invLevIndex = definition.FindField(_p.InvLevelField);
            int nameIndex = definition.FindField("SiteName");
            int elevIndex = definition.FindField("ElevationM");
            int dateIndex = definition.FindField("LoginAddedDate");
            int infoSourceIndex = definition.FindField("infoSource");
            int infoSourceDetailIndex = definition.FindField("infoSourceDetail");

            if (siteIdIndex < 0 || invLevIndex < 0)
            {
                throw new InvalidOperationException(
                    "The selected SiteID or Inventory Level field could not be found on the layer.");
            }

            // Unproject to geographic coordinates if the source is projected, so the
            // sweep (which works in coordinate-Y order) and the Vincenty distance
            // both operate on longitude / latitude degrees.
            SpatialReference sourceSr = definition.GetSpatialReference();
            bool shouldUnproject = sourceSr != null && sourceSr.IsProjected;
            SpatialReference geographicSr = shouldUnproject ? sourceSr.Gcs : sourceSr;

            long totalCount = featureClass.GetCount();

            // --- Pass 1: build the comparison set (all features) -----------------
            SetProgress(progressor, "Initial data preparation...");

            var compareRows = new List<double[]>((int)Math.Min(totalCount, int.MaxValue));
            var additional = includeAdditional
                ? new Dictionary<string, AddData>()
                : null;

            using (RowCursor cursor = featureClass.Search(null, false))
            {
                while (cursor.MoveNext())
                {
                    ThrowIfCancelled(progressor);
                    using Feature feature = (Feature)cursor.Current;

                    double compareId = ToDouble(feature[siteIdIndex]);
                    string invLevel = ToStringValue(feature[invLevIndex]);

                    if (includeAdditional)
                    {
                        var data = new AddData();
                        if (_p.IncludeNames) { data.Name = nameIndex >= 0 ? ToStringValue(feature[nameIndex]) : ""; }
                        if (_p.IncludeElevations) { data.Elevation = ReadElevation(feature, elevIndex); }
                        if (_p.IncludeDate) { data.Date = ReadDate(feature, dateIndex); }
                        if (_p.IncludeInfoSource) { data.InfoSource = ReadInfoSource(feature, infoSourceIndex, infoSourceDetailIndex); }
                        data.InvLevel = invLevel;
                        additional[KeyOf(compareId)] = data;
                    }

                    if (feature.GetShape() is not MapPoint point)
                    {
                        continue;
                    }
                    if (shouldUnproject)
                    {
                        point = (MapPoint)GeometryEngine.Instance.Project(point, geographicSr);
                    }

                    double posFlag = string.Equals(invLevel, NotASpringValue, StringComparison.OrdinalIgnoreCase) ? -1 : 1;
                    compareRows.Add(new[] { point.X, point.Y, compareId, posFlag });
                }
            }

            int compareCount = compareRows.Count;
            double[,] compareArray = new double[Math.Max(compareCount, 1), 4];
            for (int i = 0; i < compareCount; i++)
            {
                compareArray[i, 0] = compareRows[i][0];
                compareArray[i, 1] = compareRows[i][1];
                compareArray[i, 2] = compareRows[i][2];
                compareArray[i, 3] = compareRows[i][3];
            }

            // Sort the comparison set by latitude (column 1), carrying all 4 columns.
            if (compareCount > 1)
            {
                JenQuickSort.Ascending_2Dimensional(compareArray, 0, compareCount - 1, 1, 3);
            }

            // Map each SiteID to its (first) row in the sorted comparison set.
            var compareLinks = new Dictionary<string, int>();
            for (int i = 0; i < compareCount; i++)
            {
                string key = KeyOf(compareArray[i, 2]);
                if (!compareLinks.ContainsKey(key))
                {
                    compareLinks[key] = i;
                }
            }

            // --- Pass 2: build the focal set (all or selected features) ----------
            QueryFilter focalFilter = null;
            long reportCount = totalCount;
            if (!_p.AnalyzeAll)
            {
                IReadOnlyList<long> oids = _p.Layer.GetSelection()?.GetObjectIDs();
                if (oids != null && oids.Count > 0)
                {
                    focalFilter = new QueryFilter { ObjectIDs = oids };
                    reportCount = oids.Count;
                }
            }

            var focalRows = new List<double[]>();
            using (RowCursor cursor = featureClass.Search(focalFilter, false))
            {
                while (cursor.MoveNext())
                {
                    ThrowIfCancelled(progressor);
                    using Feature feature = (Feature)cursor.Current;
                    if (feature.GetShape() is not MapPoint point)
                    {
                        continue;
                    }
                    if (shouldUnproject)
                    {
                        point = (MapPoint)GeometryEngine.Instance.Project(point, geographicSr);
                    }
                    focalRows.Add(new[] { point.X, point.Y, ToDouble(feature[siteIdIndex]) });
                }
            }

            // --- Pass 3: find the nearest qualifying spring for each focal point --
            if (progressor != null)
            {
                progressor.Max = (uint)Math.Max(focalRows.Count, 1);
                progressor.Value = 0;
            }
            SetProgress(progressor, "Finding nearest springs...");

            Directory.CreateDirectory(Path.GetDirectoryName(_p.OutputCsvPath));
            using var writer = new StreamWriter(_p.OutputCsvPath, false, new UTF8Encoding(true));
            writer.WriteLine(BuildHeader(includeAdditional));

            int lastCompareIndex = compareCount - 1;
            for (int f = 0; f < focalRows.Count; f++)
            {
                ThrowIfCancelled(progressor);
                Step(progressor);

                double startX = focalRows[f][0];
                double startY = focalRows[f][1];
                double siteId = focalRows[f][2];

                double minDist = double.MaxValue;     // best geodesic (Vincenty) distance, metres
                double minNaiveDist = double.MaxValue; // planar distance (degrees) of the current best
                double nearId = -999;

                if (compareLinks.TryGetValue(KeyOf(siteId), out int keyIndex))
                {
                    bool doneNorth = false;
                    bool doneSouth = false;

                    for (int step = 1; step <= lastCompareIndex; step++)
                    {
                        if (doneNorth && doneSouth) { break; }

                        // Sweep north (increasing latitude / index).
                        if (!doneNorth)
                        {
                            int northIndex = keyIndex + step;
                            if (northIndex <= lastCompareIndex)
                            {
                                double endX = compareArray[northIndex, 0];
                                double endY = compareArray[northIndex, 1];
                                if (compareArray[northIndex, 3] == 1)
                                {
                                    double naive = Math.Sqrt(((startX - endX) * (startX - endX)) + ((startY - endY) * (startY - endY)));
                                    double dist = MyGeometricOps.DistanceVincentyNumbers(startX, startY, endX, endY);
                                    if (dist < minDist)
                                    {
                                        nearId = compareArray[northIndex, 2];
                                        minDist = dist;
                                        minNaiveDist = naive;
                                    }
                                }
                                if (Math.Abs(endY - startY) > minNaiveDist) { doneNorth = true; }
                            }
                            else
                            {
                                doneNorth = true;
                            }
                        }

                        // Sweep south (decreasing latitude / index).
                        if (!doneSouth)
                        {
                            int southIndex = keyIndex - step;
                            if (southIndex >= 0)
                            {
                                double endX = compareArray[southIndex, 0];
                                double endY = compareArray[southIndex, 1];
                                if (compareArray[southIndex, 3] == 1)
                                {
                                    double naive = Math.Sqrt(((startX - endX) * (startX - endX)) + ((startY - endY) * (startY - endY)));
                                    double dist = MyGeometricOps.DistanceVincentyNumbers(startX, startY, endX, endY);
                                    if (dist < minDist)
                                    {
                                        nearId = compareArray[southIndex, 2];
                                        minDist = dist;
                                        minNaiveDist = naive;
                                    }
                                }
                                if (Math.Abs(endY - startY) > minNaiveDist) { doneSouth = true; }
                            }
                            else
                            {
                                doneSouth = true;
                            }
                        }
                    }
                }

                writer.WriteLine(BuildLine(includeAdditional, siteId, nearId, minDist, additional));
            }

            AnalyzedCount = focalRows.Count;
            return BuildReport(started, reportCount, totalCount);
        }

        // --- Output formatting ---------------------------------------------------

        private string BuildHeader(bool includeAdditional)
        {
            if (!includeAdditional)
            {
                return "\"SiteID\",\"NearestSpringM\",\"Nearest_SiteID\"";
            }

            var sb = new StringBuilder();
            sb.Append("\"SiteID\",");
            if (_p.IncludeNames) { sb.Append("\"SiteName\","); }
            if (_p.IncludeElevations) { sb.Append("\"SiteElevation\","); }
            if (_p.IncludeDate) { sb.Append("\"SiteDateEntered\","); }
            if (_p.IncludeInfoSource) { sb.Append("\"Info_Source_Data\","); }
            if (_p.IncludeInvLevel) { sb.Append("\"Inventory_Level\","); }
            sb.Append("\"NearestSpringM\",\"Nearest_SiteID\",");
            if (_p.IncludeNames) { sb.Append("\"Nearest_SiteName\","); }
            if (_p.IncludeElevations) { sb.Append("\"Nearest_SiteElevation\","); }
            if (_p.IncludeDate) { sb.Append("\"Nearest_SiteDateEntered\","); }
            if (_p.IncludeInfoSource) { sb.Append("\"Nearest_Info_Source_Data\","); }
            if (_p.IncludeInvLevel) { sb.Append("\"Nearest_Inventory_Level\","); }
            return TrimTrailingComma(sb.ToString());
        }

        private string BuildLine(bool includeAdditional, double siteId, double nearId, double minDist,
            Dictionary<string, AddData> additional)
        {
            string nearestDistText = minDist == double.MaxValue ? "" : minDist.ToString(CultureInfo.InvariantCulture);
            string nearestIdText = nearId == -999 ? "-999" : KeyOf(nearId);

            if (!includeAdditional)
            {
                return $"{KeyOf(siteId)},{nearestDistText},{nearestIdText}";
            }

            var sb = new StringBuilder();
            AddData focal = Lookup(additional, siteId);
            AddData nearest = Lookup(additional, nearId);

            sb.Append(KeyOf(siteId)).Append(',');
            AppendData(sb, focal);

            sb.Append(nearestDistText).Append(',').Append(nearestIdText).Append(',');
            AppendData(sb, nearest);

            return TrimTrailingComma(sb.ToString());
        }

        private void AppendData(StringBuilder sb, AddData data)
        {
            if (_p.IncludeNames) { sb.Append(Quote(data?.Name ?? "")).Append(','); }
            if (_p.IncludeElevations) { sb.Append((data?.Elevation ?? -999).ToString(CultureInfo.InvariantCulture)).Append(','); }
            if (_p.IncludeDate) { sb.Append(Quote(data != null ? data.Date.ToString(CultureInfo.InvariantCulture) : "")).Append(','); }
            if (_p.IncludeInfoSource) { sb.Append(Quote(data?.InfoSource ?? "")).Append(','); }
            if (_p.IncludeInvLevel) { sb.Append(Quote(data?.InvLevel ?? "")).Append(','); }
        }

        private string BuildReport(DateTime started, long reportCount, long totalCount)
        {
            string selOption = _p.AnalyzeAll
                ? $"Analyze All Springs [n = {reportCount:N0}]"
                : $"Analyze Selected Springs [n = {reportCount:N0} of {totalCount:N0}]";

            TimeSpan elapsed = DateTime.Now - started;
            var sb = new StringBuilder();
            sb.AppendLine("Springs Distance Analysis Complete");
            sb.AppendLine();
            sb.AppendLine($"Springs Feature Class: {_p.Layer.Name}");
            sb.AppendLine($"    Springs ID Field: {_p.SiteIdField}");
            sb.AppendLine($"    Inventory Level Field: {_p.InvLevelField}");
            sb.AppendLine($"    Selection Option: {selOption}");
            sb.AppendLine($"    Include Spring Names: {_p.IncludeNames.ToString().ToUpperInvariant()}");
            sb.AppendLine($"    Include Elevations: {_p.IncludeElevations.ToString().ToUpperInvariant()}");
            sb.AppendLine($"    Include Date Entered: {_p.IncludeDate.ToString().ToUpperInvariant()}");
            sb.AppendLine($"    Include Info Source data: {_p.IncludeInfoSource.ToString().ToUpperInvariant()}");
            sb.AppendLine($"    Include Inventory Level data: {_p.IncludeInvLevel.ToString().ToUpperInvariant()}");
            sb.AppendLine();
            sb.AppendLine($"CSV File Saved to: {_p.OutputCsvPath}");
            sb.AppendLine();
            sb.AppendLine($"Time elapsed: {elapsed:hh\\:mm\\:ss}");
            return sb.ToString();
        }

        // --- Value helpers -------------------------------------------------------

        private static AddData Lookup(Dictionary<string, AddData> additional, double id)
        {
            if (additional != null && additional.TryGetValue(KeyOf(id), out AddData data))
            {
                return data;
            }
            return null;
        }

        private string ReadInfoSource(Feature feature, int sourceIndex, int detailIndex)
        {
            string source = sourceIndex >= 0 ? ToStringValue(feature[sourceIndex]).Trim() : "";
            string detail = detailIndex >= 0 ? ToStringValue(feature[detailIndex]).Trim() : "";
            if (source.Length == 0) { source = "[No Info Source]"; }
            if (detail.Length == 0) { detail = "[No Info Source Details]"; }
            return source + ": " + detail;
        }

        private static double ReadElevation(Feature feature, int index)
        {
            if (index < 0) { return -999; }
            object v = feature[index];
            return v == null || v == DBNull.Value ? -999 : ToDouble(v);
        }

        private static DateTime ReadDate(Feature feature, int index)
        {
            if (index < 0) { return new DateTime(1900, 1, 1); }
            object v = feature[index];
            return v is DateTime dt ? dt : new DateTime(1900, 1, 1);
        }

        private static double ToDouble(object value)
        {
            return value == null || value == DBNull.Value ? 0 : Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        private static string ToStringValue(object value)
        {
            return value == null || value == DBNull.Value ? "" : value.ToString();
        }

        /// <summary>Integer-valued SiteID formatted with no decimals (the legacy key form).</summary>
        private static string KeyOf(double id)
        {
            return ((long)Math.Round(id)).ToString(CultureInfo.InvariantCulture);
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        }

        private static string TrimTrailingComma(string text)
        {
            return text.EndsWith(",", StringComparison.Ordinal) ? text.Substring(0, text.Length - 1) : text;
        }

        private static void SetProgress(CancelableProgressor progressor, string message)
        {
            if (progressor != null) { progressor.Message = message; }
        }

        private static void Step(CancelableProgressor progressor)
        {
            if (progressor != null) { progressor.Value += 1; }
        }

        private static void ThrowIfCancelled(CancelableProgressor progressor)
        {
            if (progressor != null && progressor.CancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        /// <summary>Per-spring extra attributes, cached by SiteID for both focal and nearest output.</summary>
        private sealed class AddData
        {
            public string Name { get; set; } = "";
            public double Elevation { get; set; } = -999;
            public DateTime Date { get; set; } = new DateTime(1900, 1, 1);
            public string InfoSource { get; set; } = "";
            public string InvLevel { get; set; } = "";
        }
    }
}
