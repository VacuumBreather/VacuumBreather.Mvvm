using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Denotes an instance which conducts other objects by managing an ActiveItem and maintaining
///     a strict lifecycle.
/// </summary>
/// <remarks>
///     <para>
///         Conducted instances can opt in to the lifecycle by implementing any of the following
///         <see cref="IActivate" />, <see cref="IDeactivate" />, <see cref="IGuardClose" />.
///     </para>
/// </remarks>
[PublicAPI]
public interface IConductor : IScreen, IParent
{
    /// <summary>Occurs when an activation request is processed.</summary>
    event EventHandler<ActivationProcessedEventArgs>? ActivationProcessed;

    /// <summary>Activates the specified item.</summary>
    /// <param name="item">The item to activate.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads
    ///     to receive notice of cancellation.
    /// </param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    ValueTask ActivateItemAsync(object item, CancellationToken cancellationToken = default);

    /// <summary>Deactivates the specified item.</summary>
    /// <param name="item">The item to deactivate.</param>
    /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    ValueTask DeactivateItemAsync(object item, bool close, CancellationToken cancellationToken = default);
}

/// <summary>
///     Denotes an instance which conducts other objects by managing an ActiveItem and maintaining
///     a strict lifecycle.
/// </summary>
/// <typeparam name="T">The type of item to conduct.</typeparam>
/// <remarks>
///     <para>
///         Conducted instances can opt in to the lifecycle by implementing any of the following
///         <see cref="IActivate" />, <see cref="IDeactivate" />, <see cref="IGuardClose" />.
///     </para>
/// </remarks>
[PublicAPI]
public interface IConductor<T> : IConductor
{
    /// <summary>Gets or sets the close strategy.</summary>
    /// <value>The close strategy.</value>
    ICloseStrategy<T> CloseStrategy { get; set; }

    /// <summary>Activates the specified item.</summary>
    /// <param name="item">The item to activate.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads
    ///     to receive notice of cancellation.
    /// </param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    ValueTask ActivateItemAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>Deactivates the specified item.</summary>
    /// <param name="item">The item to deactivate.</param>
    /// <param name="close">Indicates whether or not to close the item after deactivating it.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    ValueTask DeactivateItemAsync(T item, bool close, CancellationToken cancellationToken = default);
}