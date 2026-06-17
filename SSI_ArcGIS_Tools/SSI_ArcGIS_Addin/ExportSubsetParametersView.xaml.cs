using System;
using System.Windows;
using System.Windows.Controls;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// The data-entry surface for the Export Subset of Springs dialog. Kept as a
    /// plain WPF UserControl (no ArcGIS Pro base type) so it renders in the Visual
    /// Studio designer; it is hosted inside <see cref="ExportSubsetParametersWindow"/>,
    /// which provides the ProWindow chrome, OK/Cancel bar and resize grip. Its
    /// DataContext is inherited from the hosting window
    /// (<see cref="ExportSubsetParametersViewModel"/>).
    /// </summary>
    public partial class ExportSubsetParametersView : UserControl
    {
        public ExportSubsetParametersView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Build the "exclude surveys for these springs" clause with the Pro Query
        /// Builder against the selected springs layer.
        /// </summary>
        private void OnBuildSitesQuery(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ExportSubsetParametersViewModel vm || vm.SelectedLayer?.Layer == null)
            {
                return;
            }

            var window = new QueryBuilderWindow(
                vm.SelectedLayer.Layer, vm.ExcludeSitesWhereClause,
                "Surveys for any spring matching this query will be excluded from the export.")
            {
                Owner = Window.GetWindow(this),
            };

            if (window.ShowDialog() == true)
            {
                vm.ExcludeSitesWhereClause = window.Expression;
            }
        }

        /// <summary>
        /// Build the "exclude these surveys" clause with the Pro Query Builder
        /// against the source tbl_Surveys (added to the map as a temporary
        /// standalone table just for the duration of the builder).
        /// </summary>
        private async void OnBuildSurveysQuery(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ExportSubsetParametersViewModel vm || vm.SelectedLayer?.Layer == null)
            {
                return;
            }

            FeatureLayer layer = vm.SelectedLayer.Layer;
            Window owner = Window.GetWindow(this);

            (StandaloneTable table, Map map) = await QueuedTask.Run(() => CreateSurveysTable(layer));
            if (table == null)
            {
                MessageBox.Show(
                    "Could not find a 'tbl_Surveys' table in the springs layer's geodatabase.",
                    "Build Query");
                return;
            }

            try
            {
                var window = new QueryBuilderWindow(
                    table, vm.ExcludeSurveysWhereClause,
                    "Surveys matching this query will be excluded from the export.")
                {
                    Owner = owner,
                };

                if (window.ShowDialog() == true)
                {
                    vm.ExcludeSurveysWhereClause = window.Expression;
                }
            }
            finally
            {
                StandaloneTable toRemove = table;
                await QueuedTask.Run(() => map.RemoveStandaloneTable(toRemove));
            }
        }

        /// <summary>
        /// Adds tbl_Surveys (from the springs layer's geodatabase) to the layer's
        /// map as a standalone table so the Query Builder can target it. Runs on
        /// the MCT. Returns (null, null) if it cannot be created.
        /// </summary>
        private static (StandaloneTable, Map) CreateSurveysTable(FeatureLayer layer)
        {
            Map map = layer.Map ?? MapView.Active?.Map;
            if (map == null)
            {
                return (null, null);
            }

            try
            {
                using FeatureClass featureClass = layer.GetFeatureClass();
                using Datastore datastore = featureClass.GetDatastore();
                if (datastore is not Geodatabase geodatabase)
                {
                    return (null, map);
                }

                using Table surveys = geodatabase.OpenDataset<Table>("tbl_Surveys");
                StandaloneTable created = StandaloneTableFactory.Instance.CreateStandaloneTable(
                    new StandaloneTableCreationParams(surveys), map);
                return (created, map);
            }
            catch (Exception)
            {
                return (null, map);
            }
        }
    }
}
