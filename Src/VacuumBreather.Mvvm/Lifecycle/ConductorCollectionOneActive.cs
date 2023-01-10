// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Contains implementations of <see cref="IConductor" /> that hold on many items.</summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public class ConductorCollectionOneActive<T> : ConductorBaseWithActiveItem<T>
        where T : class
    {
        private readonly BindableCollection<T> items = new();

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="Mvvm.Lifecycle.ConductorCollectionOneActive{T}" /> class.
        /// </summary>
        public ConductorCollectionOneActive()
        {
            this.items.AreChildrenOf(this);
        }

        /// <summary>Gets the items that are currently being conducted.</summary>
        public IBindableCollection<T> Items => this.items;

        /// <inheritdoc />
        public override async ValueTask ActivateItemAsync(T? item, CancellationToken cancellationToken = default)
        {
            if (item?.Equals(ActiveItem) ?? false)
            {
                if (IsActive)
                {
                    await ScreenExtensions.TryActivateAsync(item, cancellationToken).ConfigureAwait(false);
                    OnActivationProcessed(item, true);
                }

                return;
            }

            await ChangeActiveItemAsync(item, false, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            ICloseResult<T> closeResult = await CloseStrategy.ExecuteAsync(this.items.ToList(), cancellationToken)
                .ConfigureAwait(false);

            if (closeResult.CloseCanOccur || !closeResult.Children.Any())
            {
                return closeResult.CloseCanOccur;
            }

            List<T> closable = closeResult.Children.ToList();

            if (ActiveItem is not null && closable.Contains(ActiveItem))
            {
                List<T> list = this.items.ToList();
                T? next = ActiveItem;

                do
                {
                    T? previous = next;
                    next = DetermineNextItemToActivate(list, list.IndexOf(previous));
                    list.Remove(previous);
                } while (next is not null && closable.Contains(next));

                T? previousActive = ActiveItem;
                await ChangeActiveItemAsync(next, true, cancellationToken).ConfigureAwait(false);
                this.items.Remove(previousActive);
                closable.Remove(previousActive);
            }

            foreach (IDeactivate deactivate in closable.OfType<IDeactivate>())
            {
                await deactivate.DeactivateAsync(true, cancellationToken).ConfigureAwait(false);
            }

            this.items.RemoveRange(closable);

            return closeResult.CloseCanOccur;
        }

        /// <inheritdoc />
        public override async ValueTask DeactivateItemAsync(
            T item,
            bool close,
            CancellationToken cancellationToken = default)
        {
            await this.DeactivateItemAsync(item, close, CloseItemCoreAsync, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public sealed override IEnumerable<T> GetChildren()
        {
            return this.items;
        }

        /// <summary>Determines the next item to activate based on the last active index.</summary>
        /// <param name="list">The list of possible active items.</param>
        /// <param name="lastIndex">The index of the last active item.</param>
        /// <returns>The next item to activate.</returns>
        /// <remarks>
        ///     <para>Called after an active item is closed.</para>
        /// </remarks>
        protected virtual T? DetermineNextItemToActivate(IList<T> list, int lastIndex)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            int toRemoveAt = lastIndex - 1;

            if (toRemoveAt == -1 && list.Count > 1)
            {
                return list[1];
            }

            if (toRemoveAt > -1 && toRemoveAt < list.Count - 1)
            {
                return list[toRemoveAt];
            }

            return default;
        }

        /// <inheritdoc />
        protected override T? EnsureItem(T? newItem)
        {
            if (newItem == null)
            {
                newItem = DetermineNextItemToActivate(
                    this.items,
                    ActiveItem != null ? this.items.IndexOf(ActiveItem) : 0);
            }
            else
            {
                int index = this.items.IndexOf(newItem);

                if (index == -1)
                {
                    this.items.Add(newItem);
                }
                else
                {
                    newItem = this.items[index];
                }
            }

            return base.EnsureItem(newItem);
        }

        /// <inheritdoc />
        protected override ValueTask OnActivateAsync(CancellationToken cancellationToken)
        {
            return ScreenExtensions.TryActivateAsync(ActiveItem, cancellationToken);
        }

        /// <inheritdoc />
        protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                foreach (IDeactivate deactivate in this.items.OfType<IDeactivate>())
                {
                    await deactivate.DeactivateAsync(true, cancellationToken).ConfigureAwait(false);
                }

                this.items.Clear();
            }
            else
            {
                await ScreenExtensions.TryDeactivateAsync(ActiveItem, false, cancellationToken).ConfigureAwait(false);
            }
        }

        private async ValueTask CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
        {
            if (item.Equals(ActiveItem))
            {
                int index = this.items.IndexOf(item);
                T? next = DetermineNextItemToActivate(this.items, index);

                await ChangeActiveItemAsync(next, true, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await ScreenExtensions.TryDeactivateAsync(item, true, cancellationToken).ConfigureAwait(false);
            }

            this.items.Remove(item);
        }
    }
}