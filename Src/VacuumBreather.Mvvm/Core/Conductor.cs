﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     An implementation of <see cref="IConductor" /> that holds on to and activates only one
///     item at a time.
/// </summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
public class Conductor<T> : ConductorBaseWithActiveItem<T>
    where T : class
{
    /// <inheritdoc />
    public override async ValueTask ActivateItemAsync(T? item, CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(item);

        if (item.Equals(ActiveItem))
        {
            if (IsActive)
            {
                await ScreenExtensions.TryActivateAsync(item, cancellationToken).ConfigureAwait(true);
                OnActivationProcessed(item, true);
            }

            return;
        }

        bool closeCanOccur = ActiveItem is null ||
                             (await CloseStrategy.ExecuteAsync(new[] { ActiveItem }, cancellationToken)
                                                 .ConfigureAwait(true)).CloseCanOccur;

        if (closeCanOccur)
        {
            await ChangeActiveItemAsync(item, true, cancellationToken).ConfigureAwait(true);
        }
        else
        {
            OnActivationProcessed(item, false);
        }
    }

    /// <summary>Called to check whether or not this instance can close.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    public override async ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        if (ActiveItem is null)
        {
            return true;
        }

        ICloseResult<T> closeResult = await CloseStrategy.ExecuteAsync(new[] { ActiveItem }, cancellationToken)
                                                         .ConfigureAwait(true);

        return closeResult.CloseCanOccur;
    }

    /// <summary>Deactivates the specified item.</summary>
    /// <param name="item">The item to close.</param>
    /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    public override async ValueTask DeactivateItemAsync(T item,
                                                        bool close,
                                                        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(item);

        if (!item.Equals(ActiveItem))
        {
            return;
        }

        bool closeCanOccur = ActiveItem is null ||
                             (await CloseStrategy.ExecuteAsync(new[] { ActiveItem }, cancellationToken)
                                                 .ConfigureAwait(true)).CloseCanOccur;

        if (closeCanOccur)
        {
            await ChangeActiveItemAsync(default, close, cancellationToken).ConfigureAwait(true);
        }
    }

    /// <summary>Gets the children.</summary>
    /// <returns>The collection of children.</returns>
    public override IEnumerable<T> GetChildren()
    {
        return ActiveItem is null ? Enumerable.Empty<T>() : new[] { ActiveItem };
    }

    /// <summary>Called when activating.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    protected override ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        return ScreenExtensions.TryActivateAsync(ActiveItem, cancellationToken);
    }

    /// <summary>Called when deactivating.</summary>
    /// <param name="close">Indicates whether this instance will be closed.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    protected override ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        return ScreenExtensions.TryDeactivateAsync(ActiveItem, close, cancellationToken);
    }
}