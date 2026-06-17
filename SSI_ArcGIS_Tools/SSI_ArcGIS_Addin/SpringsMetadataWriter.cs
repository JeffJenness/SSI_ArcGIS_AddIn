using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Writes ArcGIS dataset metadata onto each exported dataset from the editable
    /// JSON templates deployed next to SSI_Defaults.gdb (common.json, datasets.json,
    /// domains.json). Coded-value domain code→description lists are read from the
    /// tlu_* lookup tables already copied into the subset (no database connection).
    /// The "{EXPORT_DATE}" token in template text is replaced with the run time.
    /// Runs on the MCT.
    /// </summary>
    internal static class SpringsMetadataWriter
    {
        private const string SpringsKey = "<SPRINGS>";
        private const string ExportDateToken = "{EXPORT_DATE}";

        internal static string Write(
            Geodatabase outputGdb, string gdbPath, string springsName,
            IEnumerable<string> createdDatasets, CancelableProgressor progressor)
        {
            CommonMeta common;
            Dictionary<string, DatasetMeta> datasets;
            Dictionary<string, DomainMap> domains;
            try
            {
                (common, datasets, domains) = LoadTemplates();
            }
            catch (Exception ex)
            {
                return $"- Metadata skipped: could not load templates ({ex.Message}).";
            }

            string exportDate = FormatExportDate(common);
            var lookupCache = new Dictionary<string, List<(string Code, string Desc)>>(StringComparer.OrdinalIgnoreCase);

            int written = 0, failed = 0;
            foreach (string dataset in createdDatasets)
            {
                if (progressor != null && progressor.CancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                string key = dataset.Equals(springsName, StringComparison.OrdinalIgnoreCase) ? SpringsKey : dataset;
                datasets.TryGetValue(key, out DatasetMeta dsMeta);

                try
                {
                    WriteDataset(Path.Combine(gdbPath, dataset), key, common, dsMeta, exportDate,
                        domains, lookupCache, outputGdb);
                    written++;
                }
                catch (Exception)
                {
                    failed++;
                }
            }

            string line = $"- Metadata written to {written} dataset(s)";
            return failed > 0 ? line + $" ({failed} failed)." : line + ".";
        }

        private static void WriteDataset(
            string datasetPath, string key, CommonMeta common, DatasetMeta dsMeta, string exportDate,
            Dictionary<string, DomainMap> domains,
            Dictionary<string, List<(string Code, string Desc)>> lookupCache, Geodatabase gdb)
        {
            Item item = ItemFactory.Instance.Create(datasetPath);
            if (item == null)
            {
                return;
            }

            string existing = item.GetXml();
            XDocument doc;
            try
            {
                doc = string.IsNullOrWhiteSpace(existing)
                    ? new XDocument(new XElement("metadata"))
                    : XDocument.Parse(existing);
            }
            catch (Exception)
            {
                doc = new XDocument(new XElement("metadata"));
            }

            XElement root = doc.Root ?? new XElement("metadata");
            if (doc.Root == null)
            {
                doc.Add(root);
            }

            ApplyCommon(root, common);
            if (dsMeta != null)
            {
                ApplyDatasetText(root, dsMeta, exportDate);
            }

            ApplyContactsAndLineage(root, common, exportDate);
            ApplyFields(root, key, dsMeta, domains, lookupCache, gdb);

            item.SetXml(doc.ToString(SaveOptions.DisableFormatting));
        }

        // --- identification info --------------------------------------------

        private static void ApplyCommon(XElement root, CommonMeta common)
        {
            XElement idInfo = GetOrCreate(root, "dataIdInfo");
            SetText(idInfo, "idCredit", common.Credits);

            XElement consts = GetOrCreate(GetOrCreate(idInfo, "resConst"), "Consts");
            SetText(consts, "useLimit", common.UseLimitations);
        }

        private static void ApplyDatasetText(XElement root, DatasetMeta dsMeta, string exportDate)
        {
            XElement idInfo = GetOrCreate(root, "dataIdInfo");
            SetText(idInfo, "idAbs", Substitute(dsMeta.Abstract, exportDate));
            SetText(idInfo, "idPurp", Substitute(dsMeta.Purpose, exportDate));

            if (dsMeta.Keywords != null && dsMeta.Keywords.Count > 0)
            {
                idInfo.Elements("themeKeys").Remove();
                idInfo.Elements("searchKeys").Remove();
                var theme = new XElement("themeKeys");
                var search = new XElement("searchKeys");
                foreach (string kw in dsMeta.Keywords)
                {
                    theme.Add(new XElement("keyword", Clean(kw)));
                    search.Add(new XElement("keyword", Clean(kw)));
                }

                idInfo.Add(theme);
                idInfo.Add(search);
            }
        }

        private static void ApplyContactsAndLineage(XElement root, CommonMeta common, string exportDate)
        {
            XElement idInfo = GetOrCreate(root, "dataIdInfo");
            idInfo.Elements("idPoC").Remove();
            if (common.Contacts != null)
            {
                foreach (ContactMeta c in common.Contacts)
                {
                    idInfo.Add(BuildContact(c, common));
                }
            }

            XElement lineage = GetOrCreate(GetOrCreate(root, "dqInfo"), "dataLineage");
            lineage.Elements("prcStep").Remove();
            lineage.Add(BuildProcessStep(common.LineageStep, exportDate));
            if (!string.IsNullOrWhiteSpace(common.GeoprocessingDescription))
            {
                lineage.Add(BuildProcessStep(common.GeoprocessingDescription, exportDate));
            }
        }

        private static XElement BuildContact(ContactMeta c, CommonMeta common)
        {
            var address = new XElement("cntAddress", new XAttribute("addressType", "both"));
            if (!string.IsNullOrWhiteSpace(common.Address)) address.Add(new XElement("delPoint", Clean(common.Address)));
            if (!string.IsNullOrWhiteSpace(common.City)) address.Add(new XElement("city", Clean(common.City)));
            if (!string.IsNullOrWhiteSpace(common.State)) address.Add(new XElement("adminArea", Clean(common.State)));
            if (!string.IsNullOrWhiteSpace(common.PostalCode)) address.Add(new XElement("postCode", Clean(common.PostalCode)));
            if (!string.IsNullOrWhiteSpace(common.Country)) address.Add(new XElement("country", Clean(common.Country)));
            if (!string.IsNullOrWhiteSpace(c.Email)) address.Add(new XElement("eMailAdd", Clean(c.Email)));

            var cntInfo = new XElement("rpCntInfo", address);
            if (!string.IsNullOrWhiteSpace(common.Phone))
            {
                cntInfo.Add(new XElement("cntPhone", new XElement("voiceNum", Clean(common.Phone))));
            }

            if (!string.IsNullOrWhiteSpace(common.Website))
            {
                cntInfo.Add(new XElement("cntOnlineRes", new XElement("linkage", Clean(common.Website))));
            }

            var poc = new XElement("idPoC",
                new XElement("rpIndName", Clean(c.Name)),
                new XElement("rpOrgName", Clean(common.Organization)));
            if (!string.IsNullOrWhiteSpace(c.Role)) poc.Add(new XElement("rpPosName", Clean(c.Role)));
            poc.Add(new XElement("role", new XElement("RoleCd", new XAttribute("value", "010"))));
            poc.Add(cntInfo);
            return poc;
        }

        private static XElement BuildProcessStep(string description, string exportDate) =>
            new XElement("prcStep",
                new XElement("stepDesc", Clean(Substitute(description, exportDate))),
                new XElement("stepDateTm", DateTime.Now.ToString("s", CultureInfo.InvariantCulture)));

        // --- entity / attribute info ----------------------------------------

        private static void ApplyFields(
            XElement root, string key, DatasetMeta dsMeta, Dictionary<string, DomainMap> domains,
            Dictionary<string, List<(string Code, string Desc)>> lookupCache, Geodatabase gdb)
        {
            if (dsMeta?.Fields == null || dsMeta.Fields.Count == 0)
            {
                return;
            }

            XElement detailed = GetOrCreate(GetOrCreate(root, "eainfo"), "detailed");

            foreach (var (fieldName, field) in dsMeta.Fields)
            {
                XElement attr = detailed.Elements("attr").FirstOrDefault(
                    a => string.Equals(a.Element("attrlabl")?.Value, fieldName, StringComparison.OrdinalIgnoreCase));
                if (attr == null)
                {
                    attr = new XElement("attr", new XElement("attrlabl", fieldName));
                    detailed.Add(attr);
                }

                if (!string.IsNullOrWhiteSpace(field.Description)) SetText(attr, "attrdef", Clean(field.Description));
                if (!string.IsNullOrWhiteSpace(field.Source)) SetText(attr, "attrdefs", Clean(field.Source));

                if (domains.TryGetValue($"{key}:{fieldName}", out DomainMap map))
                {
                    AddDomain(attr, map, lookupCache, gdb);
                }
            }
        }

        private static void AddDomain(
            XElement attr, DomainMap map,
            Dictionary<string, List<(string Code, string Desc)>> lookupCache, Geodatabase gdb)
        {
            if (string.IsNullOrWhiteSpace(map.DescField) ||
                map.DescField.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            List<(string Code, string Desc)> values = ReadLookup(gdb, map, lookupCache);
            if (values.Count == 0)
            {
                return;
            }

            attr.Elements("attrdomv").Remove();
            var domv = new XElement("attrdomv");
            foreach (var (code, desc) in values)
            {
                domv.Add(new XElement("edom",
                    new XElement("edomv", Clean(code)),
                    new XElement("edomvd", Clean(desc)),
                    new XElement("edomvds", Clean(map.Source))));
            }

            attr.Add(domv);
        }

        private static List<(string Code, string Desc)> ReadLookup(
            Geodatabase gdb, DomainMap map, Dictionary<string, List<(string Code, string Desc)>> cache)
        {
            string cacheKey = $"{map.LookupTable}|{map.CodeField}|{map.DescField}";
            if (cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var values = new List<(string, string)>();
            try
            {
                using Table table = gdb.OpenDataset<Table>(map.LookupTable);
                TableDefinition def = table.GetDefinition();
                if (def.FindField(map.CodeField) >= 0 && def.FindField(map.DescField) >= 0)
                {
                    using RowCursor cursor = table.Search(null, false);
                    while (cursor.MoveNext())
                    {
                        using Row row = cursor.Current;
                        string code = Convert.ToString(row[map.CodeField], CultureInfo.InvariantCulture);
                        string desc = Convert.ToString(row[map.DescField], CultureInfo.InvariantCulture);
                        if (!string.IsNullOrEmpty(code))
                        {
                            values.Add((code, desc ?? string.Empty));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Lookup table absent in this subset; leave the domain empty.
            }

            cache[cacheKey] = values;
            return values;
        }

        // --- helpers --------------------------------------------------------

        private static (CommonMeta, Dictionary<string, DatasetMeta>, Dictionary<string, DomainMap>) LoadTemplates()
        {
            string dir = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Metadata");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            CommonMeta common = JsonSerializer.Deserialize<CommonMeta>(
                File.ReadAllText(Path.Combine(dir, "common.json")), options) ?? new CommonMeta();
            var datasets = JsonSerializer.Deserialize<Dictionary<string, DatasetMeta>>(
                File.ReadAllText(Path.Combine(dir, "datasets.json")), options) ?? new();
            var domains = JsonSerializer.Deserialize<Dictionary<string, DomainMap>>(
                File.ReadAllText(Path.Combine(dir, "domains.json")), options) ?? new();
            return (common, datasets, domains);
        }

        private static string FormatExportDate(CommonMeta common)
        {
            string format = string.IsNullOrWhiteSpace(common.ExportDateFormat)
                ? "h:mm:ss 'hrs', dddd, MMMM d, yyyy"
                : common.ExportDateFormat;
            try
            {
                return DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return DateTime.Now.ToString(CultureInfo.InvariantCulture);
            }
        }

        private static string Substitute(string text, string exportDate) =>
            string.IsNullOrEmpty(text) ? text : text.Replace(ExportDateToken, exportDate);

        private static XElement GetOrCreate(XElement parent, string name)
        {
            XElement child = parent.Element(name);
            if (child == null)
            {
                child = new XElement(name);
                parent.Add(child);
            }

            return child;
        }

        private static void SetText(XElement parent, string name, string value) =>
            GetOrCreate(parent, name).Value = Clean(value) ?? string.Empty;

        /// <summary>Removes XML-illegal characters (reserved chars are auto-escaped on save).</summary>
        private static string Clean(string value)
        {
            if (string.IsNullOrEmpty(value) || value.All(XmlConvert.IsXmlChar))
            {
                return value;
            }

            return new string(value.Where(XmlConvert.IsXmlChar).ToArray());
        }

        // --- template models ------------------------------------------------

        private sealed class CommonMeta
        {
            [JsonPropertyName("credits")] public string Credits { get; set; }
            [JsonPropertyName("useLimitations")] public string UseLimitations { get; set; }
            [JsonPropertyName("organization")] public string Organization { get; set; }
            [JsonPropertyName("address")] public string Address { get; set; }
            [JsonPropertyName("city")] public string City { get; set; }
            [JsonPropertyName("state")] public string State { get; set; }
            [JsonPropertyName("postalCode")] public string PostalCode { get; set; }
            [JsonPropertyName("country")] public string Country { get; set; }
            [JsonPropertyName("phone")] public string Phone { get; set; }
            [JsonPropertyName("website")] public string Website { get; set; }
            [JsonPropertyName("contacts")] public List<ContactMeta> Contacts { get; set; }
            [JsonPropertyName("lineageStep")] public string LineageStep { get; set; }
            [JsonPropertyName("geoprocessingDescription")] public string GeoprocessingDescription { get; set; }
            [JsonPropertyName("exportDateFormat")] public string ExportDateFormat { get; set; }
        }

        private sealed class ContactMeta
        {
            [JsonPropertyName("name")] public string Name { get; set; }
            [JsonPropertyName("role")] public string Role { get; set; }
            [JsonPropertyName("email")] public string Email { get; set; }
        }

        private sealed class DatasetMeta
        {
            [JsonPropertyName("abstract")] public string Abstract { get; set; }
            [JsonPropertyName("purpose")] public string Purpose { get; set; }
            [JsonPropertyName("keywords")] public List<string> Keywords { get; set; }
            [JsonPropertyName("fields")] public Dictionary<string, FieldMeta> Fields { get; set; }
        }

        private sealed class FieldMeta
        {
            [JsonPropertyName("description")] public string Description { get; set; }
            [JsonPropertyName("source")] public string Source { get; set; }
            [JsonPropertyName("units")] public string Units { get; set; }
        }

        private sealed class DomainMap
        {
            [JsonPropertyName("lookupTable")] public string LookupTable { get; set; }
            [JsonPropertyName("codeField")] public string CodeField { get; set; }
            [JsonPropertyName("descField")] public string DescField { get; set; }
            [JsonPropertyName("source")] public string Source { get; set; }
        }
    }
}
