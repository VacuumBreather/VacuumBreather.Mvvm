using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Core;

/// <summary>A base implementation of <see cref="IScreen"/>.</summary>
[PublicAPI]
[SuppressMessage(category: "Design",
                 checkId: "CA1001:Types that own disposable fields should be disposable",
                 Justification = "The fields in question are only ever instantiated in using blocks")]
public abstract class Screen : ViewAware, IScreen, IChild, IHaveAsynchronousOperations
{
    private IAsyncOperation? _initializationOperation;
    private IAsyncOperation? _activateOperation;
    private IAsyncOperation? _deactivateOperation;

    private string _displayName;
    private bool _isActive;
    private bool _isInitialized;
    private object? _parent;

    /// <summary>Initializes a new instance of the <see cref="Screen"/> class.</summary>
    protected Screen()
    {
        _displayName = GetType().Name;
        AsyncGuard.IsOngoingChanged += (_, _) => OnPropertyChanged(nameof(IsBusy));
    }

    /// <inheritdoc/>
    public event AsyncEventHandler<ActivationEventArgs>? Activated;

    /// <inheritdoc/>
    public event AsyncEventHandler<ActivatingEventArgs>? Activating;

    /// <inheritdoc/>
    public event AsyncEventHandler<DeactivationEventArgs>? Deactivated;

    /// <inheritdoc/>
    public event AsyncEventHandler<DeactivatingEventArgs>? Deactivating;

    /// <summary>Gets or sets a value indicating whether this instance has been initialized.</summary>
    public bool IsInitialized
    {
        get => _isInitialized;
        protected set => SetProperty(ref _isInitialized, value);
    }

    /// <inheritdoc/>
    public object? Parent
    {
        get => _parent;
        set => SetProperty(ref _parent, value);
    }

    /// <inheritdoc/>
    public bool IsBusy => AsyncGuard.IsOngoing;

    /// <inheritdoc/>
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    /// <inheritdoc/>
    public bool IsActive
    {
        get => _isActive;
        private set => SetProperty(ref _isActive, value);
    }

    /// <summary>
    ///     Gets the <see cref="AsyncGuard"/> which is keeping track of ongoing asynchronous operations on this
    ///     <see cref="BindableObject"/>. Use tokens from this guard in a using block to mark the scope of an asynchronous
    ///     operation that should set the IsBusy state, while ongoing.
    /// </summary>
    protected AsyncGuard AsyncGuard { get; } = new();

    /// <inheritdoc/>
    public virtual ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(result: true);
    }

    /// <inheritdoc/>
    public virtual async ValueTask TryCloseAsync(bool? dialogResult = null,
                                                 CancellationToken cancellationToken = default)
    {
        if (Parent is IConductor conductor)
        {
            await conductor.CloseItemAsync(this, cancellationToken);
        }
        else if (await CanCloseAsync(cancellationToken))
        {
            await DeactivateAsync(close: true, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async ValueTask ActivateAsync(CancellationToken cancellationToken = default)
    {
        if (IsActive)
        {
            return;
        }

        // Guard against multiple simultaneous executions.
        await AsyncHelper.AwaitCompletionAsync(_activateOperation, cancellationToken);

        using var operation = AsyncHelper.CreateAsyncOperation(cancellationToken)
                                         .CancelWhenDeactivating(this)
                                         .Assign(out _activateOperation);

        await RaiseActivatingAsync(!IsInitialized, operation.Token);

        var initialized = false;

        if (!IsInitialized)
        {
            // Deactivation is not allowed to cancel initialization.
            using var initOperation = AsyncHelper.CreateAsyncOperation(cancellationToken)
                                                 .Assign(out _initializationOperation);

            Logger.LogDebug(message: "Initializing {Screen}...", GetType().Name);

            if (initOperation.IsCancellationRequested)
            {
                Logger.LogDebug(message: "Initialization of {Screen} canceled", GetType().Name);

                return;
            }

            await OnInitializeAsync(initOperation.Token);

            IsInitialized = initialized = true;
        }

        Logger.LogTrace(message: "Activating {Screen}...", GetType().Name);

        if (operation.IsCancellationRequested)
        {
            Logger.LogTrace(message: "Activation of {Screen} canceled", GetType().Name);

            return;
        }

        await OnActivateAsync(operation.Token);

        IsActive = true;

        await RaiseActivatedAsync(initialized, operation.Token);

        Logger.LogTrace(message: "Activated {Screen}", GetType().Name);

        if (initialized)
        {
            Logger.LogDebug(message: "Initialized {Screen}", GetType().Name);
        }
    }

    /// <inheritdoc/>
    public async ValueTask DeactivateAsync(bool close, CancellationToken cancellationToken = default)
    {
        // Guard against multiple simultaneous executions.
        await AsyncHelper.AwaitCompletionAsync(_deactivateOperation, cancellationToken);

        using var operation = AsyncHelper.CreateAsyncOperation(cancellationToken)
                                         .CancelWhenActivating(this)
                                         .Assign(out _deactivateOperation);

        if (!IsInitialized)
        {
            // We do not allow deactivation before initialization.
            await AsyncHelper.AwaitCompletionAsync(_initializationOperation, cancellationToken);
        }

        if (operation.IsCancellationRequested)
        {
            Logger.LogTrace(message: "Deactivation of {Screen} canceled", GetType().Name);

            return;
        }

        if (IsActive || (IsInitialized && close))
        {
            if (close)
            {
                Logger.LogDebug(message: "Closing {Screen}...", GetType().Name);
            }
            else
            {
                Logger.LogTrace(message: "Deactivating {Screen}...", GetType().Name);
            }

            await RaiseDeactivatingAsync(close, operation.Token);
            await OnDeactivateAsync(close, operation.Token);

            IsActive = false;

            await RaiseDeactivatedAsync(close, operation.Token);

            if (close)
            {
                Logger.LogDebug(message: "Closed {Screen}", GetType().Name);
                IsInitialized = false;
            }
            else
            {
                Logger.LogTrace(message: "Deactivated {Screen}", GetType().Name);
            }
        }
    }

    /// <summary>Called when activating.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected virtual ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Called when deactivating.</summary>
    /// <param name="close">Indicates whether this instance will be closed.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected virtual ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>Called when initializing.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    protected virtual ValueTask OnInitializeAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    private ValueTask RaiseActivatedAsync(bool wasInitialized, CancellationToken cancellationToken)
    {
        return Activated?.InvokeAllAsync(this, new ActivationEventArgs(wasInitialized), cancellationToken) ??
               ValueTask.CompletedTask;
    }

    private ValueTask RaiseActivatingAsync(bool willInitialize, CancellationToken cancellationToken)
    {
        return Activating?.InvokeAllAsync(this, new ActivatingEventArgs(willInitialize), cancellationToken) ??
               ValueTask.CompletedTask;
    }

    private ValueTask RaiseDeactivatedAsync(bool wasClosed, CancellationToken cancellationToken)
    {
        return Deactivated?.InvokeAllAsync(this, new DeactivationEventArgs(wasClosed), cancellationToken) ??
               ValueTask.CompletedTask;
    }

    private ValueTask RaiseDeactivatingAsync(bool willClose, CancellationToken cancellationToken)
    {
        return Deactivating?.InvokeAllAsync(this, new DeactivatingEventArgs(willClose), cancellationToken) ??
               ValueTask.CompletedTask;
    }
}