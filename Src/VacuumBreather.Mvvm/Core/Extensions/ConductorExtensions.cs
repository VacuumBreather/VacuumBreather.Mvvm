using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="IConductor" /> type.</summary>
public static class ConductorExtensions
{
    /// <summary>Closes the specified item.</summary>
    /// <param name="conductor">The conductor.</param>
    /// <param name="item">The item to close.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    public static ValueTask CloseItemAsync(this IConductor conductor,
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
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    public static ValueTask CloseItemAsync<T>(this IConductor<T> conductor,
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
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    public static async ValueTask DeactivateItemAsync<T>(this IConductor<T> conductor,
                                                         T item,
                                                         bool close,
                                                         Func<T, CancellationToken, ValueTask> closeItemAsync,
                                                         CancellationToken cancellationToken = default)
        where T : class
    {
        Guard.IsNotNull(item, nameof(item));
        Guard.IsNotNull(closeItemAsync, nameof(closeItemAsync));

        if (close)
        {
            ICloseResult<T> closeResult = await conductor.CloseStrategy
                                                         .ExecuteAsync(new[] { item }, CancellationToken.None)
                                                         .ConfigureAwait(true);

            if (closeResult.CloseCanOccur)
            {
                await closeItemAsync(item, cancellationToken).ConfigureAwait(true);
            }
        }
        else
        {
            await ScreenExtensions.TryDeactivateAsync(item, false, cancellationToken).ConfigureAwait(true);
        }
    }
}