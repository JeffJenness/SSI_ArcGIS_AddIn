using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Creates layer files (.lyrx) next to an exported geodatabase that load and
    /// symbolize the summary feature class. Each bundled template under
    /// <c>LayerTemplates\</c> was authored against a different summary feature class
    /// (nested group layers + per-class definition queries on Flow/pH/Spec-Cond/
    /// Temperature/Spring-Type); this retargets every data connection to the new
    /// summary feature class using a path RELATIVE to the layer file
    /// (".\&lt;gdb&gt;") so each one always resolves the geodatabase in its own folder.
    /// Each output keeps the template's file name. The summary FC schema is identical
    /// across exports, so the symbology and definition queries carry over unchanged.
    /// </summary>
    internal static class SummaryLayerFileWriter
    {
        private const string TemplateFolder = "LayerTemplates";

        /// <summary>
        /// Writes one retargeted layer file per bundled template into the
        /// geodatabase's folder. Returns the created .lyrx paths (empty if the
        /// templates folder is missing). A template with an unexpected shape or a
        /// read error is skipped without aborting the rest. Runs on the MCT.
        /// </summary>
        internal static IReadOnlyList<string> Write(string outputGdbPath, string summaryFeatureClassName)
        {
            var created = new List<string>();

            string installDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            string templateDir = Path.Combine(installDir, TemplateFolder);
            if (!Directory.Exists(templateDir))
            {
                return created;
            }

            string folder = Path.GetDirectoryName(outputGdbPath);
            string gdbFileName = Path.GetFileName(outputGdbPath);                       // e.g. Springs_Subset.gdb
            // Relative connection so each layer file finds the gdb in its own folder.
            string relativeConnection = "DATABASE=.\\" + gdbFileName;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            foreach (string templatePath in Directory.EnumerateFiles(templateDir, "*.lyrx"))
            {
                try
                {
                    JsonNode root = JsonNode.Parse(File.ReadAllText(templatePath));
                    if (Retarget(root, relativeConnection, summaryFeatureClassName) == 0)
                    {
                        continue; // unexpected shape — skip, don't emit a broken file
                    }

                    string outPath = Path.Combine(folder, Path.GetFileName(templatePath));
                    File.WriteAllText(outPath, root.ToJsonString(options), new UTF8Encoding(false));
                    created.Add(outPath);
                }
                catch (Exception)
                {
                    // Skip a problematic template; the others still get written.
                }
            }

            return created;
        }

        /// <summary>
        /// Recursively retargets every file-geodatabase data connection (any object
        /// carrying a "workspaceConnectionString") to the relative gdb path and the
        /// summary feature class. Returns the number of connections changed.
        /// </summary>
        private static int Retarget(JsonNode node, string connectionString, string dataset)
        {
            int count = 0;
            switch (node)
            {
                case JsonObject obj:
                    if (obj.ContainsKey("workspaceConnectionString"))
                    {
                        obj["workspaceConnectionString"] = connectionString;
                        obj["workspaceFactory"] = "FileGDB";
                        obj["dataset"] = dataset;
                        obj["datasetType"] = "esriDTFeatureClass";
                        count++;
                    }

                    foreach (JsonNode child in obj.Select(kv => kv.Value).ToList())
                    {
                        count += Retarget(child, connectionString, dataset);
                    }
                    break;

                case JsonArray arr:
                    foreach (JsonNode item in arr.ToList())
                    {
                        count += Retarget(item, connectionString, dataset);
                    }
                    break;
            }

            return count;
        }
    }
}
