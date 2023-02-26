using System;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Provides a helper method to automatically handle the setting of a status on a
///     <see cref="TaskCompletionSource" /> after a provided guard goes out of scope.
/// </summary>
public static class TaskCompletion
{
    /// <summary>
    ///     Creates a guarded <see cref="TaskCompletionSource" />.
    /// </summary>
    /// <param name="completionSource">
    ///     A reference to the <see cref="TaskCompletionSource" /> to initialize and handle.
    ///     Intended for use in a using block or statement.
    /// </param>
    /// <returns>
    ///     An <see cref="IDisposable" /> which will set the result of the
    ///     <see cref="TaskCompletionSource" /> when disposed.
    /// </returns>
    /// <example>
    ///     <code>
    ///         using var _ = TaskCompletion.CreateGuard(out var completionSource);
    ///     </code>
    /// </example>
    /// <seealso cref="System.IDisposable" />
    public static IDisposable CreateGuard(out TaskCompletionSource completionSource)
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        completionSource = source;

        return new DisposableAction(() => source.TrySetResult());
    }
}