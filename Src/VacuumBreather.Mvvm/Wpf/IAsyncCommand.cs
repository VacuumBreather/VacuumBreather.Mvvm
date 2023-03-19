using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Defines a command which executes asynchronously.</summary>
public interface IAsyncCommand : IRaisingCommand, ICancellable
{
    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    bool CanExecute();

    /// <summary>Defines the asynchronous method to be called when the command is invoked.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous save operation.</returns>
    ValueTask ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>Defines a command which executes asynchronously.</summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public interface IAsyncCommand<in T> : IRaisingCommand, ICancellable
{
    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <param name="parameter">
    ///     Data used by the command. If the command does not require data to be passed, this object can be
    ///     set to <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if this command can be executed; otherwise, <see langword="false"/>.</returns>
    bool CanExecute(T parameter);

    /// <summary>Defines the asynchronous method to be called when the command is invoked.</summary>
    /// <param name="parameter">
    ///     Data used by the command. If the command does not require data to be passed, this object can be
    ///     set to <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous save operation.</returns>
    ValueTask ExecuteAsync(T parameter, CancellationToken cancellationToken = default);
}