// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Windows;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Defines a view-model type which is aware of its view.</summary>
    public interface IViewAware
    {
        /// <summary>Raised when a view is attached.</summary>
        event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>Attaches a view to this instance.</summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The ID of the context in which the view appears.</param>
        void AttachView(UIElement view, Guid context);

        /// <summary>Gets a view previously attached to this instance.</summary>
        /// <param name="context">The ID of the context in which the view appears.</param>
        /// <returns>The attached view, or <c>null</c> if no view has been attached for the specified context.</returns>
        UIElement? GetView(Guid context);
    }
}