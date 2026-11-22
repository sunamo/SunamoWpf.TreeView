// UI of ItemView is built in code (BuildUi) so `dotnet watch` hot reload can refresh it live.
using Binding = System.Windows.Data.Binding;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Orientation = System.Windows.Controls.Orientation;
using RelativeSource = System.Windows.Data.RelativeSource;
using RelativeSourceMode = System.Windows.Data.RelativeSourceMode;
using StackPanel = System.Windows.Controls.StackPanel;
using UiXaml = global::SunamoWpf.TreeView.UiXaml;
using UiHotReload = global::SunamoWpf.TreeView.UiHotReload;

namespace SunamoWpf.TreeView;

partial class ItemView : global::SunamoWpf.TreeView.IRebuildableUi
{
    /// <summary>Builds the whole layout in code and applies all properties; safe to call repeatedly.</summary>
    public void BuildUi()
    {
        var stackPanel1 = new StackPanel();
        stackPanel1.Orientation = Orientation.Vertical;
        var menuItem1 = new MenuItem();
        menuItem1.SetBinding(MenuItem.CommandProperty, new Binding("TickInThisItemAllCommand"));
        menuItem1.SetBinding(MenuItem.CommandParameterProperty, new Binding("PlacementTarget.Tag") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(ContextMenu) } });
        menuItem1.Header = "Zaškrtnout všechny mimo podsložek";
        stackPanel1.Children.Add(menuItem1);
        var menuItem2 = new MenuItem();
        menuItem2.SetBinding(MenuItem.CommandProperty, new Binding("TickInThisItemAndUnderAllCommand"));
        menuItem2.SetBinding(MenuItem.CommandParameterProperty, new Binding("PlacementTarget.Tag") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(ContextMenu) } });
        menuItem2.Header = "Zaškrtnout všechny vč. podprvků";
        stackPanel1.Children.Add(menuItem2);
        var menuItem3 = new MenuItem();
        menuItem3.SetBinding(MenuItem.CommandProperty, new Binding("UntickInThisItemAllCommand"));
        menuItem3.SetBinding(MenuItem.CommandParameterProperty, new Binding("PlacementTarget.Tag") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(ContextMenu) } });
        menuItem3.Header = "Odškrtnout všechny mimo podsložek";
        stackPanel1.Children.Add(menuItem3);
        var menuItem4 = new MenuItem();
        menuItem4.SetBinding(MenuItem.CommandProperty, new Binding("UntickInThisItemAndUnderAllCommand"));
        menuItem4.SetBinding(MenuItem.CommandParameterProperty, new Binding("PlacementTarget.Tag") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(ContextMenu) } });
        menuItem4.Header = "Odškrtnout všechny vč. podprvků";
        stackPanel1.Children.Add(menuItem4);
        this.Content = stackPanel1;
    }
}
