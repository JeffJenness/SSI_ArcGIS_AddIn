using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Creates the two layer files that symbolize the PRIMARY springs feature class
    /// (not the summary) next to an exported geodatabase, always written on a
    /// successful export. Both are built from a minimal CIM feature-layer document
    /// pointed at the springs feature class with a path RELATIVE to the layer file
    /// (".\&lt;gdb&gt;"), embedding the same renderers the Load Springs and Inventory
    /// Level ribbon tools apply:
    ///   * Symbolize_with_Single_Symbol.lyrx  - single-symbol renderer.
    ///   * Symbolize_by_Inventory_Level.lyrx   - unique values on InventoryLevel,
    ///     with the scale-dependent SiteName labels.
    /// </summary>
    internal static class SpringsLayerFileWriter
    {
        // Inventory-level labels show at 1:30,000 or larger (matches InventoryLevelButton).
        private const double InventoryLabelMinimumScale = 30000;

        /// <summary>
        /// Writes both springs layer files into the geodatabase's folder, retargeted
        /// to the springs feature class via a relative path. Returns the created
        /// paths. Runs on the MCT (file I/O only).
        /// </summary>
        internal static IReadOnlyList<string> Write(string outputGdbPath, string springsFeatureClassName)
        {
            string folder = Path.GetDirectoryName(outputGdbPath);
            string gdbFileName = Path.GetFileName(outputGdbPath);

            var created = new List<string>
            {
                WriteDocument(folder, "Symbolize_with_Single_Symbol.lyrx",
                    BuildLayerDocument("Springs", "Springs_Single", gdbFileName, springsFeatureClassName,
                        LoadSpringsFeatureClassButton.SpringsRendererJson, labelJson: null)),

                WriteDocument(folder, "Symbolize_by_Inventory_Level.lyrx",
                    BuildLayerDocument("Springs by Inventory Level", "Springs_Inventory", gdbFileName,
                        springsFeatureClassName, InventoryLevelButton.InventoryLevelRendererJson,
                        InventoryLevelButton.InventoryLevelLabelClassJson)),
            };

            return created;
        }

        private static string WriteDocument(string folder, string fileName, JsonObject document)
        {
            string path = Path.Combine(folder, fileName);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            File.WriteAllText(path, document.ToJsonString(options), new UTF8Encoding(false));
            return path;
        }

        /// <summary>
        /// Builds a minimal CIMLayerDocument for one feature layer pointed at the
        /// springs FC (relative gdb path), with the given renderer and optional
        /// scale-dependent labels.
        /// </summary>
        private static JsonObject BuildLayerDocument(
            string name, string uriName, string gdbFileName, string dataset, string rendererJson, string labelJson)
        {
            string uri = "CIMPATH=map/" + uriName + ".json";

            var dataConnection = new JsonObject
            {
                ["type"] = "CIMStandardDataConnection",
                ["workspaceConnectionString"] = "DATABASE=.\\" + gdbFileName,
                ["workspaceFactory"] = "FileGDB",
                ["dataset"] = dataset,
                ["datasetType"] = "esriDTFeatureClass",
            };

            var featureTable = new JsonObject
            {
                ["type"] = "CIMFeatureTable",
                ["displayField"] = "SiteName",
                ["editable"] = true,
                ["dataConnection"] = dataConnection,
                ["studyAreaSpatialRel"] = "esriSpatialRelUndefined",
                ["searchOrder"] = "esriSearchOrderSpatial",
            };

            var layer = new JsonObject
            {
                ["type"] = "CIMFeatureLayer",
                ["name"] = name,
                ["uRI"] = uri,
                ["useSourceMetadata"] = true,
                ["description"] = name,
                ["layerType"] = "Operational",
                ["showLegends"] = true,
                ["visibility"] = true,
                ["displayCacheType"] = "Permanent",
                ["maxDisplayCacheAge"] = 5,
                ["showPopups"] = true,
                ["serviceLayerID"] = -1,
                ["refreshRate"] = -1,
                ["refreshRateUnit"] = "esriTimeUnitsSeconds",
                ["blendingMode"] = "Alpha",
                ["allowDrapingOnIntegratedMesh"] = true,
                ["featureTable"] = featureTable,
                ["htmlPopupEnabled"] = true,
                ["selectable"] = true,
                ["featureCacheType"] = "Session",
                ["scaleSymbols"] = true,
                ["expanded"] = true,
                ["renderer"] = JsonNode.Parse(rendererJson),
            };

            if (labelJson != null)
            {
                JsonObject labelClass = JsonNode.Parse(labelJson).AsObject();
                labelClass["minimumScale"] = InventoryLabelMinimumScale;
                layer["labelClasses"] = new JsonArray(labelClass);
                layer["labelVisibility"] = true;
            }

            return new JsonObject
            {
                ["type"] = "CIMLayerDocument",
                ["version"] = "3.7.0",
                ["layers"] = new JsonArray(uri),
                ["layerDefinitions"] = new JsonArray(layer),
            };
        }
    }
}
