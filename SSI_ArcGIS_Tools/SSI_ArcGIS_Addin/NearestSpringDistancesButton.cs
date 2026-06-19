using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    /// Ribbon button for the "Nearest Spring Distances" tool. Collects parameters
    /// in a modal dialog, then runs <see cref="SpringDistanceCalculator"/> on the
    /// MCT under a cancellable progress dialog and reports the outcome. This is the
    /// only class that crosses the UI thread / MCT boundary for this tool.
    /// </summary>
    internal sealed class NearestSpringDistancesButton : Button
    {
        private const string UsersTableName = "SSI_Users";
        private const string DefaultsGeodatabaseName = "SSI_Defaults.gdb";

        protected override void OnUpdate()
        {
            Enabled = MapView.Active != null;
        }

        protected override async void OnClick()
        {
            try
            {
                // 1) Gather candidate springs point layers (with field lists + counts)
                //    and the registered user names, all on the MCT.
                (List<SpringsDistanceLayerItem> layers, List<string> userNames, string preselectedUser) =
                    await QueuedTask.Run(() =>
                    {
                        List<SpringsDistanceLayerItem> gathered = SpringsLayers.GatherDistanceLayers();
                        (List<string> names, string preselect) = ReadRegisteredUsers();
                        return (gathered, names, preselect);
                    });

                if (layers.Count == 0)
                {
                    MessageBox.Show(
                        "The active map has no springs point layers to analyze." + Environment.NewLine +
                        "A layer must be a point feature class with a 'SiteID', 'SiteName' and 'InventoryLevel' field.",
                        "Nearest Spring Distances");
                    return;
                }

                // 2) Compute the default output path: last-used folder (else project
                //    folder) + a dated, uniquified CSV name.
                string defaultFolder = Module1.LastDistanceFolder;
                if (string.IsNullOrWhiteSpace(defaultFolder) || !Directory.Exists(defaultFolder))
                {
                    defaultFolder = Project.Current?.HomeFolderPath;
                }
                string defaultPath = ComputeDefaultCsvPath(defaultFolder);

                // 3) Show the modal parameter dialog (UI thread).
                var viewModel = new NearestSpringDistancesViewModel(
                    layers, defaultPath, userNames, preselectedUser,
                    Module1.LastDistIncludeNames, Module1.LastDistIncludeElevations,
                    Module1.LastDistIncludeInfoSource, Module1.LastDistIncludeDate,
                    Module1.LastDistIncludeInvLevel);
                var window = new NearestSpringDistancesWindow(viewModel)
                {
                    Owner = FrameworkApplication.Current.MainWindow,
                };

                if (window.ShowDialog() != true)
                {
                    return; // cancelled
                }

                // Remember the dialog choices for next time.
                Module1.LastDistIncludeNames = viewModel.IncludeNames;
                Module1.LastDistIncludeElevations = viewModel.IncludeElevations;
                Module1.LastDistIncludeInfoSource = viewModel.IncludeInfoSource;
                Module1.LastDistIncludeDate = viewModel.IncludeDate;
                Module1.LastDistIncludeInvLevel = viewModel.IncludeInvLevel;

                string csvPath = viewModel.ResolvedCsvPath;
                var parameters = new SpringDistanceParameters
                {
                    Layer = viewModel.SelectedLayer.Layer,
                    SiteIdField = viewModel.SelectedSiteIdField,
                    InvLevelField = viewModel.SelectedInvLevelField,
                    AnalyzeAll = viewModel.AnalyzeAll,
                    OutputCsvPath = csvPath,
                    IncludeNames = viewModel.IncludeNames,
                    IncludeElevations = viewModel.IncludeElevations,
                    IncludeDate = viewModel.IncludeDate,
                    IncludeInfoSource = viewModel.IncludeInfoSource,
                    IncludeInvLevel = viewModel.IncludeInvLevel,
                };

                // 4) Run the analysis under a cancellable progress dialog (MCT).
                var progressDialog = new ProgressDialog(
                    "Calculating nearest spring distances...", "Cancel", 100, false);
                var progressSource = new CancelableProgressorSource(progressDialog);

                var calculator = new SpringDistanceCalculator(parameters);
                string report = null;
                bool cancelled = false;

                try
                {
                    report = await QueuedTask.Run(
                        () => calculator.Run(progressSource.Progressor),
                        progressSource.Progressor);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                }

                if (cancelled)
                {
                    MessageBox.Show(
                        $"Analysis cancelled.{Environment.NewLine}{Environment.NewLine}" +
                        $"A partial CSV may remain at:{Environment.NewLine}{csvPath}",
                        "Nearest Spring Distances");
                    return;
                }

                // Remember the folder for next time (session + persisted with project).
                string folder = Path.GetDirectoryName(csvPath);
                if (!string.IsNullOrEmpty(folder))
                {
                    Module1.LastDistanceFolder = folder;
                }

                MessageBox.Show(
                    $"{report}{Environment.NewLine}" +
                    $"{calculator.AnalyzedCount:N0} springs analyzed.",
                    "Nearest Spring Distances");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The nearest-spring-distance analysis failed:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Nearest Spring Distances");
            }
        }

        /// <summary>
        /// Reads the SSI_Users table (in the deployed defaults geodatabase) and
        /// returns the user "Name" values plus the first name whose "Connect_String"
        /// is a path that exists on this computer (so the disabled user combo can be
        /// pre-selected for a future "Include Login Data" feature). Any failure
        /// returns an empty list. Runs on the MCT.
        /// </summary>
        private static (List<string> names, string preselected) ReadRegisteredUsers()
        {
            var names = new List<string>();
            string preselected = null;

            try
            {
                string gdbPath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                    DefaultsGeodatabaseName);
                if (!Directory.Exists(gdbPath))
                {
                    return (names, null);
                }

                var connectionPath = new FileGeodatabaseConnectionPath(new Uri(gdbPath));
                using var geodatabase = new Geodatabase(connectionPath);
                using Table table = geodatabase.OpenDataset<Table>(UsersTableName);
                using RowCursor cursor = table.Search(null, false);
                while (cursor.MoveNext())
                {
                    using Row row = cursor.Current;
                    string name = Convert.ToString(row["Name"]);
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }
                    names.Add(name);

                    if (preselected == null)
                    {
                        string connect = Convert.ToString(row["Connect_String"]);
                        if (!string.IsNullOrWhiteSpace(connect) &&
                            (File.Exists(connect) || Directory.Exists(connect)))
                        {
                            preselected = name;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // The defaults gdb or SSI_Users table may be absent; the combo is
                // disabled anyway, so an empty list is fine.
            }

            return (names, preselected);
        }

        /// <summary>
        /// Returns a dated CSV path in the given folder
        /// ("Spring_Distances_&lt;Mon&gt;_&lt;dd&gt;_&lt;yyyy&gt;.csv"), auto-incrementing a
        /// numeric suffix until the name is free.
        /// </summary>
        private static string ComputeDefaultCsvPath(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                return string.Empty;
            }

            DateTime now = DateTime.Now;
            string baseName = string.Format(CultureInfo.InvariantCulture,
                "Spring_Distances_{0:MMM}_{0:dd}_{0:yyyy}", now);

            string candidate = Path.Combine(folder, baseName + ".csv");
            int suffix = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(folder, $"{baseName}_{suffix}.csv");
                suffix++;
            }

            return candidate;
        }
    }
}
