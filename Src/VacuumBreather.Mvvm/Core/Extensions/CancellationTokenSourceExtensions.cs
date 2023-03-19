using System;
using System.Threading;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="CancellationTokenSource"/> type.</summary>
[PublicAPI]
public static class CancellationTokenSourceExtensions
{
    /// <summary>Tries to communicate a request for cancellation.</summary>
    /// <remarks>
    ///     <para>
    ///         The associated <see cref="CancellationToken"/> will be notified of the cancellation and will transition to a
    ///         state where <see cref="CancellationToken.IsCancellationRequested"/> returns true. Any callbacks or cancelable
    ///         operations registered with the <see cref="CancellationToken"/>  will be executed.
    ///     </para>
    ///     <para>
    ///         Cancelable operations and callbacks registered with the token should not throw exceptions. If
    ///         <paramref name="throwOnFirstException"/> is true, an exception will immediately propagate out of the call to
    ///         Cancel, preventing the remaining callbacks and cancelable operations from being processed. If
    ///         <paramref name="throwOnFirstException"/> is false, this overload will aggregate any exceptions thrown into a
    ///         <see cref="AggregateException"/>, such that one callback throwing an exception will not prevent other
    ///         registered callbacks from being executed.
    ///     </para>
    ///     <para>
    ///         The <see cref="ExecutionContext"/> that was captured when each callback was registered will be reestablished
    ///         when the callback is invoked.
    ///     </para>
    /// </remarks>
    /// <param name="source">The <see cref="CancellationTokenSource"/> to try and cancel.</param>
    /// <param name="throwOnFirstException">Specifies whether exceptions should immediately propagate.</param>
    /// <returns>
    ///     <see langword="true"/> If the <see cref="CancellationTokenSource"/> was cancelled successfully; otherwise,
    ///     <see langword="false"/>.
    /// </returns>
    /// <exception cref="AggregateException">
    ///     An aggregate exception containing all the exceptions thrown by the registered
    ///     callbacks on the associated <see cref="CancellationToken"/>.
    /// </exception>
    public static bool TryCancel(this CancellationTokenSource source, bool throwOnFirstException = false)
    {
        try
        {
            source.Cancel(throwOnFirstException);

            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }
}