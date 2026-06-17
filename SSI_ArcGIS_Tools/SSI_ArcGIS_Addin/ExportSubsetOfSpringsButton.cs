using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Ribbon button for the "Export Subset of Springs" tool. Collects parameters
    /// in a modal dialog, then runs <see cref="SpringsSubsetExporter"/> on the MCT
    /// under a cancellable progress dialog and reports the outcome. This is the
    /// only class that crosses the UI thread / MCT boundary.
    /// </summary>
    internal sealed class ExportSubsetOfSpringsButton : Button
    {
        protected override void OnUpdate()
        {
            Enabled = MapView.Active != null;
        }

        protected override async void OnClick()
        {
            try
            {
                // 1) Gather candidate point feature layers (and their selection counts) on the MCT.
                List<SpringsLayerItem> layers = await QueuedTask.Run(GatherSpringsLayers);
                if (layers.Count == 0)
                {
                    MessageBox.Show(
                        "The active map has no springs point layers to export." + Environment.NewLine +
                        "A layer must be a point feature class with both a 'SiteID' and a 'SiteName' field.",
                        "Export Subset of Springs");
                    return;
                }

                // 2) Show the modal parameter dialog (UI thread). Default the output
                // folder to the last one used by this tool, else the project folder.
                string defaultFolder = Module1.LastOutputFolder;
                if (string.IsNullOrWhiteSpace(defaultFolder) || !Directory.Exists(defaultFolder))
                {
                    defaultFolder = Project.Current?.HomeFolderPath;
                }

                var viewModel = new ExportSubsetParametersViewModel(
                    layers, defaultFolder,
                    Module1.LastSelectedFeaturesOnly, Module1.LastTrimStrings,
                    Module1.LastCreateSummary, Module1.LastCreateGpx);
                var window = new ExportSubsetParametersWindow(viewModel)
                {
                    Owner = FrameworkApplication.Current.MainWindow,
                };

                if (window.ShowDialog() != true)
                {
                    return; // cancelled
                }

                // Remember the dialog choices for next time.
                Module1.LastSelectedFeaturesOnly = viewModel.SelectedFeaturesOnly;
                Module1.LastTrimStrings = viewModel.TrimStrings;
                Module1.LastCreateSummary = viewModel.CreateSummary;
                Module1.LastCreateGpx = viewModel.CreateGpx;

                // 3) Resolve the chosen layer to thread-agnostic parameters on the MCT.
                ExportSubsetParameters parameters = await QueuedTask.Run(() =>
                    ResolveParameters(viewModel));

                // 4) Run the export under a cancellable progress dialog (MCT).
                var progressDialog = new ProgressDialog(
                    "Exporting subset of springs...", "Cancel", 100, false);
                var progressSource = new CancelableProgressorSource(progressDialog);

                var exporter = new SpringsSubsetExporter(parameters);
                string report = null;
                bool cancelled = false;

                try
                {
                    report = await QueuedTask.Run(
                        () => exporter.Run(progressSource.Progressor),
                        progressSource.Progressor);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                }

                // 5) Report the outcome.
                if (cancelled)
                {
                    MessageBox.Show(
                        $"Export cancelled.{Environment.NewLine}{Environment.NewLine}" +
                        $"A partial geodatabase may remain at:{Environment.NewLine}{exporter.OutputGeodatabasePath}",
                        "Export Subset of Springs");
                    return;
                }

                // Remember the folder for next time (session + persisted with project).
                Module1.LastOutputFolder = parameters.OutputFolder;

                // 6) Optional GPX export via the native FeaturesToGPX tool.
                if (parameters.CreateGpx)
                {
                    string gpxLine = await GpxExporter.ExportAsync(
                        exporter.OutputGeodatabasePath, parameters.OutputName);
                    report += Environment.NewLine + gpxLine;
                }

                string reportPath = WriteReport(exporter.OutputGeodatabasePath, report);
                MessageBox.Show(
                    $"Export complete.{Environment.NewLine}{Environment.NewLine}" +
                    $"Geodatabase:{Environment.NewLine}{exporter.OutputGeodatabasePath}{Environment.NewLine}{Environment.NewLine}" +
                    $"Full report saved to:{Environment.NewLine}{reportPath}",
                    "Export Subset of Springs");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The subset export failed:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Export Subset of Springs");
            }
        }

        /// <summary>
        /// Returns the active map's point feature layers that can actually be
        /// exported as springs — i.e. those whose feature class has both a
        /// "SiteID" and a "SiteName" field — each with its current selection
        /// count. Runs on the MCT.
        /// </summary>
        private static List<SpringsLayerItem> GatherSpringsLayers()
        {
            Map map = MapView.Active?.Map;
            if (map == null)
            {
                return new List<SpringsLayerItem>();
            }

            var items = new List<SpringsLayerItem>();
            foreach (FeatureLayer layer in map.GetLayersAsFlattenedList().OfType<FeatureLayer>())
            {
                if (layer.ShapeType != esriGeometryType.esriGeometryPoint)
                {
                    continue;
                }

                if (!HasSpringsKeyFields(layer))
                {
                    continue;
                }

                long count = layer.GetSelection()?.GetCount() ?? 0;
                items.Add(new SpringsLayerItem(layer, count));
            }

            return items;
        }

        /// <summary>
        /// True if the layer's feature class has both the "SiteID" and "SiteName"
        /// fields (a minimal check that it is a springs feature class). Any failure
        /// to open the source is treated as "not a springs layer". Runs on the MCT.
        /// </summary>
        private static bool HasSpringsKeyFields(FeatureLayer layer)
        {
            try
            {
                using FeatureClass featureClass = layer.GetFeatureClass();
                if (featureClass == null)
                {
                    return false;
                }

                FeatureClassDefinition definition = featureClass.GetDefinition();
                return definition.FindField("SiteID") >= 0 && definition.FindField("SiteName") >= 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Builds the export parameters from the chosen layer: source geodatabase
        /// path, springs feature class name, and selected ObjectIDs. Runs on the MCT.
        /// </summary>
        private static ExportSubsetParameters ResolveParameters(ExportSubsetParametersViewModel vm)
        {
            FeatureLayer layer = vm.SelectedLayer.Layer;

            using FeatureClass featureClass = layer.GetFeatureClass();
            using Datastore datastore = featureClass.GetDatastore();

            if (datastore is not Geodatabase geodatabase ||
                geodatabase.GetConnector() is not FileGeodatabaseConnectionPath connection)
            {
                throw new InvalidOperationException(
                    "The springs layer must come from a file geodatabase.");
            }

            IReadOnlyList<long> selectedOids = null;
            if (vm.SelectedFeaturesOnly)
            {
                selectedOids = layer.GetSelection().GetObjectIDs();
            }

            return new ExportSubsetParameters
            {
                SourceGeodatabasePath = connection.Path.LocalPath,
                SpringsFeatureClassName = featureClass.GetName(),
                SelectedObjectIds = selectedOids,
                ExcludeSitesWhereClause = string.IsNullOrWhiteSpace(vm.ExcludeSitesWhereClause)
                    ? null : vm.ExcludeSitesWhereClause.Trim(),
                ExcludeSurveysWhereClause = string.IsNullOrWhiteSpace(vm.ExcludeSurveysWhereClause)
                    ? null : vm.ExcludeSurveysWhereClause.Trim(),
                OutputFolder = vm.OutputFolder,
                OutputName = vm.OutputName.Trim(),
                TrimStrings = vm.TrimStrings,
                CreateSummary = vm.CreateSummary,
                CreateGpx = vm.CreateGpx,
            };
        }

        private static string WriteReport(string outputGeodatabasePath, string report)
        {
            string folder = Path.GetDirectoryName(outputGeodatabasePath);
            string baseName = Path.GetFileNameWithoutExtension(outputGeodatabasePath);
            string reportPath = Path.Combine(folder, baseName + "_report.txt");
            File.WriteAllText(reportPath, report);
            return reportPath;
        }
    }
}
