using System;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Defines a view-model type which is aware of its view.</summary>
public interface IViewAware
{
    /// <summary>Raised when a view is attached.</summary>
    event EventHandler<ViewAttachedEventArgs> ViewAttached;

    /// <summary>Attaches a view to this instance.</summary>
    /// <param name="view">The view.</param>
    /// <param name="context">The ID of the context in which the view appears.</param>
    void AttachView(object view, Guid context);

    /// <summary>Gets a view previously attached to this instance.</summary>
    /// <param name="context">The ID of the context in which the view appears.</param>
    /// <returns>The attached view, or <see langword="null"/> if no view has been attached for the specified context.</returns>
    object? GetView(Guid context);
}