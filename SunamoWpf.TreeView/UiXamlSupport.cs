// Generated from the single template (PowershellScripts\XamlToCs\UiXamlSupport.template.cs); do not edit per project, change the template and re-run Convert-XamlProject.ps1.
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Aliases make every type name unambiguous whatever global usings the project has.
using Exception = System.Exception;
using Type = System.Type;
using Environment = System.Environment;
using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;
using EventArgs = System.EventArgs;
using Math = System.Math;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Utf8JsonWriter = System.Text.Json.Utf8JsonWriter;
using JsonWriterOptions = System.Text.Json.JsonWriterOptions;
using JsonObject = System.Text.Json.Nodes.JsonObject;
using JsonNode = System.Text.Json.Nodes.JsonNode;
using JsonValue = System.Text.Json.Nodes.JsonValue;
using TaskScheduler = System.Threading.Tasks.TaskScheduler;
using Application = System.Windows.Application;
using Window = System.Windows.Window;
using FrameworkElement = System.Windows.FrameworkElement;
using FrameworkContentElement = System.Windows.FrameworkContentElement;
using DependencyObject = System.Windows.DependencyObject;
using DependencyProperty = System.Windows.DependencyProperty;
using NameScope = System.Windows.NameScope;
using Rect = System.Windows.Rect;
using SystemParameters = System.Windows.SystemParameters;
using WindowState = System.Windows.WindowState;
using WindowStartupLocation = System.Windows.WindowStartupLocation;
using ResizeMode = System.Windows.ResizeMode;
using SizeToContent = System.Windows.SizeToContent;
using RoutedEventHandler = System.Windows.RoutedEventHandler;
using RoutedEventArgs = System.Windows.RoutedEventArgs;
using EventManager = System.Windows.EventManager;
using Panel = System.Windows.Controls.Panel;
using ItemsControl = System.Windows.Controls.ItemsControl;
using HeaderedItemsControl = System.Windows.Controls.HeaderedItemsControl;
using ContentControl = System.Windows.Controls.ContentControl;
using HeaderedContentControl = System.Windows.Controls.HeaderedContentControl;
using Decorator = System.Windows.Controls.Decorator;
using Page = System.Windows.Controls.Page;
using TextBox = System.Windows.Controls.TextBox;
using DataGrid = System.Windows.Controls.DataGrid;
using ProgressBar = System.Windows.Controls.ProgressBar;
using Expander = System.Windows.Controls.Expander;
using ListBoxItem = System.Windows.Controls.ListBoxItem;
using DataGridRow = System.Windows.Controls.DataGridRow;
using TreeViewItem = System.Windows.Controls.TreeViewItem;
using ComboBoxItem = System.Windows.Controls.ComboBoxItem;
using DataGridCell = System.Windows.Controls.DataGridCell;
using Popup = System.Windows.Controls.Primitives.Popup;
using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;
using Selector = System.Windows.Controls.Primitives.Selector;
using RangeBase = System.Windows.Controls.Primitives.RangeBase;
using DispatcherTimer = System.Windows.Threading.DispatcherTimer;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;
[assembly: MetadataUpdateHandler(typeof(SunamoWpf.TreeView.UiHotReload))]

namespace SunamoWpf.TreeView;

/// <summary>A view whose whole layout is built in code and can be rebuilt after a hot reload.</summary>
public interface IRebuildableUi
{
    /// <summary>Builds the layout and applies all properties; must be safe to call repeatedly.</summary>
    void BuildUi();
}

/// <summary>Rebuilds all live code-only views after dotnet watch applies a code change, so UI edits show up without a restart.</summary>
public static class UiHotReload
{
    private static readonly List<WeakReference<IRebuildableUi>> views = new();

    /// <summary>Raised on the UI thread after every hot reload; hand-written hosts and factories subscribe to rebuild themselves.</summary>
    public static event Action ReloadRequested;

    /// <summary>Registers a view so it is rebuilt after every hot reload.</summary>
    public static void Register(IRebuildableUi view)
    {
        lock (views)
        {
            views.Add(new WeakReference<IRebuildableUi>(view));
        }
    }

    /// <summary>Called by the runtime after a hot reload delta is applied; rebuilds every live view on the UI thread.</summary>
    public static void UpdateApplication(Type[] updatedTypes)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
        {
            return;
        }

