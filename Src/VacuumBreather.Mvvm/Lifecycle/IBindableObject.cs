// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     Extends <see cref="INotifyPropertyChanged" />  such that the change event can be
    ///     suspended.
    /// </summary>
    public interface IBindableObject : INotifyPropertyChanged
    {
        /// <summary>Raises a change notification indicating that all bindings should be refreshed.</summary>
        void Refresh();

        /// <summary>Suspends property change notifications.</summary>
        /// <returns>An object which, when disposed, resumes the notifications.</returns>
        IDisposable SuspendNotifications();
    }
}