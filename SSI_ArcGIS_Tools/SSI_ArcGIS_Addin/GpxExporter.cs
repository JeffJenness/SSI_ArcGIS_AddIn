using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Exports the subset springs feature class to a GPX file. The native
    /// FeaturesToGPX tool only writes name/elevation/time/description, so this
    /// runs it with SiteID as the waypoint name (a unique key) and then
    /// post-processes the GPX XML to rewrite the name into the legacy composite
    /// ("ShortName [SiteID = N]") and inject the extra waypoint elements the old
    /// WriteGPX produced: magvar, cmt, src, link, sym, type.
    /// </summary>
    internal static class GpxExporter
    {
        private static readonly XNamespace Gpx = "http://www.topografix.com/GPX/1/1";
        private const string SymbolValue = "Blue Circle";
        private const string LinkText = "Image of Spring";

        internal static async Task<string> ExportAsync(string outputGeodatabasePath, string springsName)
        {
            try
            {
                string inFeatures = Path.Combine(outputGeodatabasePath, springsName);
                string gpxPath = Path.Combine(
                    Path.GetDirectoryName(outputGeodatabasePath) ?? string.Empty, springsName + ".gpx");

                GpxData data = await QueuedTask.Run(() => ReadGpxData(outputGeodatabasePath, springsName));
                if (data == null)
                {
                    return "- GPX export skipped: springs feature class not found.";
                }

                // Run FeaturesToGPX with SiteID as <name> so waypoints can be matched.
                var values = Geoprocessing.MakeValueArray(
                    inFeatures, gpxPath, data.NameKeyField, data.ElevationField, data.DateField, data.DescriptionField);
                IGPResult result = await Geoprocessing.ExecuteToolAsync("conversion.FeaturesToGPX", values);
                if (result.IsFailed)
                {
                    return $"- GPX export failed: {string.Join("; ", result.Messages.Select(m => m.Text))}";
                }

                int enriched = await Task.Run(() => InjectExtraElements(gpxPath, data.Waypoints));
                return $"- GPX file: {gpxPath} ({enriched:N0} waypoint(s))";
            }
            catch (Exception ex)
            {
                return $"- GPX export failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Reads the GPX field mappings and the per-spring extra-element values
        /// (keyed by SiteID text) from the output springs feature class. MCT.
        /// </summary>
        private static GpxData ReadGpxData(string gdbPath, string springsName)
        {
            using var geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(gdbPath)));
            FeatureClass featureClass;
            try
            {
                featureClass = geodatabase.OpenDataset<FeatureClass>(springsName);
            }
            catch (Exception)
            {
                return null;
            }

            using (featureClass)
            {
                FeatureClassDefinition def = featureClass.GetDefinition();
                string Present(string field) => def.FindField(field) >= 0 ? field : string.Empty;

                var data = new GpxData
                {
                    NameKeyField = Present("SiteID"),
                    ElevationField = Present("ElevationM"),
                    DateField = Present("LoginAddedDate"),
                    DescriptionField = Present("CastSiteDescription"),
                    Waypoints = new Dictionary<string, Waypoint>(StringComparer.Ordinal),
                };

                int site = def.FindField("SiteID");
                int shortName = def.FindField("ShortName");
                int magDev = def.FindField("MagDev");
                int inventory = def.FindField("InventoryLevel");
                int source = def.FindField("infoSourceDetail");
                int url = def.FindField("CastImageHyperlink");
                int type = def.FindField("SpringType1");

                if (site < 0)
                {
                    return data;
                }

                using RowCursor cursor = featureClass.Search(null, false);
                while (cursor.MoveNext())
                {
                    using Row row = cursor.Current;
                    string siteId = KeepSetStore.KeyString(row[site]);
                    if (siteId == null || data.Waypoints.ContainsKey(siteId))
                    {
                        continue;
                    }

                    string name = shortName >= 0 ? Text(row[shortName]) : null;
                    string composite = string.IsNullOrEmpty(name)
                        ? $"[SiteID = {siteId}]"
                        : $"{name} [SiteID = {siteId}]";

                    data.Waypoints[siteId] = new Waypoint
                    {
                        Name = composite,
                        MagVar = magDev >= 0 ? Number(row[magDev]) : null,
                        Comment = inventory >= 0 ? Text(row[inventory]) : null,
                        Source = source >= 0 ? Text(row[source]) : null,
                        Url = url >= 0 ? Text(row[url]) : null,
                        Type = type >= 0 ? Text(row[type]) : null,
                    };
                }

                return data;
            }
        }

        /// <summary>
        /// Rewrites each waypoint's name to the composite and inserts the extra
        /// elements in GPX-schema order. Returns the number of waypoints updated.
        /// </summary>
        private static int InjectExtraElements(string gpxPath, IReadOnlyDictionary<string, Waypoint> waypoints)
        {
            XDocument document = XDocument.Load(gpxPath);
            int updated = 0;

            foreach (XElement wpt in document.Descendants(Gpx + "wpt").ToList())
            {
                string key = wpt.Element(Gpx + "name")?.Value;
                if (key == null || !waypoints.TryGetValue(key, out Waypoint info))
                {
                    continue;
                }

                // Preserve what FeaturesToGPX wrote, then rebuild children in order:
                // ele, time, magvar, name, cmt, desc, src, link, sym, type.
                string ele = wpt.Element(Gpx + "ele")?.Value;
                string time = wpt.Element(Gpx + "time")?.Value;
                string desc = wpt.Element(Gpx + "desc")?.Value;

                wpt.RemoveNodes(); // removes child elements; keeps lat/lon attributes

                if (ele != null) wpt.Add(new XElement(Gpx + "ele", ele));
                if (time != null) wpt.Add(new XElement(Gpx + "time", time));
                if (info.MagVar != null) wpt.Add(new XElement(Gpx + "magvar", info.MagVar));
                wpt.Add(new XElement(Gpx + "name", info.Name));
                if (info.Comment != null) wpt.Add(new XElement(Gpx + "cmt", info.Comment));
                if (desc != null) wpt.Add(new XElement(Gpx + "desc", desc));
                if (info.Source != null) wpt.Add(new XElement(Gpx + "src", info.Source));
                if (info.Url != null)
                {
                    wpt.Add(new XElement(Gpx + "link",
                        new XAttribute("href", info.Url),
                        new XElement(Gpx + "text", LinkText)));
                }

                wpt.Add(new XElement(Gpx + "sym", SymbolValue));
                if (info.Type != null) wpt.Add(new XElement(Gpx + "type", info.Type));

                updated++;
            }

            document.Save(gpxPath);
            return updated;
        }

        private static string Text(object value)
        {
            if (value == null || value is DBNull)
            {
                return null;
            }

            string s = value.ToString().Trim();
            return s.Length == 0 ? null : s;
        }

        private static string Number(object value)
        {
            switch (value)
            {
                case null:
                case DBNull:
                    return null;
                case double d:
                    return d.ToString(CultureInfo.InvariantCulture);
                case float f:
                    return f.ToString(CultureInfo.InvariantCulture);
                case decimal m:
                    return m.ToString(CultureInfo.InvariantCulture);
                case int i:
                    return i.ToString(CultureInfo.InvariantCulture);
                case long l:
                    return l.ToString(CultureInfo.InvariantCulture);
                default:
                    return Text(value);
            }
        }

        private sealed class GpxData
        {
            internal string NameKeyField { get; set; }
            internal string ElevationField { get; set; }
            internal string DateField { get; set; }
            internal string DescriptionField { get; set; }
            internal Dictionary<string, Waypoint> Waypoints { get; set; }
        }

        private sealed class Waypoint
        {
            internal string Name { get; set; }
            internal string MagVar { get; set; }
            internal string Comment { get; set; }
            internal string Source { get; set; }
            internal string Url { get; set; }
            internal string Type { get; set; }
        }
    }
}
