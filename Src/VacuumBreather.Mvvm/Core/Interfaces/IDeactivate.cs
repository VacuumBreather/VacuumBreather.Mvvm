using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an instance which requires deactivation.</summary>
[PublicAPI]
public interface IDeactivate
{
    /// <summary>Raised after deactivation.</summary>
    [SuppressMessage("Design",
                     "CA1003:Use generic event handler instances",
                     Justification =
                         "This is a special async type of event handler delegate and appropriate in this case.")]
    event AsyncEventHandler<DeactivationEventArgs>? Deactivated;

    /// <summary>Raised before deactivation.</summary>
    [SuppressMessage("Design",
                     "CA1003:Use generic event handler instances",
                     Justification =
                         "This is a special async type of event handler delegate and appropriate in this case.")]
    event AsyncEventHandler<DeactivatingEventArgs>? Deactivating;

    /// <summary>Deactivates this instance.</summary>
    /// <param name="close">Indicates whether or not this instance is being closed.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    ValueTask DeactivateAsync(bool close, CancellationToken cancellationToken = default);
}