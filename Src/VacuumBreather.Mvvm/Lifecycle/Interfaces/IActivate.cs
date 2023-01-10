// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Denotes an instance which requires activation.</summary>
    public interface IActivate
    {
        /// <summary>Gets a value indicating whether this instance is active.</summary>
        bool IsActive { get; }

        /// <summary>Raised after activation occurs.</summary>
        [SuppressMessage(
            "Design", "CA1003:Use generic event handler instances",
            Justification = "We need an asynchronous event handler.")]
        event AsyncEventHandler<ActivationEventArgs>? Activated;

        /// <summary>Activates this instance.</summary>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
        ValueTask ActivateAsync(CancellationToken cancellationToken = default);
    }
}