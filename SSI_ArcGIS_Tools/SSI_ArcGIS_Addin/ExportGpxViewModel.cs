using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// View model for the Export GPX dialog. Picks a springs feature layer (whose
    /// current selection is exported) and the output GPX file path, and validates
    /// the inputs that gate the OK button. The GPX itself is created and formatted
    /// by <see cref="GpxExporter"/>, identical to the Export Geodatabase tool.
    /// </summary>
    internal sealed class ExportGpxViewModel : PropertyChangedBase
    {
        private SpringsLayerItem _selectedLayer;
        private string _gpxPath = string.Empty;
        private string _validationMessage = string.Empty;
        private bool _canAccept;

        internal ExportGpxViewModel(IEnumerable<SpringsLayerItem> layers, string defaultFolder)
        {
            Layers = new List<SpringsLayerItem>(layers);

            // Default to the first layer that has a selection, else the first layer.
            foreach (SpringsLayerItem item in Layers)
            {
                _selectedLayer = item;
                if (item.SelectionCount > 0)
                {
                    break;
                }
            }

            if (!string.IsNullOrWhiteSpace(defaultFolder) && Directory.Exists(defaultFolder))
            {
                _gpxPath = Path.Combine(defaultFolder, "Springs.gpx");
            }

            BrowseCommand = new RelayCommand(BrowseForFile);
            Validate();
        }

        public IReadOnlyList<SpringsLayerItem> Layers { get; }

        public SpringsLayerItem SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                if (SetProperty(ref _selectedLayer, value))
                {
                    Validate();
                }
            }
        }

        public string GpxPath
        {
            get => _gpxPath;
            set
            {
                if (SetProperty(ref _gpxPath, value))
                {
                    Validate();
                }
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            private set => SetProperty(ref _validationMessage, value);
        }

        public bool CanAccept
        {
            get => _canAccept;
            private set => SetProperty(ref _canAccept, value);
        }

        public ICommand BrowseCommand { get; }

        /// <summary>The output path, guaranteed to end in ".gpx".</summary>
        internal string ResolvedGpxPath
        {
            get
            {
                string path = (GpxPath ?? string.Empty).Trim();
                return path.EndsWith(".gpx", System.StringComparison.OrdinalIgnoreCase)
                    ? path
                    : path + ".gpx";
            }
        }

        private void BrowseForFile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save the springs GPX file",
                Filter = "GPX files (*.gpx)|*.gpx|All files (*.*)|*.*",
                DefaultExt = ".gpx",
                AddExtension = true,
                OverwritePrompt = true,
            };

            string current = (GpxPath ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(current))
            {
                string dir = Path.GetDirectoryName(current);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    dialog.InitialDirectory = dir;
                }

                dialog.FileName = Path.GetFileName(current);
            }

            if (dialog.ShowDialog() == true)
            {
                GpxPath = dialog.FileName;
            }
        }

        private void Validate()
        {
            if (SelectedLayer == null)
            {
                Fail("Select a springs feature layer.");
                return;
            }

            string path = (GpxPath ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(path))
            {
                Fail("Choose a name and location for the GPX file.");
                return;
            }

            string dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                Fail("The GPX file's folder does not exist.");
                return;
            }

            // A layer with no selection exports all of its springs; note it but allow it.
            ValidationMessage = SelectedLayer.SelectionCount == 0
                ? "No springs selected — all springs in the layer will be exported."
                : string.Empty;
            CanAccept = true;
        }

        private void Fail(string message)
        {
            ValidationMessage = message;
            CanAccept = false;
        }
    }
}
