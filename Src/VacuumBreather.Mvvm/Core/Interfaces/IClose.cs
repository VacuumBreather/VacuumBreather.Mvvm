using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an object that can be closed.</summary>
[PublicAPI]
public interface IClose
{
    /// <summary>Tries to close this instance. Also provides an opportunity to pass a dialog result to it's corresponding view.</summary>
    /// <param name="dialogResult">The dialog result.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask TryCloseAsync(bool? dialogResult = null, CancellationToken cancellationToken = default);
}