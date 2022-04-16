// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>A base implementation of <see cref="IScreen" />.</summary>
public abstract class Screen : ObservableObject, IScreen, IChild
{
    private string displayName;
    private bool isActive;
    private bool isInitialized;
    private object? parent;

    /// <summary>Initializes a new instance of the <see cref="Screen" /> class.</summary>
    protected Screen()
    {
        this.displayName = GetType().Name;
    }

    /// <inheritdoc />
    public event AsyncEventHandler<ActivationEventArgs>? Activated;

    /// <inheritdoc />
    public event AsyncEventHandler<DeactivationEventArgs>? Deactivated;

    /// <inheritdoc />
    public event AsyncEventHandler<DeactivationEventArgs>? Deactivating;

    /// <inheritdoc />
    public string DisplayName
    {
        get => this.displayName;
        set => SetProperty(ref this.displayName, value);
    }

    /// <inheritdoc />
    public bool IsActive
    {
        get => this.isActive;
        private set => SetProperty(ref this.isActive, value);
    }

    /// <summary>Gets a value indicating whether this instance has been initialized.</summary>
    public bool IsInitialized
    {
        get => this.isInitialized;
        private set => SetProperty(ref this.isInitialized, value);
    }

    /// <inheritdoc />
    public object? Parent
    {
        get => this.parent;
        set => SetProperty(ref this.parent, value);
    }

    /// <inheritdoc />
    public async Task ActivateAsync(CancellationToken cancellationToken)
    {
        if (IsActive || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var initialized = false;

        if (!IsInitialized)
        {
            // Deactivation is not allowed to cancel initialization, so we are only
            // passing the token that was passed to us.
            await OnInitializeAsync(cancellationToken).ConfigureAwait(false);
            IsInitialized = initialized = true;
        }

        await OnActivateAsync(cancellationToken).ConfigureAwait(false);

        IsActive = true;

        await RaiseActivatedAsync(initialized, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task TryCloseAsync(
        bool? dialogResult = null,
        CancellationToken cancellationToken = default)
    {
        if (Parent is IConductor conductor)
        {
            await conductor.CloseItemAsync(this, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task DeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (IsActive || (IsInitialized && close))
        {
            await RaiseDeactivatingAsync(close, cancellationToken).ConfigureAwait(false);
            await OnDeactivateAsync(close, cancellationToken).ConfigureAwait(false);

            IsActive = false;

            await RaiseDeactivatedAsync(close, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public virtual Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    /// <summary>Called when activating.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected virtual Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>Called when deactivating.</summary>
    /// <param name="close">Indicates whether this instance will be closed.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected virtual Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>Called when initializing.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task RaiseActivatedAsync(bool wasInitialized, CancellationToken cancellationToken)
    {
        await (Activated?.InvokeAllAsync(this, new ActivationEventArgs(wasInitialized), cancellationToken) ??
               Task.CompletedTask).ConfigureAwait(false);
    }

    private async Task RaiseDeactivatedAsync(bool wasClosed, CancellationToken cancellationToken)
    {
        await (Deactivated?.InvokeAllAsync(this, new DeactivationEventArgs(wasClosed), cancellationToken) ??
               Task.CompletedTask).ConfigureAwait(false);
    }

    private async Task RaiseDeactivatingAsync(bool wasClosed, CancellationToken cancellationToken)
    {
        await (Deactivating?.InvokeAllAsync(this, new DeactivationEventArgs(wasClosed), cancellationToken) ??
               Task.CompletedTask).ConfigureAwait(false);
    }
}