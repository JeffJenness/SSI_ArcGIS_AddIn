using System;
using System.IO;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Loads the Springs feature class (World_Springs) into the active map.
    /// The feature class location is not hard-coded: it is read from a defaults
    /// table inside the add-in's file geodatabase, so the source can be changed
    /// without rebuilding the add-in.
    /// </summary>
    internal class LoadSpringsFeatureClassButton : Button
    {
        // --- Defaults table query definition ---------------------------------

        // NOTE: The original request referred to this table as "SSI_Defaults",
        // but the actual table inside SSI_Defaults.gdb is named
        // "Default_Parameters". Change this constant if the table is renamed.
        private const string DefaultsTableName = "Default_Parameters";
        private const string AnalysisTypeField = "Analysis_Type";
        private const string ValueField = "Value";
        private const string SpringsAnalysisType = "Springs Feature Class";

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
                await LoadSpringsLayerAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load the Springs feature class:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Load Springs Feature Class");
            }
        }

        private static Task LoadSpringsLayerAsync()
        {
            return QueuedTask.Run(() =>
            {
                Map map = MapView.Active?.Map
                    ?? throw new InvalidOperationException("There is no active map view.");

                // 1. Read the feature class path from the defaults geodatabase.
                string featureClassPath = GetSpringsFeatureClassPath();
                if (string.IsNullOrWhiteSpace(featureClassPath))
                {
                    throw new InvalidOperationException(
                        $"No '{SpringsAnalysisType}' row was found in table '{DefaultsTableName}'.");
                }

                // 2. Split the catalog path into the geodatabase and feature class.
                (string gdbPath, string featureClassName) = SplitFeatureClassPath(featureClassPath);

                // 3. Open the feature class and add it to the active map.
                var connectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath));
                using (var geodatabase = new Geodatabase(connectionPath))
                using (FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(featureClassName))
                {
                    var layerParams = new FeatureLayerCreationParams(featureClass);
                    LayerFactory.Instance.CreateLayer<FeatureLayer>(layerParams, map);
                }
            });
        }

        /// <summary>
        /// Queries the defaults table for the "Springs Feature Class" row and
        /// returns the catalog path stored in the "Value" field.
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
                    $"The Value path does not point to a file geodatabase:{Environment.NewLine}{fullPath}");
            }

            string gdbPath = fullPath.Substring(0, idx + marker.Length);
            string featureClassName = Path.GetFileName(fullPath.TrimEnd('\\', '/'));
            return (gdbPath, featureClassName);
        }

        // File geodatabase that holds the defaults table, deployed alongside the add-in.
        private const string DefaultsGeodatabaseName = "SSI_Defaults.gdb";

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
