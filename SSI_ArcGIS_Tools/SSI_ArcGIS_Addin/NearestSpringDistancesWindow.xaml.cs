using ArcGIS.Desktop.Framework.Controls;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Modal dialog for the Nearest Spring Distances tool: pick a springs layer,
    /// the SiteID and Inventory-Level fields, the records to analyze, the optional
    /// output columns, and the output CSV file. The owning
    /// <see cref="NearestSpringDistancesViewModel"/> validates the inputs and gates OK.
    /// </summary>
    public partial class NearestSpringDistancesWindow : ProWindow
    {
        internal NearestSpringDistancesWindow(NearestSpringDistancesViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
        }

        internal NearestSpringDistancesViewModel ViewModel { get; }

        private void OnOkClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClick(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnManualClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBox.Show("Manual not written yet.", "Nearest Spring Distances");
        }
    }
}
