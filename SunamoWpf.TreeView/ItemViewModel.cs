namespace SunamoWpf.TreeView;

/// <summary>
/// Observable view model of a tree node with name, path and checked state.
/// </summary>
public class ItemViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _path = string.Empty;
    private bool _isChecked;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the display name of the node.
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value, nameof(Name));
    }

    /// <summary>
    /// Gets or sets the full path of the node.
    /// </summary>
    public string Path
    {
        get => _path;
        set => SetField(ref _path, value, nameof(Path));
    }

    /// <summary>
    /// Gets or sets whether the node is checked.
    /// </summary>
    public bool IsChecked
    {
        get => _isChecked;
        set => SetField(ref _isChecked, value, nameof(IsChecked));
    }

    /// <summary>
    /// Raises <see cref="PropertyChanged"/> for the given property.
    /// </summary>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Assigns the field and raises <see cref="PropertyChanged"/> when the value changed.
    /// </summary>
    private void SetField<T>(ref T field, T value, string propertyName)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}
