namespace SunamoWpf.TreeView;

/// <summary>
/// Checkbox tree view that displays a filesystem folder hierarchy built by <see cref="ItemProvider"/>,
/// with a per-node context menu offering bulk tick/untick commands (see <see cref="ItemsViewModel"/>).
/// </summary>
public partial class SunamoTreeViewUC : UserControl
{
    /// <summary>
    /// Initializes the tree view control.
    /// </summary>
    public SunamoTreeViewUC()
    {
        InitializeComponent();
    }
}
