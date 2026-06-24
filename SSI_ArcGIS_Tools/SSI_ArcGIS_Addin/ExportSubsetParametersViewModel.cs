using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// A springs feature layer offered in the dialog's layer picker, with its
    /// pre-read selection count (read on the MCT before the dialog is shown).
    /// </summary>
    internal sealed class SpringsLayerItem
    {
        internal SpringsLayerItem(FeatureLayer layer, long selectionCount)
        {
            Layer = layer;
            SelectionCount = selectionCount;
        }

        internal FeatureLayer Layer { get; }
        internal long SelectionCount { get; }
        public string DisplayName =>
            SelectionCount > 0 ? $"{Layer.Name}  ({SelectionCount:N0} selected)" : Layer.Name;
    }

    /// <summary>
    /// View model for the Export Subset of Springs parameter dialog. Holds the
    /// bindable inputs and the validation that gates the OK button. All values
    /// are plain data; the button resolves the chosen layer to a
    /// <see cref="ExportSubsetParameters"/> on the MCT after the dialog closes.
    /// </summary>
    internal sealed class ExportSubsetParametersViewModel : PropertyChangedBase
    {
        private const string DefaultNameBase = "Springs_Subset";

        private SpringsLayerItem _selectedLayer;
        private bool _selectedFeaturesOnly = true;
        private string _outputFolder = string.Empty;
        private string _outputName = DefaultNameBase;

        // Set once the user types in the geodatabase-name box; while false, the name
        // auto-fills from the output folder's leaf name.
        private bool _userEditedName;
        private bool _trimStrings;
        private bool _createSummary;
        private bool _createGpx;
        private bool _writeMetadata;
        private string _excludeSitesWhereClause = string.Empty;
        private string _excludeSurveysWhereClause = string.Empty;
        private string _validationMessage = string.Empty;

        internal ExportSubsetParametersViewModel(
            IEnumerable<SpringsLayerItem> layers, string defaultFolder,
            bool selectedFeaturesOnly, bool trimStrings, bool createSummary, bool createGpx,
            bool writeMetadata)
        {
            Layers = new List<SpringsLayerItem>(layers);
            _selectedLayer = Layers.FirstOrDefault();
            BrowseFolderCommand = new RelayCommand(BrowseForFolder);
            _outputFolder = defaultFolder ?? string.Empty;
            _outputName = DeriveNameFromFolder(_outputFolder);
            _selectedFeaturesOnly = selectedFeaturesOnly;
            _trimStrings = trimStrings;
            _createSummary = createSummary;
            _createGpx = createGpx;
            _writeMetadata = writeMetadata;
            Validate();
        }

        public IReadOnlyList<SpringsLayerItem> Layers { get; }

        public SpringsLayerItem SelectedLayer
        {
            get => _selectedLayer;
            set { SetProperty(ref _selectedLayer, value); Validate(); }
        }

        public bool SelectedFeaturesOnly
        {
            get => _selectedFeaturesOnly;
            set { SetProperty(ref _selectedFeaturesOnly, value); Validate(); }
        }

        public string OutputFolder
        {
            get => _outputFolder;
            set
            {
                if (SetProperty(ref _outputFolder, value) && !_userEditedName)
                {
                    // Auto-fill the geodatabase name from the new folder's leaf name,
                    // unless the user has manually edited the name (then leave it).
                    _outputName = DeriveNameFromFolder(_outputFolder);
                    NotifyPropertyChanged(nameof(OutputName));
                }

                Validate();
            }
        }

        public string OutputName
        {
            get => _outputName;
            set
            {
                // Any edit here is the user typing (programmatic auto-fill sets the
                // backing field directly), so stop auto-filling from the folder.
                _userEditedName = true;
                SetProperty(ref _outputName, value);
                Validate();
            }
        }

        public bool TrimStrings
        {
            get => _trimStrings;
            set => SetProperty(ref _trimStrings, value);
        }

        public bool CreateSummary
        {
            get => _createSummary;
            set => SetProperty(ref _createSummary, value);
        }

        public bool CreateGpx
        {
            get => _createGpx;
            set => SetProperty(ref _createGpx, value);
        }

        public bool WriteMetadata
        {
            get => _writeMetadata;
            set => SetProperty(ref _writeMetadata, value);
        }

        public string ExcludeSitesWhereClause
        {
            get => _excludeSitesWhereClause;
            set => SetProperty(ref _excludeSitesWhereClause, value);
        }

        public string ExcludeSurveysWhereClause
        {
            get => _excludeSurveysWhereClause;
            set => SetProperty(ref _excludeSurveysWhereClause, value);
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            private set => SetProperty(ref _validationMessage, value);
        }

        private bool _canAccept;
        public bool CanAccept
        {
            get => _canAccept;
            private set => SetProperty(ref _canAccept, value);
        }

        public ICommand BrowseFolderCommand { get; }

        private void BrowseForFolder()
        {
            var dialog = new OpenItemDialog
            {
                Title = "Select the output folder for the new geodatabase",
                MultiSelect = false,
                BrowseFilter = BrowseProjectFilter.GetFilter(ItemFilters.Folders),
            };

            if (dialog.ShowDialog() == true)
            {
                Item item = dialog.Items?.FirstOrDefault();
                if (item != null)
                {
                    OutputFolder = item.Path;
                }
            }
        }

        /// <summary>
        /// Recomputes <see cref="CanAccept"/> and <see cref="ValidationMessage"/>.
        /// </summary>
        private void Validate()
        {
            if (SelectedLayer == null)
            {
                Fail("Select a springs feature layer.");
                return;
            }

            if (SelectedFeaturesOnly && SelectedLayer.SelectionCount == 0)
            {
                Fail("The chosen layer has no selected features. Select springs, or uncheck \"selected features only\".");
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputFolder) || !Directory.Exists(OutputFolder))
            {
                Fail("Choose an existing output folder.");
                return;
            }

            if (!IsValidGeodatabaseName(OutputName, out string nameError))
            {
                Fail(nameError);
                return;
            }

            ValidationMessage = string.Empty;
            CanAccept = true;
        }

        private void Fail(string message)
        {
            ValidationMessage = message;
            CanAccept = false;
        }

        /// <summary>
        /// Derives the auto-populated geodatabase name from the output folder's leaf
        /// name (e.g. "...\Coconino_Export" → "Coconino_Export"), sanitized to a
        /// valid geodatabase name. Falls back to the default base when the folder is
        /// unknown.
        /// </summary>
        private static string DeriveNameFromFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return DefaultNameBase;
            }

            string leaf = Path.GetFileName(
                folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            return string.IsNullOrWhiteSpace(leaf) ? DefaultNameBase : SanitizeGeodatabaseName(leaf);
        }

        /// <summary>
        /// Makes a folder name usable as a geodatabase name: replaces any character
        /// that is not a letter, digit or underscore with "_", and prefixes "_" when
        /// it would otherwise start with a digit.
        /// </summary>
        private static string SanitizeGeodatabaseName(string name)
        {
            var sb = new System.Text.StringBuilder(name.Length);
            foreach (char c in name)
            {
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
            }

            string cleaned = sb.ToString();
            if (cleaned.Length > 0 && char.IsDigit(cleaned[0]))
            {
                cleaned = "_" + cleaned;
            }

            return cleaned.Length > 0 ? cleaned : DefaultNameBase;
        }

        /// <summary>
        /// Validates the output geodatabase name: no spaces or other whitespace,
        /// only letters/digits/underscore, and not starting with a digit.
        /// </summary>
        internal static bool IsValidGeodatabaseName(string name, out string error)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Enter a name for the output geodatabase.";
                return false;
            }

            if (name.Any(char.IsWhiteSpace))
            {
                error = "The geodatabase name must not contain spaces.";
                return false;
            }

            if (!name.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                error = "The geodatabase name may contain only letters, digits, and underscores.";
                return false;
            }

            if (char.IsDigit(name[0]))
            {
                error = "The geodatabase name must not start with a digit.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
