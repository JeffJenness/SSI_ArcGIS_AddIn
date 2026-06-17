using System.Windows;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;

namespace SSI_ArcGIS_Addin
{
    /// <summary>
    /// Hosts the ArcGIS Pro QueryBuilderControl so the user can build a SQL
    /// where-clause against a map member (the springs layer, or a standalone
    /// table for tbl_Surveys). The resulting clause is exposed via
    /// <see cref="Expression"/>.
    /// </summary>
    public partial class QueryBuilderWindow : ProWindow
    {
        private readonly MapMember _member;
        private readonly string _initialExpression;

        internal QueryBuilderWindow(MapMember member, string initialExpression, string prompt)
        {
            InitializeComponent();
            _member = member;
            _initialExpression = initialExpression ?? string.Empty;
            PromptText.Text = prompt;
            Loaded += OnLoaded;
        }

        /// <summary>The where-clause the user built (empty if none).</summary>
        internal string Expression => QbControl.Expression ?? string.Empty;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            QbControl.ConfigureControl = new QueryBuilderControlProperties
            {
                MapMember = _member,
                Expression = _initialExpression,
            };
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
