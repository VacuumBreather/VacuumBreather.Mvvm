namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Diagnostics;
    using JetBrains.Annotations;

    /// <summary>
    ///     An implementation of <see cref="IConductor" /> that holds on to and activates only one
    ///     item at a time.
    /// </summary>
    [PublicAPI]
    public partial class Conductor<T> : ConductorBaseWithActiveItem<T>
        where T : class
    {
        #region Public Methods

        /// <inheritdoc />
        public override async Task ActivateItemAsync(T item, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(item, nameof(item));

            if (item.Equals(ActiveItem))
            {
                if (IsActive)
                {
                    await ScreenExtensions.TryActivateAsync(item, cancellationToken);
                    OnActivationProcessed(item, true);
                }

                return;
            }

            var closeCanOccur = ActiveItem is null || (await CloseStrategy.ExecuteAsync(
                                                           new[]
                                                               {
                                                                   ActiveItem
                                                               },
                                                           cancellationToken)).CloseCanOccur;

            if (closeCanOccur)
            {
                await ChangeActiveItemAsync(item, true, cancellationToken);
            }
            else
            {
                OnActivationProcessed(item, false);
            }
        }

        /// <summary>Called to check whether or not this instance can close.</summary>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            if (ActiveItem is null)
            {
                return true;
            }

            var closeResult = await CloseStrategy.ExecuteAsync(
                                  new[]
                                      {
                                          ActiveItem
                                      },
                                  cancellationToken);

            return closeResult.CloseCanOccur;
        }

        /// <summary>Deactivates the specified item.</summary>
        /// <param name="item">The item to close.</param>
        /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public override async Task DeactivateItemAsync(T item,
                                                       bool close,
                                                       CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(item, nameof(item));

            if (!item.Equals(ActiveItem))
            {
                return;
            }

            var closeCanOccur = ActiveItem is null || (await CloseStrategy.ExecuteAsync(
                                                           new[]
                                                               {
                                                                   ActiveItem
                                                               },
                                                           cancellationToken)).CloseCanOccur;

            if (closeCanOccur)
            {
                await ChangeActiveItemAsync(default, close, cancellationToken);
            }
        }

        /// <summary>Gets the children.</summary>
        /// <returns>The collection of children.</returns>
        public override IEnumerable<T> GetChildren()
        {
            return ActiveItem is null
                       ? Enumerable.Empty<T>()
                       : new[]
                             {
                                 ActiveItem
                             };
        }

        #endregion

        #region Protected Methods

        /// <summary>Called when activating.</summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return ScreenExtensions.TryActivateAsync(ActiveItem, cancellationToken);
        }

        /// <summary>Called when deactivating.</summary>
        /// <param name="close">Indicates whether this instance will be closed.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return ScreenExtensions.TryDeactivateAsync(ActiveItem, close, cancellationToken);
        }

        #endregion
    }
}