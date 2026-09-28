namespace SunamoWpf.TreeView.Commands;

/// <summary>
/// An <see cref="ICommand"/> implementation that relays <see cref="Execute"/> and
/// <see cref="CanExecute"/> to delegates accepting a command parameter.
/// </summary>
public class DelegateCommand : ICommand
{
    private readonly Action<object?> executeAction;
    private readonly Func<object?, bool>? canExecuteFunc;

    /// <summary>
    /// Creates a command that relays execution to <paramref name="executeAction"/>, optionally
    /// gated by <paramref name="canExecuteFunc"/>.
    /// </summary>
    /// <param name="executeAction">The action invoked when the command executes.</param>
    /// <param name="canExecuteFunc">Predicate deciding whether the command can currently execute.</param>
    public DelegateCommand(Action<object?> executeAction, Func<object?, bool>? canExecuteFunc = null)
    {
        this.executeAction = executeAction;
        this.canExecuteFunc = canExecuteFunc;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => canExecuteFunc?.Invoke(parameter) ?? true;

    /// <inheritdoc />
    public void Execute(object? parameter) => executeAction(parameter);
}
