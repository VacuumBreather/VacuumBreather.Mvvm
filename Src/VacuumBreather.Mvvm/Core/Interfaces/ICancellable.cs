using System;
using System.Threading;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Interface for an object that represents an operation that can be canceled.</summary>
public interface ICancellable
{
    /// <summary>Gets a value indicating whether cancellation has been requested for this <see cref="ICancellable"/>.</summary>
    /// <value>Whether cancellation has been requested for this <see cref="ICancellable"/>.</value>
    /// <remarks>
    ///     <para>
    ///         This property indicates whether cancellation has been requested for this token source, such as due to a call
    ///         to its <see cref="Cancel"/> method.
    ///     </para>
    ///     <para>
    ///         If this property returns true, it only guarantees that cancellation has been requested. It does not guarantee
    ///         that every handler registered with the corresponding token has finished executing, nor that cancellation
    ///         requests have finished propagating to all registered handlers. Additional synchronization may be required,
    ///         particularly in situations where related objects are being canceled concurrently.
    ///     </para>
    /// </remarks>
    bool IsCancellationRequested { get; }

    /// <summary>Gets the <see cref="CancellationToken"/> associated with this <see cref="ICancellable"/>.</summary>
    CancellationToken Token { get; }

    /// <summary>Communicates a request for cancellation.</summary>
    /// <param name="useNewThread">
    ///     (Optional) If set to <see langword="true"/> cancellation will be invoked on another thread.
    ///     The default is <see langword="true"/>.
    /// </param>
    /// <exception cref="AggregateException">
    ///     An aggregate exception containing all the exceptions thrown by the registered
    ///     callbacks on the associated <see cref="CancellationToken"/>.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         The associated <see cref="CancellationToken"/> will be notified of the cancellation and will transition to a
    ///         state where <see cref="CancellationToken.IsCancellationRequested"/> returns true. Any callbacks or cancelable
    ///         operations registered with the <see cref="CancellationToken"/>  will be executed.
    ///     </para>
    ///     <para>
    ///         Cancelable operations and callbacks registered with the token should not throw exceptions. However, this
    ///         overload of Cancel will aggregate any exceptions thrown into a <see cref="AggregateException"/>, such that one
    ///         callback throwing an exception will not prevent other registered callbacks from being executed.
    ///     </para>
    ///     <para>
    ///         The <see cref="ExecutionContext"/> that was captured when each callback was registered will be reestablished
    ///         when the callback is invoked.
    ///     </para>
    /// </remarks>
    void Cancel(bool useNewThread = true);
}