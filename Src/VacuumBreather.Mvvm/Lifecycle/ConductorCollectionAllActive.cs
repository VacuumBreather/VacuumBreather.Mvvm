// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Contains implementations of <see cref="IConductor" /> that hold on many items.</summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public class ConductorCollectionAllActive<T> : ConductorBase<T>
        where T : class
    {
        private readonly bool conductPublicItems;

        private readonly BindableCollection<T> items = new();

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="Mvvm.Lifecycle.ConductorCollectionAllActive{T}" /> class.
        /// </summary>
        /// <param name="conductPublicItems">
        ///     If set to <see langword="true" /> public items that are properties of this
        ///     class will be conducted.
        /// </param>
        public ConductorCollectionAllActive(bool conductPublicItems)
            : this()
        {
            this.conductPublicItems = conductPublicItems;
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="Mvvm.Lifecycle.ConductorCollectionAllActive{T}" /> class.
        /// </summary>
        public ConductorCollectionAllActive()
        {
            this.items.AreChildrenOf(this);
        }

        /// <summary>Gets the items that are currently being conducted.</summary>
        public IBindableCollection<T> Items => this.items;

        /// <inheritdoc />
        public override async ValueTask ActivateItemAsync(T? item, CancellationToken cancellationToken = default)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item = EnsureItem(item)!;

            if (IsActive)
            {
                await ScreenExtensions.TryActivateAsync(item, cancellationToken).ConfigureAwait(false);
            }

            OnActivationProcessed(item, true);
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

            foreach (IDeactivate deactivate in closeResult.Children.OfType<IDeactivate>())
            {
                await deactivate.DeactivateAsync(true, cancellationToken).ConfigureAwait(false);
            }

            this.items.RemoveRange(closeResult.Children);

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

        /// <inheritdoc />
        protected override T? EnsureItem(T? newItem)
        {
            if (newItem is null)
            {
                return base.EnsureItem(newItem);
            }

            int index = this.items.IndexOf(newItem);

            if (index == -1)
            {
                this.items.Add(newItem);
            }
            else
            {
                newItem = this.items[index];
            }

            return base.EnsureItem(newItem);
        }

        /// <inheritdoc />
        protected override async ValueTask OnActivateAsync(CancellationToken cancellationToken)
        {
            foreach (IActivate activate in this.items.OfType<IActivate>())
            {
                await activate.ActivateAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            foreach (IDeactivate deactivate in this.items.OfType<IDeactivate>())
            {
                await deactivate.DeactivateAsync(close, cancellationToken).ConfigureAwait(false);
            }

            if (close)
            {
                this.items.Clear();
            }
        }

        /// <inheritdoc />
        protected override async ValueTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (this.conductPublicItems)
            {
                List<T> publicItems = GetType()
                    .GetTypeInfo()
                    .DeclaredProperties
                    .Where(
                        propertyInfo =>
                            propertyInfo.Name != nameof(Parent) && typeof(T).GetTypeInfo()
                                .IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
                    .Select(propertyInfo => propertyInfo.GetValue(this, null))
                    .Cast<T>()
                    .ToList();

                await Task.WhenAll(publicItems.Select(item => ActivateItemAsync(item, cancellationToken))
                    .Select(t => t.AsTask())).ConfigureAwait(false);
            }
        }

        private async ValueTask CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
        {
            await ScreenExtensions.TryDeactivateAsync(item, true, cancellationToken).ConfigureAwait(false);

            this.items.Remove(item);
        }
    }
}