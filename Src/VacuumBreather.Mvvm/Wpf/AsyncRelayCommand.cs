using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A command which relays its execution to an asynchronous delegate.</summary>
[SuppressMessage(category: "IDisposableAnalyzers.Correctness",
                 checkId: "IDISP006:Implement IDisposable",
                 Justification = "Disposable members are only instantiated in using blocks.")]
[SuppressMessage(category: "IDisposableAnalyzers.Correctness",
                 checkId: "IDISP006:Implement IDisposable",
                 Justification = "Generic version of same type")]
public sealed class AsyncRelayCommand : IAsyncCommand
{
    /// <summary>A command which does nothing and can always be executed.</summary>
    public static readonly AsyncRelayCommand DoNothing = new(_ => ValueTask.CompletedTask);

    private readonly AsyncGuard _asyncGuard = new();

    private readonly Func<CancellationToken, ValueTask> _execute;
    private readonly Func<bool>? _canExecute;

    private CancellationTokenSource? _cts;

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
    public bool IsCancellationRequested => Token.IsCancellationRequested;

    /// <inheritdoc/>
    public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

    /// <inheritdoc/>
    public void Cancel(bool useNewThread = true)
    {
        _cts?.TryCancel(useNewThread);
    }

    /// <inheritdoc/>
    public bool CanExecute()
    {
        return !_asyncGuard.IsOngoing && (_canExecute?.Invoke() ?? true);
    }

    /// <inheritdoc/>
    public async ValueTask ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (CanExecute())
        {
            using (_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                await _execute(_cts.Token).Using(_asyncGuard);
            }

            _cts = null;
        }
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        ThreadHelper.RunOnUIThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty),
                                   cancellationToken: CancellationToken.None);
    }

    /// <inheritdoc/>
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute();
    }

    /// <inheritdoc/>
    void ICommand.Execute(object? parameter)
    {
        ThreadHelper.RunOnUIThreadAndForget(ExecuteAsync, cancellationToken: CancellationToken.None);
    }
}

/// <summary>A command which relays its execution to an asynchronous delegate.</summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
[SuppressMessage(category: "StyleCop.CSharp.MaintainabilityRules",
                 checkId: "SA1402:File may only contain a single type",
                 Justification = "Generic version of same type")]
[SuppressMessage(category: "IDisposableAnalyzers.Correctness",
                 checkId: "IDISP006:Implement IDisposable",
                 Justification = "Generic version of same type")]
public sealed class AsyncRelayCommand<T> : IAsyncCommand<T>
{
    private readonly AsyncGuard _asyncGuard = new();

    private readonly Func<T?, CancellationToken, ValueTask> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private CancellationTokenSource? _cts;

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
    public bool IsCancellationRequested => Token.IsCancellationRequested;

    /// <inheritdoc/>
    public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

    /// <inheritdoc/>
    public void Cancel(bool useNewThread = true)
    {
        _cts?.TryCancel(useNewThread);
    }

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
            using (_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                await _execute(parameter, cancellationToken).Using(_asyncGuard);
            }

            _cts = null;
        }
    }

    /// <inheritdoc/>
    public void Refresh()
    {
        ThreadHelper.RunOnUIThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty),
                                   cancellationToken: CancellationToken.None);
    }

    /// <inheritdoc/>
    bool ICommand.CanExecute(object? parameter)
    {
        return CanExecute((T?)parameter);
    }

    /// <inheritdoc/>
    void ICommand.Execute(object? parameter)
    {
        ThreadHelper.RunOnUIThreadAndForget(token => ExecuteAsync((T?)parameter, token),
                                            cancellationToken: CancellationToken.None);
    }
}