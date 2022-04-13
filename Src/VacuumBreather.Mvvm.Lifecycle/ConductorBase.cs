namespace VacuumBreather.Mvvm.Lifecycle
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>A base class for various implementations of <see cref="IConductor" />.</summary>
    /// <typeparam name="T">The type that is being conducted.</typeparam>
    [PublicAPI]
    public abstract class ConductorBase<T> : Screen, IConductor<T>, IParent<T>
        where T : class
    {
        #region IConductor Implementation

        /// <inheritdoc />
        public event EventHandler<ActivationProcessedEventArgs>? ActivationProcessed;

        /// <inheritdoc />
        Task IConductor.ActivateItemAsync(object item, CancellationToken cancellationToken)
        {
            return ActivateItemAsync((T)item, cancellationToken);
        }

        /// <inheritdoc />
        Task IConductor.DeactivateItemAsync(object item, bool close, CancellationToken cancellationToken)
        {
            return DeactivateItemAsync((T)item, close, cancellationToken);
        }

        #endregion

        #region IConductor<T> Implementation

        /// <summary>Gets or sets the close strategy.</summary>
        /// <value>The close strategy.</value>
        public ICloseStrategy<T> CloseStrategy { get; set; } = new DefaultCloseStrategy<T>();

        /// <inheritdoc />
        public abstract Task ActivateItemAsync(T item, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public abstract Task DeactivateItemAsync(T item, bool close, CancellationToken cancellationToken = default);

        #endregion

        #region IParent Implementation

        /// <inheritdoc />
        IEnumerable IParent.GetChildren()
        {
            return GetChildren();
        }

        #endregion

        #region IParent<T> Implementation

        /// <inheritdoc />
        public abstract IEnumerable<T> GetChildren();

        #endregion

        #region Protected Methods

        /// <summary>Ensures that an item is ready to be activated.</summary>
        /// <param name="newItem">The item that is about to be activated.</param>
        /// <returns>The item to be activated.</returns>
        protected virtual T? EnsureItem(T? newItem)
        {
            if (newItem is IChild child && (child.Parent != this))
            {
                child.Parent = this;
            }

            return newItem;
        }

        /// <summary>Called by a subclass when an activation needs processing.</summary>
        /// <param name="item">The item on which activation was attempted.</param>
        /// <param name="success">If set to <c>true</c> the activation was successful.</param>
        protected virtual void OnActivationProcessed(T item, bool success)
        {
            ActivationProcessed?.Invoke(this, new ActivationProcessedEventArgs(item, success));
        }

        #endregion
    }
}