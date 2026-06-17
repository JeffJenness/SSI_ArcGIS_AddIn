using System.Windows.Controls;

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
    }
}
