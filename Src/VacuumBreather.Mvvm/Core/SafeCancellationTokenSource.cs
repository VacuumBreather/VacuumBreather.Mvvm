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
public sealed class SafeCancellationTokenSource : IDisposable, ICanBeCanceled
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

    /// <inheritdoc />
    public bool IsCancellationRequested => _state != State.Initial;

    /// <inheritdoc />
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

    /// <inheritdoc />
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