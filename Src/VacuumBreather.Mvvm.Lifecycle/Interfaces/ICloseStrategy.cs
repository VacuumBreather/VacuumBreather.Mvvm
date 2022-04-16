// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>
///     Used to gather the results from multiple child elements which may or may not prevent
///     closing.
/// </summary>
/// <typeparam name="T">The type of child element.</typeparam>
public interface ICloseStrategy<T>
{
    /// <summary>Executes the strategy.</summary>
    /// <param name="toClose">Items that are requesting close.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result contains information about the result of the closing strategy.</returns>
    Task<ICloseResult<T>> ExecuteAsync(IEnumerable<T> toClose, CancellationToken cancellationToken = default);
}