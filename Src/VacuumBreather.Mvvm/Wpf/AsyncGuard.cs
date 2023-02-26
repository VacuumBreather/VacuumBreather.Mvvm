using System;
using System.Threading;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Helper class to keep track of ongoing asynchronous operations.</summary>
/// <example>
///     <code>
///     await OperationAsync().Using(asyncGuard);
///     </code>
/// </example>
public sealed class AsyncGuard
{
    private int _asyncCounter;

    /// <summary>Occurs when the number of ongoing operations has changed.</summary>
    public event EventHandler? IsOngoingChanged;

    /// <summary>
    ///     Gets a value indicating whether any asynchronous operations are still ongoing (i.e. using
    ///     tokens).
    /// </summary>
    /// <value><see langword="true" /> if any asynchronous operations are still ongoing; otherwise, <see langword="false" />.</value>
    public bool IsOngoing => _asyncCounter != 0;

    /// <summary>
    ///     Gets a new token to track an asynchronous operation. Use as the <see cref="IDisposable" />
    ///     with <see cref="ValueTaskExtensions.Using" />.
    /// </summary>
    /// <returns>A new token to track an asynchronous operation.</returns>
    public IDisposable GetToken()
    {
        IncrementCounter();

        return new DisposableAction(DecrementCounter);
    }

    private void DecrementCounter()
    {
        Interlocked.Decrement(ref _asyncCounter);
        RaiseIsOngoingChanged();
    }

    private void IncrementCounter()
    {
        Interlocked.Increment(ref _asyncCounter);
        RaiseIsOngoingChanged();
    }

    private void RaiseIsOngoingChanged()
    {
        IsOngoingChanged?.Invoke(this, EventArgs.Empty);
    }
}