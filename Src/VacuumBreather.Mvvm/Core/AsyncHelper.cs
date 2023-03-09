using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides a helper method to automatically handle ongoing asynchronous operations and their cancellation.</summary>
[PublicAPI]
public static class AsyncHelper
{
    /// <summary>
    ///     Awaits an <see cref="IAwaitable"/>, representing the completion of an asynchronous operation. Can be
    ///     <see langword="null"/>. This ignores any <see cref="OperationCanceledException"/> thrown by the task.
    /// </summary>
    /// <param name="awaitableCompletion">
    ///     The <see cref="IAwaitable"/> representing the completion of an asynchronous
    ///     operation.
    /// </param>
    /// <param name="cancellationToken">(Optional) An external cancellation token, which can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
    public static async ValueTask AwaitCompletionAsync(IAwaitable? awaitableCompletion,
                                                       CancellationToken cancellationToken = default)
    {
        if (awaitableCompletion is not null && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                await awaitableCompletion;
            }
            catch (OperationCanceledException)
            {
                // Ignore this. We only care about the task completion, not about how it completed.
            }
        }
    }

    /// <summary>
    ///     Creates an awaitable and cancelable object which represents the asynchronous operation in the current scope.
    ///     Use in a using block or statement.
    /// </summary>
    /// <param name="cancellationToken">(Optional) An external cancellation token, which can be used to cancel the operation.</param>
    /// <returns>The <see cref="IAsyncOperation"/> representing the asynchronous operation in the current scope.</returns>
    public static IAsyncOperation CreateAsyncOperation(CancellationToken cancellationToken = default)
    {
        return new AsyncOperation(cancellationToken);
    }

    /// <summary>
    ///     Creates an awaitable and cancelable object which represents the asynchronous operation in the current scope.
    ///     Use in a using block or statement.
    /// </summary>
    /// <param name="guard">An <see cref="AsyncGuard"/> which will report the ongoing state of the asynchronous operation.</param>
    /// <param name="cancellationToken">(Optional) An external cancellation token, which can be used to cancel the operation.</param>
    /// <returns>The <see cref="IAsyncOperation"/> representing the asynchronous operation in the current scope.</returns>
    public static IAsyncOperation CreateAsyncOperation(AsyncGuard guard, CancellationToken cancellationToken = default)
    {
        return new AsyncOperation(guard, cancellationToken);
    }

    private sealed class AsyncOperation : IAsyncOperation
    {
        private readonly SafeCancellationTokenSource _cancellationTokenSource;
        private readonly TaskCompletionSource _completion;
        private readonly IDisposable _completionGuard;
        private readonly IDisposable? _guardToken;

        internal AsyncOperation(CancellationToken cancellationToken = default)
        {
            _completionGuard = CreateGuard(out _completion);
            _cancellationTokenSource = SafeCancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _cancellationTokenSource.Token.Register(() => _completion.SetCanceled(_cancellationTokenSource.Token));
        }

        internal AsyncOperation(AsyncGuard asyncGuard, CancellationToken cancellationToken)
            : this(cancellationToken)
        {
            _guardToken = asyncGuard.GetToken();
        }

        public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

        public CancellationToken Token => _cancellationTokenSource.Token;

        public void Cancel(bool useNewThread = true)
        {
            _cancellationTokenSource.Cancel(useNewThread);
        }

        public void Dispose()
        {
            _guardToken?.Dispose();
            _completionGuard.Dispose();
            _cancellationTokenSource.Dispose();
        }

        public TaskAwaiter GetAwaiter()
        {
            return _completion.Task.GetAwaiter();
        }

        private static IDisposable CreateGuard(out TaskCompletionSource completionSource)
        {
            var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            completionSource = source;

            return new DisposableAction(() => source.TrySetResult());
        }
    }
}