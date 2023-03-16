using System;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Executes an action when disposed.</summary>
[PublicAPI]
public sealed class DisposableAction : IDisposable
{
    private readonly Action _action;
    private bool _isDisposed;

    /// <summary>Initializes a new instance of the <see cref="DisposableAction"/> class.</summary>
    /// <param name="action">The action to execute on dispose.</param>
    public DisposableAction(Action action)
    {
        Guard.IsNotNull(action);

        _action = action;
    }

    /// <summary>Gets a <see cref="DisposableAction"/> that does nothing.</summary>
    public static DisposableAction DoNothing { get; } = new(() => { });

    /// <summary>Executes the supplied action.</summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _action();
        _isDisposed = true;
    }
}