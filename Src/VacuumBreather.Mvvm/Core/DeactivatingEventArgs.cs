using System;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Event arguments for the <see cref="IDeactivate.Deactivating" />
///     <see cref="IDeactivate.Deactivating" /> events.
/// </summary>
public class DeactivatingEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeactivatingEventArgs" /> class.
    /// </summary>
    /// <param name="willClose">A value indicating whether the sender will be closed in addition to being deactivated.</param>
    public DeactivatingEventArgs(bool willClose)
    {
        WillClose = willClose;
    }

    /// <summary>Gets a value indicating whether the sender will be closed in addition to being deactivated.</summary>
    public bool WillClose { get; }
}