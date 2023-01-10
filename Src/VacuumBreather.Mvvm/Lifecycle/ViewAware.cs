// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Collections.Generic;
using System.Windows;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     A base implementation of <see cref="IViewAware" /> which is capable of caching views by
    ///     context.
    /// </summary>
    public class ViewAware : BindableObject, IViewAware
    {
        private readonly IDictionary<Guid, UIElement> viewCache = new Dictionary<Guid, UIElement>();

        /// <inheritdoc />
        public event EventHandler<ViewAttachedEventArgs>? ViewAttached;

        /// <inheritdoc />
        public void AttachView(UIElement view, Guid context)
        {
            this.viewCache[context] = view;
            OnViewAttached(view, context);
        }

        /// <inheritdoc />
        public UIElement? GetView(Guid context)
        {
            return this.viewCache.TryGetValue(context, out UIElement? view) ? view : null;
        }

        /// <summary>Called when a view is attached.</summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The ID of the context in which the view appears.</param>
        protected virtual void OnViewAttached(UIElement view, Guid context)
        {
            ViewAttached?.Invoke(this, new ViewAttachedEventArgs { View = view, Context = context });
        }
    }
}