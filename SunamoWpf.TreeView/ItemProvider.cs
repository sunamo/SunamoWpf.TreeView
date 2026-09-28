namespace SunamoWpf.TreeView;

/// <summary>
/// Builds a hierarchical <see cref="Item"/> tree from the filesystem, for use with
/// <see cref="SunamoTreeViewUC"/>.
/// </summary>
public static class ItemProvider
{
    /// <summary>
    /// Gets the last built items, hierarchic (top level of the last <see cref="GetItems"/> call).
    /// </summary>
    public static ObservableCollection<Item> LastItems { get; private set; } = [];

    /// <summary>
    /// Gets the last built items, flattened (every file and folder from the last <see cref="GetItems"/> call).
    /// </summary>
    public static ObservableCollection<Item> LastItemsNonHierarchic { get; } = [];

    /// <summary>
    /// Recursively builds the item tree for <paramref name="path"/> and records it in
    /// <see cref="LastItems"/> and <see cref="LastItemsNonHierarchic"/>.
    /// </summary>
    /// <param name="path">The root folder to scan.</param>
    /// <returns>The top-level items found directly under <paramref name="path"/>.</returns>
    public static ObservableCollection<Item> GetItems(string path)
    {
        var items = new ObservableCollection<Item>();
        var directoryInfo = new DirectoryInfo(path);

        DirectoryInfo[]? directories = null;
        try
        {
            directories = directoryInfo.GetDirectories();
        }
        catch (Exception)
        {
            // No access to enumerate subdirectories, treat as leaf.
        }

        if (directories is not null)
        {
            foreach (var directory in directories)
            {
                var directoryItem = new Item
                {
                    Name = directory.Name,
                    Path = directory.FullName + System.IO.Path.DirectorySeparatorChar,
                    Items = GetItems(directory.FullName),
                    IsDirectory = true,
                    TokensCount = (byte)directory.FullName.Split(System.IO.Path.DirectorySeparatorChar).Length
                };

                LastItemsNonHierarchic.Add(directoryItem);
                items.Add(directoryItem);
            }
        }

        FileInfo[]? files = null;
        try
        {
            files = directoryInfo.GetFiles();
        }
        catch (Exception)
        {
            // No access to enumerate files, treat as leaf.
        }

        if (files is not null)
        {
            foreach (var file in files)
            {
                var fileItem = new Item
                {
                    Name = file.Name,
                    Path = file.FullName,
                    IsDirectory = false,
                    TokensCount = (byte)file.FullName.Split(System.IO.Path.DirectorySeparatorChar).Length
                };

                LastItemsNonHierarchic.Add(fileItem);
                items.Add(fileItem);
            }
        }

        LastItems = items;
        return items;
    }
}
