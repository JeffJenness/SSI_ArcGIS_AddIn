using ArcGIS.Desktop.Framework.Controls;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Modal dialog that collects the parameters for the Export Subset of Springs
    /// tool. The owning <see cref="ExportSubsetParametersViewModel"/> validates the
    /// inputs and gates the OK button.
    /// </summary>
    public partial class ExportSubsetParametersWindow : ProWindow
    {
        internal ExportSubsetParametersWindow(ExportSubsetParametersViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }

        internal ExportSubsetParametersViewModel ViewModel { get; }

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
