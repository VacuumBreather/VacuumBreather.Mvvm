// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Provides extension methods for the <see cref="IConductor" /> type.</summary>
public static class ConductorExtensions
{
    /// <summary>Closes the specified item.</summary>
    /// <param name="conductor">The conductor.</param>
    /// <param name="item">The item to close.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task CloseItemAsync(
        this IConductor conductor,
        object item,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(item, nameof(item));

        return conductor.DeactivateItemAsync(item, true, cancellationToken);
    }

    /// <summary>Closes the specified item.</summary>
    /// <param name="conductor">The conductor.</param>
    /// <param name="item">The item to close.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel the operation.</param>
    /// <typeparam name="T">The type of the conducted item.</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static Task CloseItemAsync<T>(
        this IConductor<T> conductor,
        T item,
        CancellationToken cancellationToken = default)
        where T : class
    {
        Guard.IsNotNull(item, nameof(item));

        return conductor.DeactivateItemAsync(item, true, cancellationToken);
    }

    /// <summary>Deactivates the specified item.</summary>
    /// <param name="conductor">The conductor to deactivate the item with.</param>
    /// <param name="item">The item to deactivate.</param>
    /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
    /// <param name="closeItemAsync">The function to close the item with if necessary.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <typeparam name="T">The type of the conducted item.</typeparam>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task DeactivateItemAsync<T>(
        this IConductor<T> conductor,
        T item,
        bool close,
        Func<T, CancellationToken, Task> closeItemAsync,
        CancellationToken cancellationToken = default)
        where T : class
    {
        Guard.IsNotNull(item, nameof(item));
        Guard.IsNotNull(closeItemAsync, nameof(closeItemAsync));

        if (close)
        {
            var closeResult = await conductor.CloseStrategy.ExecuteAsync(
                                  new[]
                                      {
                                              item,
                                      },
                                  CancellationToken.None).ConfigureAwait(false);

            if (closeResult.CloseCanOccur)
            {
                await closeItemAsync(item, cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            await ScreenExtensions.TryDeactivateAsync(item, false, cancellationToken).ConfigureAwait(false);
        }
    }
}