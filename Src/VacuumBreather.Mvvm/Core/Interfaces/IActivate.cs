using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an instance which requires activation.</summary>
[PublicAPI]
public interface IActivate
{
    /// <summary>Raised after activation occurs.</summary>
    [SuppressMessage(category: "Design",
                     checkId: "CA1003:Use generic event handler instances",
                     Justification = "We need an asynchronous event handler.")]
    [SuppressMessage(category: "Meziantou.Analyzer",
                     checkId: "MA0046:Use EventHandler<T> to declare events",
                     Justification =
                         "This is a special async type of event handler delegate and appropriate in this case.")]
    event AsyncEventHandler<ActivationEventArgs>? Activated;

    /// <summary>Raised before activation.</summary>
    [SuppressMessage(category: "Design",
                     checkId: "CA1003:Use generic event handler instances",
                     Justification =
                         "This is a special async type of event handler delegate and appropriate in this case.")]
    [SuppressMessage(category: "Meziantou.Analyzer",
                     checkId: "MA0046:Use EventHandler<T> to declare events",
                     Justification =
                         "This is a special async type of event handler delegate and appropriate in this case.")]
    event AsyncEventHandler<ActivatingEventArgs>? Activating;

    /// <summary>Gets a value indicating whether this instance is active.</summary>
    bool IsActive { get; }

    /// <summary>Activates this instance.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask ActivateAsync(CancellationToken cancellationToken = default);
}