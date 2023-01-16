// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Event arguments for the <see cref="IViewAware.ViewAttached" /> event.</summary>
    public class ViewAttachedEventArgs : EventArgs
    {
        /// <summary>The ID of the context in which the view appears.</summary>
        public Guid Context { get; set; }

        /// <summary>The attached view.</summary>
        public object? View { get; set; }
    }
}