// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Denotes an instance which may prevent closing.</summary>
public interface IGuardClose : IClose
{
    /// <summary>Called to check whether or not this instance can be closed.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// The task result contains a value indicating whether the instance can be closed.
    /// </returns>
    Task<bool> CanCloseAsync(CancellationToken cancellationToken = default);
}