using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Core;

/// <summary>A base implementation of <see cref="IScreen" />.</summary>
[PublicAPI]
[SuppressMessage("Design",
                 "CA1001:Types that own disposable fields should be disposable",
                 Justification = "The fields in question are only ever instantiated in using blocks")]
public abstract class Screen : ViewAware, IScreen, IChild
{
    private TaskCompletionSource? _initializationCompletion;
    private TaskCompletionSource? _activateCompletion;
    private TaskCompletionSource? _deactivateCompletion;

    private SafeCancellationTokenSource? _activateCancellation;
    private SafeCancellationTokenSource? _deactivateCancellation;

    private string _displayName;
    private bool _isActive;
    private bool _isInitialized;
    private object? _parent;

    /// <summary>Initializes a new instance of the <see cref="Screen" /> class.</summary>
    protected Screen()
    {
        _displayName = GetType().Name;
    }

    /// <inheritdoc />
    public event AsyncEventHandler<ActivationEventArgs>? Activated;

    /// <inheritdoc />
    public event AsyncEventHandler<DeactivationEventArgs>? Deactivated;

    /// <inheritdoc />
    public event AsyncEventHandler<DeactivationEventArgs>? Deactivating;

    /// <summary>Gets or sets a value indicating whether this instance has been initialized.</summary>
    public bool IsInitialized
    {
        get => _isInitialized;
        protected set => SetProperty(ref _isInitialized, value);
    }

    /// <inheritdoc />
    public object? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }

    /// <inheritdoc />
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    /// <inheritdoc />
    public bool IsActive
    {
        get => _isActive;
        private set => SetProperty(ref _isActive, value);
    }

    /// <inheritdoc />
    public virtual ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(true);
    }

    /// <inheritdoc />
    public virtual async ValueTask TryCloseAsync(bool? dialogResult = null,
                                                 CancellationToken cancellationToken = default)
    {
        if (Parent is IConductor conductor)
        {
            await conductor.CloseItemAsync(this, cancellationToken).ConfigureAwait(true);
        }
    }

    /// <inheritdoc />
    public async ValueTask ActivateAsync(CancellationToken cancellationToken)
    {
        if (IsActive)
        {
            return;
        }

        // Guard against multiple simultaneous executions and provide cancellation source.
        await (_activateCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);
        using var guard = TaskCompletion.CreateGuard(out _activateCompletion);

        using (_activateCancellation = SafeCancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            // Cancel deactivation and wait for potential synchronous steps to complete.
            _deactivateCancellation?.Cancel();

            await (_deactivateCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);

            bool initialized = false;

            if (!IsInitialized)
            {
                using var initGuard = TaskCompletion.CreateGuard(out _initializationCompletion);

                Logger.LogDebug("Initializing {Screen}...", GetType().Name);

                // Deactivation is not allowed to cancel initialization, so we are only
                // using the token that was passed to us.
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.LogDebug("Initialization of {Screen} canceled", GetType().Name);

                    return;
                }

                await OnInitializeAsync(cancellationToken).ConfigureAwait(true);

                IsInitialized = initialized = true;
            }

            Logger.LogTrace("Activating {Screen}...", GetType().Name);

            if (_activateCancellation.IsCancellationRequested)
            {
                Logger.LogTrace("Activation of {Screen} canceled", GetType().Name);

                return;
            }

            await OnActivateAsync(_activateCancellation.Token).ConfigureAwait(true);

            IsActive = true;

            await RaiseActivatedAsync(initialized, _activateCancellation.Token).ConfigureAwait(true);

            Logger.LogTrace("Activated {Screen}", GetType().Name);

            if (initialized)
            {
                Logger.LogDebug("Initialized {Screen}", GetType().Name);
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        // Guard against multiple simultaneous executions and provide cancellation source.
        await (_deactivateCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);
        using var guard = TaskCompletion.CreateGuard(out _deactivateCompletion);

        using (_deactivateCancellation = SafeCancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            if (!IsInitialized)
            {
                // We do not allow deactivation before initialization.
                await (_initializationCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);
            }

            if (_deactivateCancellation.IsCancellationRequested)
            {
                Logger.LogTrace("Deactivation of {Screen} canceled", GetType().Name);

                return;
            }

            // Cancel activation and wait for potential synchronous steps to complete.
            _activateCancellation?.Cancel();

            await (_activateCompletion?.Task ?? Task.CompletedTask).ConfigureAwait(true);

            if (IsActive || (IsInitialized && close))
            {
                if (close)
                {
                    Logger.LogDebug("Closing {Screen}...", GetType().Name);
                }
                else
                {
                    Logger.LogTrace("Deactivating {Screen}...", GetType().Name);
                }

                await RaiseDeactivatingAsync(close, cancellationToken).ConfigureAwait(true);
                await OnDeactivateAsync(close, cancellationToken).ConfigureAwait(true);

                IsActive = false;

                await RaiseDeactivatedAsync(close, cancellationToken).ConfigureAwait(true);

                if (close)
                {
                    Logger.LogDebug("Closed {Screen}", GetType().Name);
                    IsInitialized = false;
                }
                else
                {
                    Logger.LogTrace("Deactivated {Screen}", GetType().Name);
                }
            }
        }
    }

    /// <summary>Called when activating.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    protected virtual ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Called when deactivating.</summary>
    /// <param name="close">Indicates whether this instance will be closed.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    protected virtual ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Called when initializing.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    protected virtual ValueTask OnInitializeAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    private async ValueTask RaiseActivatedAsync(bool wasInitialized, CancellationToken cancellationToken)
    {
        ValueTask task = Activated?.InvokeAllAsync(this, new ActivationEventArgs(wasInitialized), cancellationToken) ??
                         ValueTask.CompletedTask;

        await task.ConfigureAwait(true);
    }

    private async ValueTask RaiseDeactivatedAsync(bool wasClosed, CancellationToken cancellationToken)
    {
        ValueTask task = Deactivated?.InvokeAllAsync(this, new DeactivationEventArgs(wasClosed), cancellationToken) ??
                         ValueTask.CompletedTask;

        await task.ConfigureAwait(true);
    }

    private async ValueTask RaiseDeactivatingAsync(bool wasClosed, CancellationToken cancellationToken)
    {
        ValueTask task = Deactivating?.InvokeAllAsync(this, new DeactivationEventArgs(wasClosed), cancellationToken) ??
                         ValueTask.CompletedTask;

        await task.ConfigureAwait(true);
    }
}