// UI of SunamoTreeViewUC is built in code (BuildUi) so `dotnet watch` hot reload can refresh it live.
using Binding = System.Windows.Data.Binding;
using BindingMode = System.Windows.Data.BindingMode;
using CheckBox = System.Windows.Controls.CheckBox;
using ContentPresenter = System.Windows.Controls.ContentPresenter;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataTemplate = System.Windows.DataTemplate;
using DataTemplateKey = System.Windows.DataTemplateKey;
using FrameworkElementFactory = System.Windows.FrameworkElementFactory;
using HierarchicalDataTemplate = System.Windows.HierarchicalDataTemplate;
using Orientation = System.Windows.Controls.Orientation;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = System.Windows.Controls.TextBlock;
using Thickness = System.Windows.Thickness;
using UiXaml = global::SunamoWpf.TreeView.UiXaml;
using UiHotReload = global::SunamoWpf.TreeView.UiHotReload;

namespace SunamoWpf.TreeView;

partial class SunamoTreeViewUC : global::SunamoWpf.TreeView.IRebuildableUi
{
    // Named elements are created once (lazily in BuildUi) so their state survives a hot-reload rebuild.
    internal global::System.Windows.Controls.TreeView tvFolders;

    /// <summary>Builds the whole layout in code and applies all properties; safe to call repeatedly.</summary>
    public void BuildUi()
    {
        tvFolders ??= new();
        UiXaml.Detach(tvFolders);
        var dataTemplate1 = new DataTemplate();
        dataTemplate1.DataType = typeof(global::SunamoWpf.TreeView.ItemsViewModel);
        var factory1 = new FrameworkElementFactory(typeof(global::SunamoWpf.TreeView.ItemView));
        dataTemplate1.VisualTree = factory1;
        this.Resources[new DataTemplateKey(typeof(global::SunamoWpf.TreeView.ItemsViewModel))] = dataTemplate1;
        UiXaml.Register(this, "tvFolders", tvFolders);
        var hierarchicalDataTemplate1 = new HierarchicalDataTemplate();
        hierarchicalDataTemplate1.DataType = typeof(global::SunamoWpf.TreeView.Item);
        hierarchicalDataTemplate1.ItemsSource = new Binding("Items");
        var factory2 = new FrameworkElementFactory(typeof(StackPanel));
        factory2.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        factory2.SetBinding(StackPanel.TagProperty, new Binding("Path"));
        var contextMenu1 = new ContextMenu();
        var contentPresenter1 = new ContentPresenter();
        var itemsViewModel1 = new global::SunamoWpf.TreeView.ItemsViewModel();
        contentPresenter1.Content = itemsViewModel1;
        contextMenu1.Items.Add(contentPresenter1);
        factory2.SetValue(StackPanel.ContextMenuProperty, contextMenu1);
        var factory3 = new FrameworkElementFactory(typeof(CheckBox));
        factory3.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsChecked") { Mode = BindingMode.TwoWay });
        factory3.SetBinding(CheckBox.TagProperty, new Binding("Path"));
        factory2.AppendChild(factory3);
        var factory4 = new FrameworkElementFactory(typeof(TextBlock));
        factory4.SetBinding(TextBlock.TextProperty, new Binding("Name"));
        factory4.SetValue(TextBlock.PaddingProperty, new Thickness(0));
        factory4.SetValue(TextBlock.MarginProperty, new Thickness(3, -2, 0, 2));
        factory4.SetBinding(TextBlock.ToolTipProperty, new Binding("Path"));
        factory2.AppendChild(factory4);
        hierarchicalDataTemplate1.VisualTree = factory2;
        tvFolders.ItemTemplate = hierarchicalDataTemplate1;
        this.Content = tvFolders;
    }
}
