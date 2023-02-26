using System;
using System.Collections.Generic;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     A base implementation of <see cref="IViewAware" /> which is capable of caching views by
///     context.
/// </summary>
public class ViewAware : BindableObject, IViewAware
{
    private readonly IDictionary<Guid, object> _viewCache = new Dictionary<Guid, object>();

    /// <inheritdoc />
    public event EventHandler<ViewAttachedEventArgs>? ViewAttached;

    /// <inheritdoc />
    public void AttachView(object view, Guid context)
    {
        _viewCache[context] = view;
        OnViewAttached(view, context);
    }

    /// <inheritdoc />
    public object? GetView(Guid context)
    {
        return _viewCache.TryGetValue(context, out object? view) ? view : null;
    }

    /// <summary>Called when a view is attached.</summary>
    /// <param name="view">The view.</param>
    /// <param name="context">The ID of the context in which the view appears.</param>
    protected virtual void OnViewAttached(object view, Guid context)
    {
        ViewAttached?.Invoke(this, new ViewAttachedEventArgs { View = view, Context = context });
    }
}