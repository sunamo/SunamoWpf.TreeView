namespace SunamoWpf.TreeView;

/// <summary>
/// Exposes tick/untick commands for a <see cref="SunamoTreeViewUC"/> checkbox tree.
/// Each command takes the target node's path as its <c>CommandParameter</c> (typically bound
/// from the context-menu owning <see cref="Item"/>), so it no longer depends on any specific
/// host window.
/// </summary>
public class ItemsViewModel
{
    /// <summary>
    /// Gets the items of the last tree built by <see cref="ItemProvider.GetItems"/>.
    /// </summary>
    public static ObservableCollection<Item> Items => ItemProvider.LastItems;

    /// <summary>
    /// Gets the command that checks every direct file under the target folder (not recursively).
    /// </summary>
    public ICommand TickInThisItemAllCommand { get; }

    /// <summary>
    /// Gets the command that checks the target folder and every item under it, recursively.
    /// </summary>
    public ICommand TickInThisItemAndUnderAllCommand { get; }

    /// <summary>
    /// Gets the command that unchecks every direct file under the target folder (not recursively).
    /// </summary>
    public ICommand UntickInThisItemAllCommand { get; }

    /// <summary>
    /// Gets the command that unchecks the target folder and every item under it, recursively.
    /// </summary>
    public ICommand UntickInThisItemAndUnderAllCommand { get; }

    /// <summary>
    /// Creates the tick/untick commands operating on <see cref="ItemProvider.LastItemsNonHierarchic"/>.
    /// </summary>
    public ItemsViewModel()
    {
        TickInThisItemAllCommand = new DelegateCommand(path => TickInThisItemAll((string)path!), CanExecuteWithPath);
        TickInThisItemAndUnderAllCommand = new DelegateCommand(path => TickInThisItemAndUnderAll((string)path!), CanExecuteWithPath);
        UntickInThisItemAllCommand = new DelegateCommand(path => UntickInThisItemAll((string)path!), CanExecuteWithPath);
        UntickInThisItemAndUnderAllCommand = new DelegateCommand(path => UntickInThisItemAndUnderAll((string)path!), CanExecuteWithPath);
    }

    private static bool CanExecuteWithPath(object? parameter) => parameter is string { Length: > 0 };

    /// <summary>
    /// Sets <see cref="Item.IsChecked"/> on every item whose <see cref="Item.Path"/> is in <paramref name="paths"/>.
    /// </summary>
    /// <param name="paths">Paths of the items to update.</param>
    /// <param name="isChecked">The value to set.</param>
    public static void TickOrUntick(IEnumerable<string> paths, bool isChecked)
    {
        var pathSet = paths as ICollection<string> ?? paths.ToList();
        foreach (var item in ItemProvider.LastItemsNonHierarchic)
        {
            if (pathSet.Contains(item.Path)) item.IsChecked = isChecked;
        }
    }

    private void TickInThisItemAll(string path) => TickOrUntick(GetDirectChildFiles(path), true);

    private void TickInThisItemAndUnderAll(string path) => TickOrUntick(GetPathAndUnderPaths(path), true);

    private void UntickInThisItemAll(string path) => TickOrUntick(GetDirectChildFiles(path), false);

    private void UntickInThisItemAndUnderAll(string path) => TickOrUntick(GetPathAndUnderPaths(path), false);

    private static List<string> GetPathAndUnderPaths(string path)
    {
        var result = new List<string>();
        foreach (var item in ItemProvider.LastItemsNonHierarchic)
        {
            if (item.Path.StartsWith(path, StringComparison.Ordinal)) result.Add(item.Path);
        }

        return result;
    }

    private static List<string> GetDirectChildFiles(string path)
    {
        var result = new List<string>();
        var depth = (byte)path.Split(System.IO.Path.DirectorySeparatorChar).Length;
        if (!path.EndsWith(System.IO.Path.DirectorySeparatorChar)) depth++;

        foreach (var item in ItemProvider.LastItemsNonHierarchic)
        {
            if (item.TokensCount == depth && !item.IsDirectory && item.Path.StartsWith(path, StringComparison.Ordinal))
                result.Add(item.Path);
        }

        return result;
    }
}
