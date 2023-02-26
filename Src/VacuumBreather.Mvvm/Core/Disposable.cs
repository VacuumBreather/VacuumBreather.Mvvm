using System;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides a disposable that does nothing when disposed.</summary>
public static class Disposable
{
    /// <summary>Gets a disposable that does nothing when disposed.</summary>
    public static IDisposable Empty => EmptyDisposable.Instance;

    private sealed class EmptyDisposable : IDisposable
    {
        public static readonly IDisposable Instance = new EmptyDisposable();

        public void Dispose()
        {
        }
    }
}