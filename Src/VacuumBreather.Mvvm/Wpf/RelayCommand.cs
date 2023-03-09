using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A command which relays its execution to a delegate.</summary>
public class RelayCommand : IRaisingCommand
{
    /// <summary>A command which does nothing and can always be executed.</summary>
    public static readonly RelayCommand DoNothing = new(() => { });

    private readonly Func<bool>? _canExecute;
    private readonly Action _execute;

    /// <summary>Initializes a new instance of the <see cref="RelayCommand"/> class.</summary>
    /// <param name="execute">The action to perform when the command is executed.</param>
    /// <param name="canExecute">(Optional) The predicate which checks if the command can be executed.</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke() ?? true;
    }

    /// <inheritdoc/>
    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            _execute.Invoke();
        }
    }

    /// <summary>Raises the <see cref="CanExecuteChanged"/> event.</summary>
    public void Refresh()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>A command which relays its execution to a delegate.</summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
[SuppressMessage(category: "StyleCop.CSharp.MaintainabilityRules",
                 checkId: "SA1402:File may only contain a single type",
                 Justification = "Generic version of same type")]
public class RelayCommand<T> : ICommand<T>
{
    private readonly Func<T?, bool>? _canExecute;
    private readonly Action<T?> _execute;

    /// <summary>Initializes a new instance of the <see cref="RelayCommand{T}"/> class.</summary>
    /// <param name="execute">The action to perform when the command is executed.</param>
    /// <param name="canExecute">(Optional) The predicate which checks if the command can be executed.</param>
    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public bool CanExecute(T? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <inheritdoc/>
    public void Execute(T? parameter)
    {
        if (CanExecute(parameter))
        {
            _execute.Invoke(parameter);
        }
    }

    /// <summary>Raises the <see cref="CanExecuteChanged"/> event.</summary>
    public void Refresh()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute((T?)parameter);
    }

    /// <inheritdoc/>
    void ICommand.Execute(object? parameter)
    {
        Execute((T?)parameter);
    }
}