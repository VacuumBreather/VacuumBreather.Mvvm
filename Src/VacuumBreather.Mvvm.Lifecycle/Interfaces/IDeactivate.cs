// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Denotes an instance which requires deactivation.</summary>
public interface IDeactivate
{
    /// <summary>Raised after deactivation.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "We need an asynchronous event handler.")]
    event AsyncEventHandler<DeactivationEventArgs>? Deactivated;

    /// <summary>Raised before deactivation.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "We need an asynchronous event handler.")]
    event AsyncEventHandler<DeactivationEventArgs>? Deactivating;

    /// <summary>Deactivates this instance.</summary>
    /// <param name="close">Indicates whether or not this instance is being closed.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeactivateAsync(bool close, CancellationToken cancellationToken = default);
}