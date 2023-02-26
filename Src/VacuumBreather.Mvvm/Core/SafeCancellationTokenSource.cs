using System;
using System.Threading;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     <para>
///         Thread safe cancellation token source. Allows the following:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 Cancel will no-op if the token is disposed.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Dispose may be called after Cancel.
///             </description>
///         </item>
///     </list>
/// </summary>
public sealed class SafeCancellationTokenSource : IDisposable
{
    private readonly CancellationTokenSource _cts;
    private int _state;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SafeCancellationTokenSource" /> class.
    /// </summary>
    public SafeCancellationTokenSource()
    {
        _cts = new CancellationTokenSource();
        Token = _cts.Token;
    }

    private SafeCancellationTokenSource(CancellationTokenSource cts)
    {
        _cts = cts;
        Token = _cts.Token;
    }

    /// <summary>
    ///     Gets a value indicating whether cancellation has been requested for this
    ///     <see cref="SafeCancellationTokenSource" />.
    /// </summary>
    /// <value>Whether cancellation has been requested for this <see cref="SafeCancellationTokenSource" />.</value>
    /// <remarks>
    ///     <para>
    ///         This property indicates whether cancellation has been requested for this token source, such as
    ///         due to a call to its <see cref="Cancel" /> method.
    ///     </para>
    ///     <para>
    ///         If this property returns true, it only guarantees that cancellation has been requested. It does not
    ///         guarantee that every handler registered with the corresponding token has finished executing, nor
    ///         that cancellation requests have finished propagating to all registered handlers. Additional
    ///         synchronization may be required, particularly in situations where related objects are being
    ///         canceled concurrently.
    ///     </para>
    /// </remarks>
    public bool IsCancellationRequested => _state != State.Initial;

    /// <summary>Gets the <see cref="CancellationToken" /> associated with this <see cref="SafeCancellationTokenSource" />.</summary>
    /// <value>The <see cref="CancellationToken" /> associated with this <see cref="SafeCancellationTokenSource" />.</value>
    public CancellationToken Token { get; }

    /// <summary>
    ///     Creates a <see cref="SafeCancellationTokenSource" /> that will be in the canceled state
    ///     when the supplied token is in the canceled state.
    /// </summary>
    /// <param name="token">The <see cref="CancellationToken">CancellationToken</see> to observe.</param>
    /// <returns>A <see cref="SafeCancellationTokenSource" /> that is linked to the source token.</returns>
    public static SafeCancellationTokenSource CreateLinkedTokenSource(CancellationToken token)
    {
        return new SafeCancellationTokenSource(CancellationTokenSource.CreateLinkedTokenSource(token));
    }

    /// <summary>
    ///     Communicates a request for cancellation.
    /// </summary>
    /// <param name="useNewThread">
    ///     (Optional) If set to <see langword="true" /> cancellation will be invoked on another thread.
    ///     The default is <see langword="true" />.
    /// </param>
    /// <exception cref="AggregateException">
    ///     An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated
    ///     <see cref="CancellationToken" />.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         The associated <see cref="CancellationToken" /> will be notified of the cancellation
    ///         and will transition to a state where <see cref="CancellationToken.IsCancellationRequested" /> returns true.
    ///         Any callbacks or cancelable operations registered with the <see cref="CancellationToken" />  will be executed.
    ///     </para>
    ///     <para>
    ///         Cancelable operations and callbacks registered with the token should not throw exceptions.
    ///         However, this overload of Cancel will aggregate any exceptions thrown into a <see cref="AggregateException" />,
    ///         such that one callback throwing an exception will not prevent other registered callbacks from being executed.
    ///     </para>
    ///     <para>
    ///         The <see cref="ExecutionContext" /> that was captured when each callback was registered
    ///         will be reestablished when the callback is invoked.
    ///     </para>
    /// </remarks>
    public void Cancel(bool useNewThread = true)
    {
        var value = Interlocked.CompareExchange(ref _state, State.Cancelling, State.Initial);

        if (value == State.Initial)
        {
            if (!useNewThread)
            {
                CancelCore();

                return;
            }

            // Because cancellation tokens are so poorly behaved, always invoke the cancellation token on
            // another thread. Don't capture any of the context (execution context or sync context)
            // while doing this.
            ThreadPool.UnsafeQueueUserWorkItem(_ => { CancelCore(); }, null);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var value = Interlocked.Exchange(ref _state, State.Disposing);

        switch (value)
        {
            case State.Initial:
            case State.Cancelled:
                _cts.Dispose();
                Interlocked.Exchange(ref _state, State.Disposed);

                break;

            case State.Cancelling:
            case State.Disposing:
                // No-op
                break;

            case State.Disposed:
                Interlocked.Exchange(ref _state, State.Disposed);

                break;
        }
    }

    private void CancelCore()
    {
        try
        {
            _cts.Cancel();
        }
        finally
        {
            if (Interlocked.CompareExchange(ref _state, State.Cancelled, State.Cancelling) == State.Disposing)
            {
                _cts.Dispose();
                Interlocked.Exchange(ref _state, State.Disposed);
            }
        }
    }

    private static class State
    {
        public const int Cancelled = 2;
        public const int Cancelling = 1;
        public const int Disposed = 4;
        public const int Disposing = 3;
        public const int Initial = 0;
    }
}