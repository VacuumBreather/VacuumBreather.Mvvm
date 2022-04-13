﻿namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    /// <summary>Denotes an instance which requires activation.</summary>
    [PublicAPI]
    public interface IActivate
    {
        /// <summary>Raised after activation occurs.</summary>
        event AsyncEventHandler<ActivationEventArgs>? Activated;

        /// <summary>Indicates whether or not this instance is active.</summary>
        bool IsActive { get; }

        /// <summary>Activates this instance.</summary>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ActivateAsync(CancellationToken cancellationToken = default);
    }
}