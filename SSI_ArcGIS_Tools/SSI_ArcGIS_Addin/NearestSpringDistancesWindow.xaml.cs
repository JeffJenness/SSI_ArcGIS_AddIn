using System;
using System.Windows.Controls;
using System.Windows.Threading;
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

        private void OnWindowLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ScrollInvLevelSelectionToTop();
        }

        private void OnLayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Selecting a layer repopulates the Inventory Level list and re-selects
            // its default field; scroll that selection to the top of the list.
            ScrollInvLevelSelectionToTop();
        }

        /// <summary>
        /// Scrolls the Inventory Level list so the selected item (the auto-selected
        /// "InventoryLevel" field) sits at the top. Deferred to Background priority
        /// so the list's item containers are realized after the binding update and
        /// layout pass before we scroll.
        /// </summary>
        private void ScrollInvLevelSelectionToTop()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                object selected = lbxInvLevField.SelectedItem;
                if (selected == null || lbxInvLevField.Items.Count == 0)
                {
                    return;
                }

                lbxInvLevField.UpdateLayout();
                // Scroll to the last item first so the subsequent ScrollIntoView of
                // the selected item aligns it to the TOP of the viewport rather than
                // the bottom.
                lbxInvLevField.ScrollIntoView(lbxInvLevField.Items[lbxInvLevField.Items.Count - 1]);
                lbxInvLevField.ScrollIntoView(selected);
            }), DispatcherPriority.Background);
        }
    }
}
