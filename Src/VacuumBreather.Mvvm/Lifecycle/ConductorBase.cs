// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>A base class for various implementations of <see cref="IConductor" />.</summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    public abstract class ConductorBase<T> : Screen, IConductor<T>, IParent<T>
        where T : class
    {
        private ICloseStrategy<T> closeStrategy = new DefaultCloseStrategy<T>();

        /// <summary>Gets or sets the close strategy.</summary>
        /// <value>The close strategy.</value>
        public ICloseStrategy<T> CloseStrategy
        {
            get => this.closeStrategy;
            set => SetProperty(ref this.closeStrategy, value);
        }

        /// <inheritdoc />
        public event EventHandler<ActivationProcessedEventArgs>? ActivationProcessed;

        /// <inheritdoc />
        ValueTask IConductor.ActivateItemAsync(object? item, CancellationToken cancellationToken)
        {
            return ActivateItemAsync((T?)item, cancellationToken);
        }

        /// <inheritdoc />
        ValueTask IConductor.DeactivateItemAsync(object item, bool close, CancellationToken cancellationToken)
        {
            return DeactivateItemAsync((T)item, close, cancellationToken);
        }

        /// <inheritdoc />
        public abstract ValueTask ActivateItemAsync(T? item, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract ValueTask
            DeactivateItemAsync(T item, bool close, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        IEnumerable IParent.GetChildren()
        {
            return GetChildren();
        }

        /// <inheritdoc />
        public abstract IEnumerable<T> GetChildren();

        /// <summary>Ensures that an item is ready to be activated.</summary>
        /// <param name="newItem">The item that is about to be activated.</param>
        /// <returns>The item to be activated.</returns>
        protected virtual T? EnsureItem(T? newItem)
        {
            if (newItem is IChild child && child.Parent != this)
            {
                child.Parent = this;
            }

            return newItem;
        }

        /// <summary>Called by a subclass when an activation needs processing.</summary>
        /// <param name="item">The item on which activation was attempted.</param>
        /// <param name="success">If set to <see langword="true" /> the activation was successful.</param>
        protected virtual void OnActivationProcessed(T item, bool success)
        {
            ActivationProcessed?.Invoke(this, new ActivationProcessedEventArgs(item, success));
        }
    }
}