        dispatcher.Invoke(() =>
        {
            List<IRebuildableUi> alive = new();
            lock (views)
            {
                views.RemoveAll(w => !w.TryGetTarget(out IRebuildableUi gone));
                foreach (var w in views)
                {
                    if (w.TryGetTarget(out var view))
                    {
                        alive.Add(view);
                    }
                }
            }

            foreach (var view in alive)
            {
                try
                {
                    view.BuildUi();
                }
                catch (Exception ex)
                {
                    // The previous layout stays on screen; the error is logged so the next save can fix it.
                    UiXaml.LogError("Hot reload rebuild of " + view.GetType().FullName + " failed (" + (updatedTypes?.Length ?? 0) + " updated types)", ex);
                }
            }

            try
            {
                ReloadRequested?.Invoke();
            }
            catch (Exception ex)
            {
                UiXaml.LogError("A ReloadRequested handler failed", ex);
            }
        });
    }
}

/// <summary>Helpers used by code-only views: state-preserving property setter, named elements, type conversion, resources and dev state.</summary>
public static class UiXaml
{
    private static readonly bool devMode = Environment.GetEnvironmentVariable("DOTNET_WATCH") == "1";
    private static readonly object stateLock = new();
    private static readonly ConditionalWeakTable<object, object> tracked = new();
    private static readonly ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, object>> lastBuilt = new();
    private static readonly Lazy<JsonObject> state = new(ReadState);
    private static readonly Lazy<DispatcherTimer> saveTimer = new(CreateSaveTimer);

    /// <summary>True only when the app runs under dotnet watch; all dev state features are disabled otherwise.</summary>
    internal static bool DevMode => devMode;

    /// <summary>
    /// Sets a property of a persistent element from the builder. When the app or the user changed the value since the
    /// last build, the runtime value wins and is kept, so a hot-reload rebuild never throws away live state.
    /// </summary>
    public static void Set(DependencyObject target, DependencyProperty property, object value)
    {
        var built = lastBuilt.GetOrCreateValue(target);
        if (built.TryGetValue(property, out var previous) && !Equals(target.GetValue(property), previous))
        {
            return;
        }

        target.SetValue(property, value);
        built[property] = target.GetValue(property);
    }

    /// <summary>Removes the controls from whatever currently contains them so they can be added to a fresh layout tree.</summary>
    public static void Detach(params object[] elements)
    {
        foreach (var o in elements)
        {
            if (o is not FrameworkElement element)
            {
                continue;
            }

            switch (element.Parent)
            {
                case Panel panel:
                    panel.Children.Remove(element);
                    break;
                case HeaderedItemsControl headeredItems when ReferenceEquals(headeredItems.Header, element):
                    headeredItems.Header = null;
                    break;
                case ItemsControl itemsControl:
                    itemsControl.Items.Remove(element);
                    break;
                case HeaderedContentControl headered when ReferenceEquals(headered.Header, element):
                    headered.Header = null;
                    break;
                case ContentControl contentControl:
                    if (ReferenceEquals(contentControl.Content, element))
                    {
                        contentControl.Content = null;
                    }

                    break;
                case Decorator decorator:
                    decorator.Child = null;
                    break;
                case Page page:
                    page.Content = null;
                    break;
                case Popup popup:
                    popup.Child = null;
                    break;
            }
        }
    }

    /// <summary>Names an element like x:Name did: sets Name, registers it in the name scope of the view and restores its dev state.</summary>
    public static void Register(FrameworkElement scope, string name, object element)
    {
        if (NameScope.GetNameScope(scope) == null)
        {
            NameScope.SetNameScope(scope, new NameScope());
        }

        if (element is FrameworkElement fe && fe.Name != name)
        {
            fe.Name = name;
        }
        else if (element is FrameworkContentElement fce && fce.Name != name)
        {
            fce.Name = name;
        }

        if (scope.FindName(name) != null)
        {
            scope.UnregisterName(name);
        }

        scope.RegisterName(name, element);
        TrackState(scope.GetType().FullName + ":" + name, element);
    }

    /// <summary>Converts a XAML-style string to the target type through its type converter.</summary>
    public static T Convert<T>(string text)
    {
        var converter = TypeDescriptor.GetConverter(Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
        return (T)converter.ConvertFromInvariantString(text)!;
    }

    /// <summary>Finds an application-level resource by key.</summary>
    public static object FindResource(object key)
    {
        return Application.Current?.TryFindResource(key);
    }

    /// <summary>Writes an error with its stack trace to a log file in the temp folder.</summary>
    public static void LogError(string message, Exception ex)
    {
        string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "UiHotReload-errors.log");
        System.IO.File.AppendAllText(path, DateTime.Now.ToString("s") + " [" + typeof(UiXaml).Assembly.GetName().Name + "] " + message + Environment.NewLine + ex + Environment.NewLine);
    }

    // ---- dev-state json: a light file with navigation and input state, restored after a full dotnet watch restart ----

