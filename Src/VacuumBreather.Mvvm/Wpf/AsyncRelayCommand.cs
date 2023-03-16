using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A command which relays its execution to an asynchronous delegate.</summary>
public sealed class AsyncRelayCommand : IAsyncCommand
{
    /// <summary>A command which does nothing and can always be executed.</summary>
    public static readonly AsyncRelayCommand DoNothing = new(_ => ValueTask.CompletedTask);

    private readonly AsyncGuard _asyncGuard = new();

    private readonly Func<CancellationToken, ValueTask> _execute;
    private readonly Func<bool>? _canExecute;

    /// <summary>Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.</summary>
    /// <param name="execute">The asynchronous action to perform when the command is executed.</param>
    /// <param name="canExecute">(Optional) The predicate which checks if the command can be executed.</param>
    public AsyncRelayCommand(Func<CancellationToken, ValueTask> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;

        _asyncGuard.IsOngoingChanged += (_, _) => Refresh();
    }

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public bool CanExecute()
    {
        return !_asyncGuard.IsOngoing && (_canExecute?.Invoke() ?? true);
    }

    /// <param name="cancellationToken"></param>
    /// <inheritdoc/>
    public async ValueTask ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (CanExecute())
        {
            await _execute(cancellationToken).Using(_asyncGuard);
        }
    }

    /// <summary>Raises the <see cref="CanExecuteChanged"/> event.</summary>
    public void Refresh()
    {
        ThreadHelper.RunOnUIThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
    }

    /// <inheritdoc/>
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute();
    }

    /// <inheritdoc/>
    void ICommand.Execute(object? parameter)
    {
        ExecuteAsync().Forget();
    }
}

/// <summary>A command which relays its execution to an asynchronous delegate.</summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
[SuppressMessage(category: "StyleCop.CSharp.MaintainabilityRules",
                 checkId: "SA1402:File may only contain a single type",
                 Justification = "Generic version of same type")]
public sealed class AsyncRelayCommand<T> : IAsyncCommand<T>
{
    private readonly AsyncGuard _asyncGuard = new();

    private readonly Func<T?, CancellationToken, ValueTask> _execute;
    private readonly Func<T?, bool>? _canExecute;

    /// <summary>Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.</summary>
    /// <param name="execute">The asynchronous action to perform when the command is executed.</param>
    /// <param name="canExecute">(Optional) The predicate which checks if the command can be executed.</param>
    public AsyncRelayCommand(Func<T?, CancellationToken, ValueTask> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;

        _asyncGuard.IsOngoingChanged += (_, _) => Refresh();
    }

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc/>
    public bool CanExecute(T? parameter)
    {
        return !_asyncGuard.IsOngoing && (_canExecute?.Invoke(parameter) ?? true);
    }

    /// <inheritdoc/>
    public async ValueTask ExecuteAsync(T? parameter, CancellationToken cancellationToken = default)
    {
        if (CanExecute(parameter))
        {
            await _execute(parameter, cancellationToken).Using(_asyncGuard);
        }
    }

    /// <summary>Raises the <see cref="CanExecuteChanged"/> event.</summary>
    public void Refresh()
    {
        ThreadHelper.RunOnUIThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
    }

    /// <inheritdoc/>
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute((T?)parameter);
    }

    /// <inheritdoc/>
    void ICommand.Execute(object? parameter)
    {
        ExecuteAsync((T?)parameter).Forget();
    }
}