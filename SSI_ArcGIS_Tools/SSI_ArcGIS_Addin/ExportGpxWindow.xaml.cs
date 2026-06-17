using ArcGIS.Desktop.Framework.Controls;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Modal dialog for the Export GPX tool: pick a springs layer (its selection
    /// is exported) and the output GPX file path. The owning
    /// <see cref="ExportGpxViewModel"/> validates the inputs and gates OK.
    /// </summary>
    public partial class ExportGpxWindow : ProWindow
    {
        internal ExportGpxWindow(ExportGpxViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }

        internal ExportGpxViewModel ViewModel { get; }

        private void OnOkClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
