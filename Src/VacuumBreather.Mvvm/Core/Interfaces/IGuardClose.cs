using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an instance which may prevent closing.</summary>
[PublicAPI]
public interface IGuardClose : IClose
{
    /// <summary>Called to check whether or not this instance can be closed.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>
    ///     A <see cref="ValueTask"/> representing the asynchronous operation. The <see cref="ValueTask"/> result contains
    ///     a value indicating whether the instance can be closed.
    /// </returns>
    ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default);
}