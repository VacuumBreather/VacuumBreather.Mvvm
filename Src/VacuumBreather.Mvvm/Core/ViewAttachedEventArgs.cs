using System;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Event arguments for the <see cref="IViewAware.ViewAttached" /> event.</summary>
public class ViewAttachedEventArgs : EventArgs
{
    /// <summary>Gets or sets the ID of the context in which the view appears.</summary>
    public Guid Context { get; set; }

    /// <summary>Gets or sets the attached view.</summary>
    public object? View { get; set; }
}