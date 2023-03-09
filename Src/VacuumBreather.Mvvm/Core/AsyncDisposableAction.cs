using System;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Executes an asynchronous operation when disposed.</summary>
[PublicAPI]
public sealed class AsyncDisposableAction : IAsyncDisposable
{
    private readonly Func<ValueTask> _asyncOperation;
    private bool _isDisposed;

    /// <summary>Initializes a new instance of the <see cref="AsyncDisposableAction"/> class.</summary>
    /// <param name="asyncOperation">The asynchronous operation to execute on async disposal.</param>
    public AsyncDisposableAction(Func<ValueTask> asyncOperation)
    {
        Guard.IsNotNull(asyncOperation);

        _asyncOperation = asyncOperation;
    }

    /// <summary>Executes the supplied asynchronous operation.</summary>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous save operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        await _asyncOperation();

        _isDisposed = true;
    }
}