// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using CommunityToolkit.Diagnostics;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Executes an action when disposed.</summary>
internal sealed class DisposableAction : IDisposable
{
    private readonly Action action;
    private bool isDisposed;

    /// <summary>Initializes a new instance of the <see cref="DisposableAction" /> class.</summary>
    /// <param name="action">The action to execute on dispose.</param>
    public DisposableAction(Action action)
    {
        Guard.IsNotNull(action, nameof(action));

        this.action = action;
    }

    /// <summary>Executes the supplied action.</summary>
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.action();
        this.isDisposed = true;
    }
}