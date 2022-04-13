namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using JetBrains.Annotations;

    /// <summary>A base implementation of <see cref="IScreen" />.</summary>
    [PublicAPI]
    public abstract class Screen : ObservableObject, IScreen, IChild
    {
        #region Constants and Fields

        private string displayName;
        private bool isActive;
        private bool isInitialized;
        private object? parent;

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="Screen" /> class.</summary>
        protected Screen()
        {
            this.displayName = GetType().Name;
        }

        #endregion

        #region Public Properties

        /// <summary>Indicates whether or not this instance is currently initialized.</summary>
        public bool IsInitialized
        {
            get => this.isInitialized;
            private set => SetProperty(ref this.isInitialized, value);
        }

        #endregion

        #region IActivate Implementation

        /// <inheritdoc />
        public event AsyncEventHandler<ActivationEventArgs>? Activated;

        /// <inheritdoc />
        public bool IsActive
        {
            get => this.isActive;
            private set => SetProperty(ref this.isActive, value);
        }

        /// <inheritdoc />
        async Task IActivate.ActivateAsync(CancellationToken cancellationToken)
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
                await OnInitializeAsync(cancellationToken);
                IsInitialized = initialized = true;
            }

            await OnActivateAsync(cancellationToken);

            IsActive = true;

            await RaiseActivatedAsync(initialized, cancellationToken);
        }

        #endregion

        #region IChild Implementation

        /// <inheritdoc />
        public object? Parent
        {
            get => this.parent;
            set => SetProperty(ref this.parent, value);
        }

        #endregion

        #region IClose Implementation

        /// <inheritdoc />
        public virtual async Task TryCloseAsync(bool? dialogResult = null,
                                                CancellationToken cancellationToken = default)
        {
            if (Parent is IConductor conductor)
            {
                await conductor.CloseItemAsync(this, cancellationToken);
            }
        }

        #endregion

        #region IDeactivate Implementation

        /// <inheritdoc />
        public event AsyncEventHandler<DeactivationEventArgs>? Deactivated;

        /// <inheritdoc />
        public event AsyncEventHandler<DeactivationEventArgs>? Deactivating;

        /// <inheritdoc />
        async Task IDeactivate.DeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (IsActive || (IsInitialized && close))
            {
                await RaiseDeactivatingAsync(close, cancellationToken);
                await OnDeactivateAsync(close, cancellationToken);

                IsActive = false;

                await RaiseDeactivatedAsync(close, cancellationToken);
            }
        }

        #endregion

        #region IGuardClose Implementation

        /// <inheritdoc />
        public virtual Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        #endregion

        #region IHaveDisplayName Implementation

        /// <inheritdoc />
        public string DisplayName
        {
            get => this.displayName;
            set => SetProperty(ref this.displayName, value);
        }

        #endregion

        #region Protected Methods

        /// <summary>Called when activating.</summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>Called when deactivating.</summary>
        /// <param name="close">Indicates whether this instance will be closed.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>Called when initializing.</summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private async Task RaiseActivatedAsync(bool wasInitialized, CancellationToken cancellationToken)
        {
            await (Activated?.InvokeAllAsync(this, new ActivationEventArgs(wasInitialized), cancellationToken) ??
                   Task.CompletedTask);
        }

        private async Task RaiseDeactivatedAsync(bool wasClosed, CancellationToken cancellationToken)
        {
            await (Deactivated?.InvokeAllAsync(this, new DeactivationEventArgs(wasClosed), cancellationToken) ??
                   Task.CompletedTask);
        }

        private async Task RaiseDeactivatingAsync(bool wasClosed, CancellationToken cancellationToken)
        {
            await (Deactivating?.InvokeAllAsync(this, new DeactivationEventArgs(wasClosed), cancellationToken) ??
                   Task.CompletedTask);
        }

        #endregion
    }
}