using System;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Event arguments for the <see cref="IActivate.Activating" /> event.</summary>
public class ActivatingEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ActivatingEventArgs" /> class.
    /// </summary>
    /// <param name="willInitialize">A value indicating whether the sender will be initialized in addition to being activated.</param>
    public ActivatingEventArgs(bool willInitialize)
    {
        WillInitialize = willInitialize;
    }

    /// <summary>Gets a value indicating whether the sender will be initialized in addition to being activated.</summary>
    public bool WillInitialize { get; }
}