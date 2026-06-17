using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Exports the subset springs feature class to a GPX file. The native
    /// FeaturesToGPX tool is used only for the geometry (and a unique SiteID name
    /// key); everything else is written by post-processing the GPX XML. This
    /// avoids FeaturesToGPX's strict field-type requirements (its elevation field
    /// must be Double and its date field must be Date) and reproduces the legacy
    /// WriteGPX output: composite name plus magvar/cmt/desc/src/link/sym/type.
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
                if (data == null || string.IsNullOrEmpty(data.NameKeyField))
                {
                    return "- GPX export skipped: springs feature class has no SiteID field.";
                }

                // FeaturesToGPX handles geometry + the SiteID name key only. The
                // elevation/date/description fields are intentionally omitted (the
                // tool is type-strict); they are written during post-processing.
                var values = Geoprocessing.MakeValueArray(inFeatures, gpxPath, data.NameKeyField);
                IGPResult result = await Geoprocessing.ExecuteToolAsync("conversion.FeaturesToGPX", values);
                if (result.IsFailed)
                {
                    return $"- GPX export failed: {string.Join("; ", result.Messages.Select(m => m.Text))}";
                }

                int enriched = await Task.Run(() => WriteWaypointElements(gpxPath, data.Waypoints));
                return $"- GPX file: {gpxPath} ({enriched:N0} waypoint(s))";
            }
            catch (Exception ex)
            {
                return $"- GPX export failed: {ex.Message}";
            }
        }

        /// <summary>
        /// Reads, keyed by SiteID text, all values written to each GPX waypoint.
        /// Runs on the MCT.
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
                var data = new GpxData
                {
                    NameKeyField = def.FindField("SiteID") >= 0 ? "SiteID" : string.Empty,
                    Waypoints = new Dictionary<string, Waypoint>(StringComparer.Ordinal),
                };

                int site = def.FindField("SiteID");
                if (site < 0)
                {
                    return data;
                }

                int shortName = def.FindField("ShortName");
                int elev = def.FindField("ElevationM");
                int date = def.FindField("LoginAddedDate");
                int desc = def.FindField("CastSiteDescription");
                int magDev = def.FindField("MagDev");
                int inventory = def.FindField("InventoryLevel");
                int source = def.FindField("infoSourceDetail");
                int url = def.FindField("CastImageHyperlink");
                int type = def.FindField("SpringType1");

                using RowCursor cursor = featureClass.Search(null, false);
                while (cursor.MoveNext())
                {
                    using Row row = cursor.Current;
                    string siteId = KeepSetStore.KeyString(row[site]);
                    if (siteId == null || data.Waypoints.ContainsKey(siteId))
                    {
                        continue;
                    }

                    // GPX waypoint name = "<ShortName> #<SiteID>".
                    string name = shortName >= 0 ? Text(row[shortName]) : null;
                    string composite = string.IsNullOrEmpty(name)
                        ? $"#{siteId}"
                        : $"{name} #{siteId}";

                    data.Waypoints[siteId] = new Waypoint
                    {
                        Name = composite,
                        Elevation = elev >= 0 ? Number(row[elev]) : null,
                        Time = date >= 0 ? IsoTime(row[date]) : null,
                        Description = desc >= 0 ? Text(row[desc]) : null,
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
        /// Replaces each waypoint's children (FeaturesToGPX wrote only the SiteID
        /// name) with the full ordered GPX element set. Returns the count updated.
        /// </summary>
        private static int WriteWaypointElements(string gpxPath, IReadOnlyDictionary<string, Waypoint> waypoints)
        {
            XDocument document = XDocument.Load(gpxPath);

            // Replace the default "Esri" creator on the root <gpx> element.
            document.Root?.SetAttributeValue("creator", "Springs Stewardship Institute");

            int updated = 0;

            foreach (XElement wpt in document.Descendants(Gpx + "wpt").ToList())
            {
                string key = wpt.Element(Gpx + "name")?.Value;
                if (key == null || !waypoints.TryGetValue(key, out Waypoint info))
                {
                    continue;
                }

                // Keep any elevation FeaturesToGPX derived from geometry Z if we
                // don't have an explicit ElevationM value.
                string toolEle = wpt.Element(Gpx + "ele")?.Value;

                wpt.RemoveNodes(); // removes child elements; keeps lat/lon attributes

                // GPX wptType element order: ele, time, magvar, name, cmt, desc, src, link, sym, type.
                string ele = info.Elevation ?? toolEle;
                if (ele != null) wpt.Add(new XElement(Gpx + "ele", ele));
                if (info.Time != null) wpt.Add(new XElement(Gpx + "time", info.Time));
                if (info.MagVar != null) wpt.Add(new XElement(Gpx + "magvar", info.MagVar));
                wpt.Add(new XElement(Gpx + "name", info.Name));
                if (info.Comment != null) wpt.Add(new XElement(Gpx + "cmt", info.Comment));
                if (info.Description != null) wpt.Add(new XElement(Gpx + "desc", info.Description));
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

            string s = RemoveInvalidXmlChars(value.ToString().Trim());
            return s.Length == 0 ? null : s;
        }

        /// <summary>
        /// Strips characters that are illegal in XML 1.0 (e.g. most control
        /// characters). XML-reserved characters such as &amp;, &lt; and &gt; are
        /// NOT touched here — System.Xml.Linq escapes those automatically when the
        /// document is saved.
        /// </summary>
        private static string RemoveInvalidXmlChars(string value)
        {
            if (string.IsNullOrEmpty(value) || value.All(XmlConvert.IsXmlChar))
            {
                return value;
            }

            return new string(value.Where(XmlConvert.IsXmlChar).ToArray());
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
                case short s:
                    return s.ToString(CultureInfo.InvariantCulture);
                default:
                    return Text(value);
            }
        }

        /// <summary>Formats a date value as GPX ISO-8601 UTC, or null if it isn't a usable date.</summary>
        private static string IsoTime(object value)
        {
            const string format = "yyyy-MM-ddTHH:mm:ssZ";
            switch (value)
            {
                case null:
                case DBNull:
                    return null;
                case DateTime dt:
                    return dt.ToUniversalTime().ToString(format, CultureInfo.InvariantCulture);
                case DateTimeOffset dto:
                    return dto.UtcDateTime.ToString(format, CultureInfo.InvariantCulture);
                default:
                    return DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsed)
                        ? parsed.ToString(format, CultureInfo.InvariantCulture)
                        : null;
            }
        }

        private sealed class GpxData
        {
            internal string NameKeyField { get; set; }
            internal Dictionary<string, Waypoint> Waypoints { get; set; }
        }

        private sealed class Waypoint
        {
            internal string Name { get; set; }
            internal string Elevation { get; set; }
            internal string Time { get; set; }
            internal string Description { get; set; }
            internal string MagVar { get; set; }
            internal string Comment { get; set; }
            internal string Source { get; set; }
            internal string Url { get; set; }
            internal string Type { get; set; }
        }
    }
}
