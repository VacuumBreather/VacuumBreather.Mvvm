using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.Dialogs;

/// <summary>A base class for dialog screens.</summary>
public abstract class DialogScreen : Screen
{
    private TaskCompletionSource<DialogResult>? _taskCompletionSource;
    private CancellationTokenRegistration? _cancellationTokenRegistration;
    private DialogResult _result = DialogResult.None;

    /// <summary>Initializes a new instance of the <see cref="DialogScreen"/> class.</summary>
    protected DialogScreen()
    {
        CloseDialogCommand = new AsyncRelayCommand<DialogResult>(CloseDialogAsync);
    }

    /// <summary>Gets the command to close the dialog.</summary>
    public AsyncRelayCommand<DialogResult> CloseDialogCommand { get; }

    /// <inheritdoc/>
    public sealed override ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(result: true);
    }

    /// <summary>Gets the result this dialog was closed with.</summary>
    /// <returns>
    ///     A <see cref="ValueTask"/> representing the asynchronous operation. The ValueTask result contains the result
    ///     this dialog was closed with.
    /// </returns>
    /// <exception cref="InvalidOperationException">Attempting to await the result before initializing the dialog.</exception>
    public async ValueTask<DialogResult> GetDialogResultAsync()
    {
        if (!IsInitialized)
        {
            ThrowHelper.ThrowInvalidOperationException(
                $"It was attempted to await the dialog result of {GetType().Name} before initializing it.");
        }

        return await _taskCompletionSource!.Task;
    }

    /// <inheritdoc/>
    protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        if (IsInitialized && close)
        {
            await _cancellationTokenRegistration!.Value.DisposeAsync();
            _cancellationTokenRegistration = null;
            _taskCompletionSource!.TrySetResult(_result);
            _taskCompletionSource = null;
            IsInitialized = false;
        }

        await base.OnDeactivateAsync(close, cancellationToken);
    }

    /// <inheritdoc/>
    protected override ValueTask OnInitializeAsync(CancellationToken cancellationToken)
    {
        _taskCompletionSource = new TaskCompletionSource<DialogResult>();

        _cancellationTokenRegistration =
            cancellationToken.Register(() => _taskCompletionSource!.TrySetResult(result: default));

        return base.OnInitializeAsync(cancellationToken);
    }

    private ValueTask CloseDialogAsync(DialogResult dialogResult, CancellationToken token)
    {
        _result = dialogResult;

        return TryCloseAsync(token);
    }
}