// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Denotes an instance which requires activation.</summary>
public interface IActivate
{
    /// <summary>Raised after activation occurs.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "We need an asynchronous event handler.")]
    event AsyncEventHandler<ActivationEventArgs>? Activated;

    /// <summary>Gets a value indicating whether this instance is active.</summary>
    bool IsActive { get; }

    /// <summary>Activates this instance.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ActivateAsync(CancellationToken cancellationToken = default);
}