    /// <summary>Path of the light state file of this assembly (one file per assembly so apps and libraries never overwrite each other).</summary>
    public static string StatePath => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "_Sunamo", "DevState",
        System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "app", "dev-state." + typeof(UiXaml).Assembly.GetName().Name + ".json");

    /// <summary>Reads a value saved by SetState; returns the default when not under dotnet watch or when missing.</summary>
    public static T GetState<T>(string key, T defaultValue = default)
    {
        if (!devMode)
        {
            return defaultValue;
        }

        lock (stateLock)
        {
            try
            {
                return state.Value.TryGetPropertyValue("app:" + key, out var node) && node != null ? node.Deserialize<T>() : defaultValue;
            }
            catch (Exception ex)
            {
                LogError("dev-state value " + key + " could not be read", ex);
                return defaultValue;
            }
        }
    }

    /// <summary>Saves a value (current screen, filter, selected id) so it is available again after a full restart under dotnet watch.</summary>
    public static void SetState<T>(string key, T value)
    {
        if (!devMode)
        {
            return;
        }

        Save("app:" + key, JsonSerializer.SerializeToNode(value));
    }

    /// <summary>Restores the saved value of the control and starts saving its changes.</summary>
    internal static void TrackState(string key, object element)
    {
        if (!devMode || tracked.TryGetValue(element, out object known))
        {
            return;
        }

        tracked.Add(element, key);
        try
        {
            lock (stateLock)
            {
                if (state.Value.TryGetPropertyValue(key, out var saved) && saved != null)
                {
                    Apply(element, saved);
                }
            }

            switch (element)
            {
                case TextBox tb when !tb.IsReadOnly:
                    tb.TextChanged += (_, _) => Save(key, JsonValue.Create(tb.Text.Length > 20000 ? tb.Text[..20000] : tb.Text));
                    break;
                case ToggleButton toggle:
                    toggle.Checked += (_, _) => Save(key, JsonValue.Create(true));
                    toggle.Unchecked += (_, _) => Save(key, JsonValue.Create(false));
                    break;
                case Selector selector and not DataGrid:
                    selector.SelectionChanged += (s, e) =>
                    {
                        if (ReferenceEquals(e.OriginalSource, selector))
                        {
                            Save(key, JsonValue.Create(selector.SelectedIndex));
                        }
                    };
                    break;
                case RangeBase range and not ProgressBar:
                    range.ValueChanged += (_, _) => Save(key, JsonValue.Create(range.Value));
                    break;
                case Expander expander:
                    expander.Expanded += (_, _) => Save(key, JsonValue.Create(true));
                    expander.Collapsed += (_, _) => Save(key, JsonValue.Create(false));
                    break;
            }
        }
        catch (Exception ex)
        {
            LogError("dev-state tracking of " + key + " failed", ex);
        }
    }

    private static void Apply(object element, JsonNode saved)
    {
        switch (element)
        {
            case TextBox tb when !tb.IsReadOnly:
                tb.Text = saved.GetValue<string>();
                break;
            case ToggleButton toggle:
                toggle.IsChecked = saved.GetValue<bool>();
                break;
            case Selector selector and not DataGrid:
                int index = saved.GetValue<int>();
                if (index < selector.Items.Count)
                {
                    selector.SelectedIndex = index;
                }

                break;
            case RangeBase range and not ProgressBar:
                range.Value = saved.GetValue<double>();
                break;
            case Expander expander:
                expander.IsExpanded = saved.GetValue<bool>();
                break;
        }
    }

    private static JsonObject ReadState()
    {
        try
        {
            return System.IO.File.Exists(StatePath) ? JsonNode.Parse(System.IO.File.ReadAllText(StatePath)) as JsonObject ?? new JsonObject() : new JsonObject();
        }
        catch (Exception ex)
        {
            LogError("dev-state json could not be read", ex);
            return new JsonObject();
        }
    }

    private static DispatcherTimer CreateSaveTimer()
    {
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(400) };
        timer.Tick += FlushState;
        return timer;
    }

    private static void Save(string key, JsonNode value)
    {
        lock (stateLock)
        {
            state.Value[key] = value;
        }

        if (Application.Current?.Dispatcher.CheckAccess() == true)
        {
            saveTimer.Value.Stop();
            saveTimer.Value.Start();
        }
        else
        {
            WriteStateFile();
        }
    }

    private static void FlushState(object sender, EventArgs e)
    {
        saveTimer.Value.Stop();
        WriteStateFile();
    }

    private static void WriteStateFile()
    {
        try
        {
            lock (stateLock)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(StatePath)!);
                using var stream = System.IO.File.Create(StatePath);
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                state.Value.WriteTo(writer);
            }
        }
        catch (Exception ex)
        {
            LogError("dev-state json could not be written", ex);
        }
    }

    /// <summary>Builds a stable key for an element: names where present, otherwise type and index among same-type siblings.</summary>
    internal static string PathOf(DependencyObject element, DependencyObject root)
    {
        var parts = new List<string>();
        DependencyObject current = element;
        while (current != null && current != root)
        {
            DependencyObject parent = System.Windows.Media.VisualTreeHelper.GetParent(current);
            string segment;
            if (current is FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
            {
                segment = fe.Name;
            }
            else
            {
                int index = 0;
                if (parent != null)
                {
                    int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
                    for (int i = 0; i < count; i++)
                    {
                        var sibling = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                        if (sibling == current)
                        {
                            break;
                        }

                        if (sibling.GetType() == current.GetType())
                        {
                            index++;
                        }
                    }
                }

                segment = current.GetType().Name + "[" + index + "]";
            }

            parts.Insert(0, segment);
            current = parent;
        }

        return string.Join("/", parts);
    }

    /// <summary>True for controls whose user-entered value is worth keeping and whose position in the tree is stable.</summary>
    internal static bool IsTrackable(FrameworkElement element)
    {
        if (element.TemplatedParent != null)
        {
            return false;
        }

        bool kind = element is TextBox { IsReadOnly: false } || element is ToggleButton
            || element is Selector and not DataGrid || element is RangeBase and not ProgressBar || element is Expander;
        if (!kind)
        {
            return false;
        }

        for (DependencyObject p = System.Windows.Media.VisualTreeHelper.GetParent(element); p != null; p = System.Windows.Media.VisualTreeHelper.GetParent(p))
        {
            if (p is ListBoxItem or DataGridRow or TreeViewItem or ComboBoxItem or DataGridCell)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Restores and tracks the position, size and state of a window.</summary>
    internal static void TrackWindow(Window window)
    {
        if (!devMode || tracked.TryGetValue(window, out object known))
        {
            return;
        }

        tracked.Add(window, "window");
        string key = window.GetType().FullName + ":window";
        try
        {
            lock (stateLock)
            {
                if (state.Value.TryGetPropertyValue(key, out var saved) && saved is JsonObject o)
                {
                    double left = o["l"]?.GetValue<double>() ?? window.Left;
                    double top = o["t"]?.GetValue<double>() ?? window.Top;
                    double width = o["w"]?.GetValue<double>() ?? window.Width;
                    double height = o["h"]?.GetValue<double>() ?? window.Height;
                    bool visible = left + width > SystemParameters.VirtualScreenLeft + 40
                        && left < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 40
                        && top > SystemParameters.VirtualScreenTop - 10
                        && top < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 40;
                    if (visible && window.WindowState == WindowState.Normal)
                    {
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Left = left;
                        window.Top = top;
                        if (window.ResizeMode != ResizeMode.NoResize && window.SizeToContent == SizeToContent.Manual)
                        {
                            window.Width = width;
                            window.Height = height;
                        }
                    }

                    if (o["s"]?.GetValue<int>() == (int)WindowState.Maximized && window.ResizeMode != ResizeMode.NoResize)
                    {
                        window.WindowState = WindowState.Maximized;
                    }
                }
            }

            void SaveWindow(object s, EventArgs e)
            {
                var bounds = window.WindowState == WindowState.Normal ? new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight) : window.RestoreBounds;
                if (bounds.IsEmpty || double.IsNaN(bounds.Left) || double.IsNaN(bounds.Top))
                {
                    return;
                }

                Save(key, new JsonObject
                {
                    ["l"] = bounds.Left, ["t"] = bounds.Top, ["w"] = bounds.Width, ["h"] = bounds.Height, ["s"] = (int)window.WindowState,
                });
            }

            window.LocationChanged += SaveWindow;
            window.SizeChanged += SaveWindow;
            window.StateChanged += SaveWindow;
        }
        catch (Exception ex)
        {
            LogError("dev-state tracking of window " + key + " failed", ex);
        }
    }

    /// <summary>Keeps the app alive on an unhandled UI exception under dotnet watch: logs it and shows it in the window title.</summary>
    internal static void InstallRecovery()
    {
        if (!devMode || Application.Current == null)
        {
            return;
        }

        Application.Current.DispatcherUnhandledException += (_, e) =>
        {
            LogError("Unhandled UI exception (kept alive under dotnet watch)", e.Exception);
            if (!e.Handled)
            {
                e.Handled = true;
                var window = Application.Current.MainWindow;
                if (window != null)
                {
                    string title = window.Title ?? string.Empty;
                    int end = title.StartsWith("[dev error") ? title.IndexOf(']') + 1 : 0;
                    window.Title = "[dev error: " + e.Exception.GetType().Name + ", fix the code and save] " + title[end..].Trim();
                }
            }
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogError("Unobserved task exception", e.Exception);
            e.SetObserved();
        };
    }
}
