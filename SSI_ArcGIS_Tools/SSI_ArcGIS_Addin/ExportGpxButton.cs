using System;
using System.Collections.Generic;
using System.IO;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Ribbon button for the "Export GPX" tool. Picks a springs layer in a modal
    /// dialog, then exports its currently selected springs to a GPX file using
    /// <see cref="GpxExporter"/> — created and formatted identically to the GPX
    /// file produced by the Export Geodatabase tool.
    /// </summary>
    internal sealed class ExportGpxButton : Button
    {
        protected override void OnUpdate()
        {
            Enabled = MapView.Active != null;
        }

        protected override async void OnClick()
        {
            try
            {
                // 1) Gather candidate springs point layers (and selection counts) on the MCT.
                List<SpringsLayerItem> layers = await QueuedTask.Run(SpringsLayers.GatherPointSpringsLayers);
                if (layers.Count == 0)
                {
                    MessageBox.Show(
                        "The active map has no springs point layers to export." + Environment.NewLine +
                        "A layer must be a point feature class with a 'SiteID', 'SiteName' and 'InventoryLevel' field.",
                        "Export GPX");
                    return;
                }

                // 2) Show the modal dialog (UI thread). Default the output folder to
                // the last one used by the export tools, else the project folder.
                string defaultFolder = Module1.LastOutputFolder;
                if (string.IsNullOrWhiteSpace(defaultFolder) || !Directory.Exists(defaultFolder))
                {
                    defaultFolder = Project.Current?.HomeFolderPath;
                }

                var viewModel = new ExportGpxViewModel(layers, defaultFolder);
                var window = new ExportGpxWindow(viewModel)
                {
                    Owner = FrameworkApplication.Current.MainWindow,
                };

                if (window.ShowDialog() != true)
                {
                    return; // cancelled
                }

                FeatureLayer layer = viewModel.SelectedLayer.Layer;
                string gpxPath = viewModel.ResolvedGpxPath;

                // Determine how many springs will actually be written (the current
                // selection, or the whole layer when nothing is selected) and
                // confirm before exporting a large number of points.
                long exportCount = await QueuedTask.Run(() =>
                {
                    var selection = layer.GetSelection();
                    long selected = selection?.GetCount() ?? 0;
                    if (selected > 0)
                    {
                        return selected;
                    }

                    using var featureClass = layer.GetFeatureClass();
                    return featureClass.GetCount();
                });

                if (exportCount > 10000)
                {
                    System.Windows.MessageBoxResult answer = MessageBox.Show(
                        $"This will export {exportCount:N0} springs to GPX.{Environment.NewLine}{Environment.NewLine}Continue?",
                        "Export GPX",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Question);
                    if (answer != System.Windows.MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                // Remember the folder for next time.
                string folder = Path.GetDirectoryName(gpxPath);
                if (!string.IsNullOrEmpty(folder))
                {
                    Module1.LastOutputFolder = folder;
                }

                // 3) Run the GPX export (same engine + formatting as Export Geodatabase).
                string resultLine = await GpxExporter.ExportFromLayerAsync(layer, gpxPath);

                MessageBox.Show(
                    $"GPX export finished.{Environment.NewLine}{Environment.NewLine}{resultLine.TrimStart('-', ' ')}",
                    "Export GPX");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The GPX export failed:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Export GPX");
            }
        }
    }
}
