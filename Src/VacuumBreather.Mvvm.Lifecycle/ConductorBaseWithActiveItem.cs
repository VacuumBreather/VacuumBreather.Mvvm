namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>
    ///     A base class for various implementations of <see cref="IConductor" /> that maintain an
    ///     active item.
    /// </summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    [PublicAPI]
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IConductActiveItem<T>
        where T : class
    {
        #region Constants and Fields

        private T? activeItem;

        #endregion

        #region Public Properties

        /// <summary>The currently active item.</summary>
        public T? ActiveItem
        {
            get => this.activeItem;
            private set => SetProperty(ref this.activeItem, value);
        }

        #endregion

        #region IHaveActiveItem Implementation

        /// <inheritdoc />
        object? IHaveActiveItem.ActiveItem => ActiveItem;

        #endregion

        #region Protected Methods

        /// <summary>Changes the active item.</summary>
        /// <param name="newItem">The new item to activate.</param>
        /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected virtual async Task ChangeActiveItemAsync(T? newItem,
                                                           bool closePrevious,
                                                           CancellationToken cancellationToken)
        {
            await ScreenExtensions.TryDeactivateAsync(ActiveItem, closePrevious, cancellationToken);

            newItem = EnsureItem(newItem);

            ActiveItem = newItem;

            if (IsActive)
            {
                await ScreenExtensions.TryActivateAsync(newItem, cancellationToken);
            }

            if (newItem is not null)
            {
                OnActivationProcessed(newItem, true);
            }
        }

        #endregion
    }
}