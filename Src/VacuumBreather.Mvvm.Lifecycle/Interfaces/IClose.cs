namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Denotes an object that can be closed.</summary>
    public interface IClose
    {
        /// <summary>
        ///     Tries to close this instance. Also provides an opportunity to pass a dialog result to it's
        ///     corresponding view.
        /// </summary>
        /// <param name="dialogResult">The dialog result.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        Task TryCloseAsync(bool? dialogResult = null, CancellationToken cancellationToken = default);
    }
}