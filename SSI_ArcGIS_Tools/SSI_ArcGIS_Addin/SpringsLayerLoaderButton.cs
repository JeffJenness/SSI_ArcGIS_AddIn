using System;
using System.IO;
using System.Linq;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Shared logic for buttons that load the Springs feature class into the
    /// active map. The feature class location is read from the Default_Parameters
    /// table in the add-in's file geodatabase; if it cannot be found, the user is
    /// prompted to browse for it and the chosen path is written back to the table.
    ///
    /// Derived buttons differ only in the symbology applied, supplied via
    /// <see cref="RendererJson"/>.
    /// </summary>
    internal abstract class SpringsLayerLoaderButton : Button
    {
        // --- Defaults table query definition ---------------------------------

        // NOTE: The original request referred to this table as "SSI_Defaults",
        // but the actual table inside SSI_Defaults.gdb is named
        // "Default_Parameters". Change this constant if the table is renamed.
        private const string DefaultsTableName = "Default_Parameters";
        private const string AnalysisTypeField = "Analysis_Type";
        private const string ValueField = "Value";
        private const string SpringsAnalysisType = "Springs Feature Class";

        // File geodatabase that holds the defaults table, deployed alongside the add-in.
        private const string DefaultsGeodatabaseName = "SSI_Defaults.gdb";

        /// <summary>
        /// The CIM renderer JSON applied to the loaded layer. Each derived button
        /// supplies its own hard-coded symbology here.
        /// </summary>
        protected abstract string RendererJson { get; }

        /// <summary>
        /// Optional CIM label-class JSON. When non-null/non-empty, labeling is
        /// turned on for the loaded layer using this label class. Default: no labels.
        /// </summary>
        protected virtual string LabelClassJson => null;

        /// <summary>
        /// Scale-dependent label visibility, as map-scale denominators (0 = no limit).
        /// MinimumScale is the most-zoomed-out scale at which labels draw, so
        /// MinimumScale = 30000 makes labels visible only at 1:30,000 or larger.
        /// </summary>
        protected virtual double LabelMinimumScale => 0;
        protected virtual double LabelMaximumScale => 0;

        /// <summary>
        /// Enable the command only when the active view is a map.
        /// (MapView.Active is null when a layout, table, or other view is active.)
        /// </summary>
        protected override void OnUpdate()
        {
            Enabled = MapView.Active != null;
        }

        protected override async void OnClick()
        {
            try
            {
                // 1. Read the configured path and check the feature class (on the MCT).
                (string configuredPath, bool exists) = await QueuedTask.Run(() =>
                {
                    string path = GetSpringsFeatureClassPath();
                    bool ok = !string.IsNullOrWhiteSpace(path) && FeatureClassExists(path);
                    return (path, ok);
                });

                string pathToLoad = configuredPath;

                // 2. If it is missing, tell the user and let them browse for it.
                if (!exists)
                {
                    string shown = string.IsNullOrWhiteSpace(configuredPath)
                        ? "(no path is configured)"
                        : configuredPath;

                    MessageBox.Show(
                        $"The Springs feature class could not be found at the configured location:" +
                        $"{Environment.NewLine}{Environment.NewLine}{shown}{Environment.NewLine}{Environment.NewLine}" +
                        $"Please browse to the correct feature class.",
                        "Springs Feature Class Not Found");

                    string picked = BrowseForFeatureClass();
                    if (string.IsNullOrWhiteSpace(picked))
                    {
                        return; // user cancelled the browse dialog
                    }

                    // 3. Persist the new path to the defaults table, then load it.
                    await QueuedTask.Run(() => UpdateSpringsFeatureClassPath(picked));
                    pathToLoad = picked;
                }

                // 4. Add the feature class to the active map with this tool's symbology.
                await QueuedTask.Run(() => AddFeatureClassToMap(pathToLoad));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load the Springs feature class:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Load Springs Feature Class");
            }
        }

        /// <summary>
        /// Adds the feature class at the given catalog path to the active map and
        /// applies this tool's renderer and (optional) labels. Runs on the MCT.
        /// </summary>
        private void AddFeatureClassToMap(string featureClassPath)
        {
            Map map = MapView.Active?.Map
                ?? throw new InvalidOperationException("There is no active map view.");

            (string gdbPath, string featureClassName) = SplitFeatureClassPath(featureClassPath);

            var connectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath));
            using (var geodatabase = new Geodatabase(connectionPath))
            using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(featureClassName))
            {
                var layerParams = new FeatureLayerCreationParams(featureClass);
                FeatureLayer featureLayer = LayerFactory.Instance.CreateLayer<FeatureLayer>(layerParams, map);

                ApplyRenderer(featureLayer);
                ApplyLabels(featureLayer);
            }
        }

        /// <summary>
        /// Applies this tool's hard-coded CIM renderer (from JSON) to the layer.
        /// Runs on the MCT.
        /// </summary>
        private void ApplyRenderer(FeatureLayer featureLayer)
        {
            if (featureLayer == null || string.IsNullOrWhiteSpace(RendererJson))
            {
                return;
            }

            var renderer = (CIMRenderer)CIMRenderer.FromJson(RendererJson);
            featureLayer.SetRenderer(renderer);
        }

        /// <summary>
        /// Turns on labeling with this tool's label class (if any) and applies the
        /// scale-dependent visibility range. Runs on the MCT.
        /// </summary>
        private void ApplyLabels(FeatureLayer featureLayer)
        {
            if (featureLayer == null || string.IsNullOrWhiteSpace(LabelClassJson))
            {
                return;
            }

            var labelClass = (CIMLabelClass)CIMLabelClass.FromJson(LabelClassJson);
            labelClass.MinimumScale = LabelMinimumScale;
            labelClass.MaximumScale = LabelMaximumScale;

            if (featureLayer.GetDefinition() is CIMFeatureLayer definition)
            {
                definition.LabelClasses = new[] { labelClass };
                definition.LabelVisibility = true;
                featureLayer.SetDefinition(definition);
            }
        }

        /// <summary>
        /// Returns true if a feature class can be opened at the given catalog
        /// path. Any failure (missing geodatabase, missing feature class, bad
        /// path) is treated as "does not exist". Runs on the MCT.
        /// </summary>
        private static bool FeatureClassExists(string featureClassPath)
        {
            try
            {
                (string gdbPath, string featureClassName) = SplitFeatureClassPath(featureClassPath);
                if (!Directory.Exists(gdbPath))
                {
                    return false;
                }

                var connectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath));
                using (var geodatabase = new Geodatabase(connectionPath))
                using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(featureClassName))
                {
                    return featureClass != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Opens a browse dialog filtered to feature classes and returns the
        /// catalog path of the selected feature class (null if cancelled).
        /// Must run on the UI thread (not the MCT).
        /// </summary>
        private static string BrowseForFeatureClass()
        {
            var dialog = new OpenItemDialog
            {
                Title = "Select the Springs Feature Class",
                MultiSelect = false,
                BrowseFilter = BrowseProjectFilter.GetFilter(ItemFilters.FeatureClasses_All)
            };

            if (dialog.ShowDialog() == true)
            {
                Item item = dialog.Items?.FirstOrDefault();
                return item?.Path;
            }

            return null;
        }

        /// <summary>
        /// Queries the defaults table for the "Springs Feature Class" row and
        /// returns the catalog path stored in the "Value" field. Runs on the MCT.
        /// </summary>
        private static string GetSpringsFeatureClassPath()
        {
            string gdbPath = GetDefaultsGeodatabasePath();
            var connectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath));

            using (var geodatabase = new Geodatabase(connectionPath))
            using (Table table = geodatabase.OpenDataset<Table>(DefaultsTableName))
            {
                var queryFilter = new QueryFilter
                {
                    WhereClause = $"{AnalysisTypeField} = '{SpringsAnalysisType}'",
                    SubFields = ValueField
                };

                using (RowCursor cursor = table.Search(queryFilter, false))
                {
                    if (cursor.MoveNext())
                    {
                        using (Row row = cursor.Current)
                        {
                            return Convert.ToString(row[ValueField]);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Writes the given feature class path to the "Value" field of the
        /// "Springs Feature Class" row in the defaults table. Runs on the MCT.
        ///
        /// The defaults geodatabase is opened directly through the Geodatabase
        /// API and is not part of any map, so edits are written straight to disk
        /// via Row.Store(). (An EditOperation would route through the Pro edit
        /// session and only reach disk on "Save Edits", which is not what we
        /// want for a configuration value.)
        /// </summary>
        private static void UpdateSpringsFeatureClassPath(string newPath)
        {
            string gdbPath = GetDefaultsGeodatabasePath();
            var connectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath));

            using (var geodatabase = new Geodatabase(connectionPath))
            using (Table table = geodatabase.OpenDataset<Table>(DefaultsTableName))
            {
                var queryFilter = new QueryFilter
                {
                    WhereClause = $"{AnalysisTypeField} = '{SpringsAnalysisType}'"
                };

                int updated = 0;
                using (RowCursor cursor = table.Search(queryFilter, false))
                {
                    while (cursor.MoveNext())
                    {
                        using (Row row = cursor.Current)
                        {
                            row[ValueField] = newPath;
                            row.Store();
                            updated++;
                        }
                    }
                }

                if (updated == 0)
                {
                    throw new InvalidOperationException(
                        $"No '{SpringsAnalysisType}' row exists in table '{DefaultsTableName}' to update.");
                }
            }
        }

        /// <summary>
        /// Splits a feature class catalog path such as
        /// "...\Springs_GDB.gdb\SSI_Springs\World_Springs" into the geodatabase
        /// path ("...\Springs_GDB.gdb") and the feature class name
        /// ("World_Springs"). The intervening feature dataset (SSI_Springs) is
        /// not needed: feature class names are unique within a geodatabase.
        /// </summary>
        private static (string gdbPath, string featureClassName) SplitFeatureClassPath(string fullPath)
        {
            const string marker = ".gdb";
            int idx = fullPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                throw new InvalidOperationException(
                    $"The path does not point to a file geodatabase:{Environment.NewLine}{fullPath}");
            }

            string gdbPath = fullPath.Substring(0, idx + marker.Length);
            string featureClassName = Path.GetFileName(fullPath.TrimEnd('\\', '/'));
            return (gdbPath, featureClassName);
        }

        /// <summary>
        /// Location of the defaults file geodatabase, resolved relative to the
        /// add-in's install location (the folder the executing assembly runs from).
        /// The geodatabase must be deployed next to the add-in assembly.
        /// </summary>
        private static string GetDefaultsGeodatabasePath()
        {
            string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string installDir = Path.GetDirectoryName(assemblyPath);
            if (string.IsNullOrEmpty(installDir))
            {
                throw new InvalidOperationException(
                    "Unable to determine the add-in install location.");
            }

            return Path.Combine(installDir, DefaultsGeodatabaseName);
        }
    }
}
