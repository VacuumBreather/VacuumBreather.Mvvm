// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     A base class for various implementations of <see cref="IConductor" /> that maintain an
    ///     active item.
    /// </summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IConductActiveItem<T>
        where T : class
    {
        private T? activeItem;

        /// <summary>Gets the currently active item.</summary>
        public T? ActiveItem
        {
            get => this.activeItem;
            set => ActivateItemAsync(value, CancellationToken.None).Forget();
        }

        /// <inheritdoc />
        object? IHaveActiveItem.ActiveItem
        {
            get => ActiveItem;
            set => ActiveItem = (T?)value;
        }

        /// <summary>Changes the active item.</summary>
        /// <param name="newItem">The new item to activate.</param>
        /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
        protected virtual async ValueTask ChangeActiveItemAsync(
            T? newItem,
            bool closePrevious,
            CancellationToken cancellationToken)
        {
            await ScreenExtensions.TryDeactivateAsync(ActiveItem, closePrevious, cancellationToken)
                .ConfigureAwait(false);

            newItem = EnsureItem(newItem);

            SetProperty(ref this.activeItem, newItem, nameof(ActiveItem));

            if (IsActive)
            {
                await ScreenExtensions.TryActivateAsync(newItem, cancellationToken).ConfigureAwait(false);
            }

            if (newItem is not null)
            {
                OnActivationProcessed(newItem, true);
            }
        }
    }
}