namespace SunamoWpf.TreeView;

/// <summary>
/// One node (file or folder) in a <see cref="SunamoTreeViewUC"/> checkbox tree.
/// </summary>
public class Item : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the child nodes of this item (empty for files).
    /// </summary>
    public ObservableCollection<Item> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the display name of the item (file or folder name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full filesystem path of the item.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this item represents a directory (as opposed to a file).
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// Gets or sets the number of path segments (depth) of this item, used to match
    /// direct children of a folder without recursing into subfolders.
    /// </summary>
    public byte TokensCount { get; set; }

    private bool isCheckedField;

    /// <summary>
    /// Gets or sets whether the item's checkbox is checked.
    /// </summary>
    public bool IsChecked
    {
        get => isCheckedField;
        set
        {
            if (isCheckedField == value) return;
            isCheckedField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
        }
    }
}
