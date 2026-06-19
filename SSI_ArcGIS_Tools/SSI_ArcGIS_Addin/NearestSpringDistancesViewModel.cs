using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// A springs feature layer offered in the Nearest Spring Distances layer list,
    /// enriched with its candidate SiteID / Inventory-Level field names and its
    /// selection / total feature counts (all read on the MCT before the dialog is
    /// shown, so the dialog never has to touch the MCT while open).
    /// </summary>
    internal sealed class SpringsDistanceLayerItem
    {
        internal SpringsDistanceLayerItem(FeatureLayer layer, long selectionCount, long totalCount,
            IReadOnlyList<string> siteIdFields, IReadOnlyList<string> invLevelFields)
        {
            Layer = layer;
            SelectionCount = selectionCount;
            TotalCount = totalCount;
            SiteIdFields = siteIdFields;
            InvLevelFields = invLevelFields;
        }

        internal FeatureLayer Layer { get; }
        internal long SelectionCount { get; }
        internal long TotalCount { get; }
        internal IReadOnlyList<string> SiteIdFields { get; }
        internal IReadOnlyList<string> InvLevelFields { get; }

        public string DisplayName => Layer.Name;
    }

    /// <summary>
    /// View model for the Nearest Spring Distances dialog. Picks a springs layer,
    /// the SiteID and Inventory-Level fields, the records to analyze, the optional
    /// output columns, and the output CSV path; validates the inputs that gate OK.
    /// The analysis itself is run by <see cref="SpringDistanceCalculator"/>.
    /// </summary>
    internal sealed class NearestSpringDistancesViewModel : PropertyChangedBase
    {
        private SpringsDistanceLayerItem _selectedLayer;
        private List<string> _siteIdFields = new();
        private List<string> _invLevelFields = new();
        private string _selectedSiteIdField;
        private string _selectedInvLevelField;

        private bool _includeNames;
        private bool _includeElevations;
        private bool _includeInfoSource;
        private bool _includeDate;
        private bool _includeInvLevel;

        private bool _analyzeAll = true;
        private bool _analyzeSelected;
        private string _allOptionLabel = "Analyze All Records";
        private string _selOptionLabel = "Analyze Only Selected Records";
        private bool _isSelectionEnabled;

        private string _outputPath = string.Empty;
        private string _validationMessage = string.Empty;
        private bool _canAccept;

        internal NearestSpringDistancesViewModel(
            IEnumerable<SpringsDistanceLayerItem> layers, string defaultOutputPath,
            IEnumerable<string> userNames, string preselectedUserName,
            bool includeNames, bool includeElevations, bool includeInfoSource,
            bool includeDate, bool includeInvLevel)
        {
            Layers = new List<SpringsDistanceLayerItem>(layers);
            UserNames = new List<string>(userNames ?? Enumerable.Empty<string>());
            SelectedUserName = preselectedUserName;

            _includeNames = includeNames;
            _includeElevations = includeElevations;
            _includeInfoSource = includeInfoSource;
            _includeDate = includeDate;
            _includeInvLevel = includeInvLevel;

            _outputPath = defaultOutputPath ?? string.Empty;

            BrowseCommand = new RelayCommand(BrowseForFile);

            // Pre-select the first layer that has a selection in the active map,
            // else the first layer; this triggers field-list population.
            SpringsDistanceLayerItem initial = Layers.FirstOrDefault(l => l.SelectionCount > 0)
                                               ?? Layers.FirstOrDefault();
            SelectedLayer = initial;

            Validate();
        }

        public IReadOnlyList<SpringsDistanceLayerItem> Layers { get; }

        public IReadOnlyList<string> UserNames { get; }

        /// <summary>Currently disabled — populated for a future "Include Login Data" feature.</summary>
        public string SelectedUserName { get; set; }

        public SpringsDistanceLayerItem SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                if (SetProperty(ref _selectedLayer, value))
                {
                    OnLayerChanged();
                    Validate();
                }
            }
        }

        public List<string> SiteIdFields
        {
            get => _siteIdFields;
            private set => SetProperty(ref _siteIdFields, value);
        }

        public List<string> InvLevelFields
        {
            get => _invLevelFields;
            private set => SetProperty(ref _invLevelFields, value);
        }

        public string SelectedSiteIdField
        {
            get => _selectedSiteIdField;
            set { if (SetProperty(ref _selectedSiteIdField, value)) { Validate(); } }
        }

        public string SelectedInvLevelField
        {
            get => _selectedInvLevelField;
            set { if (SetProperty(ref _selectedInvLevelField, value)) { Validate(); } }
        }

        public bool IncludeNames { get => _includeNames; set => SetProperty(ref _includeNames, value); }
        public bool IncludeElevations { get => _includeElevations; set => SetProperty(ref _includeElevations, value); }
        public bool IncludeInfoSource { get => _includeInfoSource; set => SetProperty(ref _includeInfoSource, value); }
        public bool IncludeDate { get => _includeDate; set => SetProperty(ref _includeDate, value); }
        public bool IncludeInvLevel { get => _includeInvLevel; set => SetProperty(ref _includeInvLevel, value); }

        public bool AnalyzeAll
        {
            get => _analyzeAll;
            set
            {
                if (SetProperty(ref _analyzeAll, value) && value)
                {
                    AnalyzeSelected = false;
                    Validate();
                }
            }
        }

        public bool AnalyzeSelected
        {
            get => _analyzeSelected;
            set
            {
                if (SetProperty(ref _analyzeSelected, value) && value)
                {
                    AnalyzeAll = false;
                    Validate();
                }
            }
        }

        public string AllOptionLabel { get => _allOptionLabel; private set => SetProperty(ref _allOptionLabel, value); }
        public string SelOptionLabel { get => _selOptionLabel; private set => SetProperty(ref _selOptionLabel, value); }
        public bool IsSelectionEnabled { get => _isSelectionEnabled; private set => SetProperty(ref _isSelectionEnabled, value); }

        public string OutputPath
        {
            get => _outputPath;
            set { if (SetProperty(ref _outputPath, value)) { Validate(); } }
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

        /// <summary>The output path, guaranteed to end in ".csv".</summary>
        internal string ResolvedCsvPath
        {
            get
            {
                string path = (OutputPath ?? string.Empty).Trim();
                return path.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase) ? path : path + ".csv";
            }
        }

        /// <summary>
        /// Repopulate the field lists for the newly selected layer, pre-select the
        /// "SiteID" and "InventoryLevel" fields, and refresh the All / Selected
        /// options from the layer's selection count.
        /// </summary>
        private void OnLayerChanged()
        {
            if (SelectedLayer == null)
            {
                SiteIdFields = new List<string>();
                InvLevelFields = new List<string>();
                SelectedSiteIdField = null;
                SelectedInvLevelField = null;
                return;
            }

            SiteIdFields = new List<string>(SelectedLayer.SiteIdFields);
            InvLevelFields = new List<string>(SelectedLayer.InvLevelFields);

            SelectedSiteIdField = SiteIdFields.FirstOrDefault(
                f => string.Equals(f, "SiteID", System.StringComparison.OrdinalIgnoreCase))
                ?? SiteIdFields.FirstOrDefault();
            SelectedInvLevelField = InvLevelFields.FirstOrDefault(
                f => string.Equals(f, "InventoryLevel", System.StringComparison.OrdinalIgnoreCase))
                ?? InvLevelFields.FirstOrDefault();

            long sel = SelectedLayer.SelectionCount;
            long total = SelectedLayer.TotalCount;
            AllOptionLabel = $"Analyze All Records (n = {total:N0})";
            IsSelectionEnabled = sel > 0;
            SelOptionLabel = sel > 0
                ? $"Analyze Only Selected Records (n = {sel:N0})"
                : "Analyze Only Selected Records (none selected)";

            // Default to "selected" when there is a selection, else "all".
            if (sel > 0) { AnalyzeSelected = true; }
            else { AnalyzeAll = true; }
        }

        private void BrowseForFile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Spring Distances Table",
                Filter = "Comma-Delimited Text (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                AddExtension = true,
                OverwritePrompt = true,
            };

            string current = (OutputPath ?? string.Empty).Trim();
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
                OutputPath = dialog.FileName;
            }
        }

        private void Validate()
        {
            if (SelectedLayer == null)
            {
                Fail("Select a springs feature layer.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedSiteIdField))
            {
                Fail("Select the SiteID field.");
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedInvLevelField))
            {
                Fail("Select the Inventory Level field.");
                return;
            }
            if (!AnalyzeAll && !AnalyzeSelected)
            {
                Fail("Choose whether to analyze all or only selected records.");
                return;
            }

            string path = (OutputPath ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(path))
            {
                Fail("Choose a name and location for the output CSV file.");
                return;
            }

            string dir = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                Fail("The output file's folder does not exist.");
                return;
            }
            if (File.Exists(ResolvedCsvPath))
            {
                Fail("A file with that name already exists — choose a new name.");
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
    }
}
