using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Wpf;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Provides a helper method to automatically handle the setting of a status on a
///     <see cref="TaskCompletionSource" /> after a provided guard goes out of scope.
/// </summary>
public static class TaskCompletion
{
    /// <summary>
    ///     Awaits an <see cref="IAwaitable" />, representing the completion of an asynchronous operation. Can be
    ///     <see langword="null" />.
    /// </summary>
    /// <param name="awaitableCompletion">
    ///     The <see cref="IAwaitable" /> representing the completion of an asynchronous
    ///     operation.
    /// </param>
    /// <returns>A <see cref="ValueTask" /> that represents the asynchronous operation.</returns>
    public static async ValueTask AwaitCompletionAsync(IAwaitable? awaitableCompletion)
    {
        if (awaitableCompletion is not null)
        {
            await awaitableCompletion;
        }
    }

    public static IAsyncOperation CreateAsyncOperation(CancellationToken cancellationToken = default)
    {
        return new AsyncOperation(cancellationToken);
    }

    public static IAsyncOperation CreateAsyncOperation(AsyncGuard guard, CancellationToken cancellationToken = default)
    {
        return new AsyncOperation(guard, cancellationToken);
    }

    private static IDisposable CreateGuard(out TaskCompletionSource completionSource)
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        completionSource = source;

        return new DisposableAction(() => source.TrySetResult());
    }

    private class AsyncOperation : IAsyncOperation
    {
        private readonly IDisposable? _guardToken;
        private readonly IDisposable _completionGuard;
        private readonly TaskCompletionSource _completion;
        private readonly SafeCancellationTokenSource _cancellationTokenSource;

        public AsyncOperation(CancellationToken cancellationToken = default)
        {
            _completionGuard = CreateGuard(out _completion);
            _cancellationTokenSource = SafeCancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public AsyncOperation(AsyncGuard asyncGuard, CancellationToken cancellationToken)
            : this(cancellationToken)
        {
            _guardToken = asyncGuard.GetToken();
        }

        public TaskAwaiter GetAwaiter()
        {
            return _completion.Task.GetAwaiter();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _guardToken?.Dispose();
            _completionGuard.Dispose();
            _cancellationTokenSource.Dispose();
        }

        public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

        /// <inheritdoc />
        public CancellationToken Token => _cancellationTokenSource.Token;

        /// <inheritdoc />
        public void Cancel(bool useNewThread = true)
        {
            _cancellationTokenSource.Cancel(useNewThread);
        }
    }
}