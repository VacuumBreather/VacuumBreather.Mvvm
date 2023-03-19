using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an object that can be closed.</summary>
[PublicAPI]
public interface IClose
{
    /// <summary>Tries to close this instance.</summary>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask TryCloseAsync(CancellationToken cancellationToken = default);
}