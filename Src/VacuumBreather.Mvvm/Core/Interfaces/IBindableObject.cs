using System;
using System.ComponentModel;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Extends <see cref="INotifyPropertyChanged"/>  such that the change event can be suspended.</summary>
public interface IBindableObject : INotifyPropertyChanged
{
    /// <summary>Gets a value indicating whether notifications will be sent or are currently suspended.</summary>
    /// <returns><see langword="true"/> if the notifications will be sent; otherwise, <see langword="false"/>.</returns>
    bool IsNotifying { get; }

    /// <summary>Raises a change notification indicating that all bindings should be refreshed.</summary>
    void Refresh();

    /// <summary>Suspends property change notifications.</summary>
    /// <returns>An object which, when disposed, resumes the notifications.</returns>
    IDisposable SuspendNotifications();
}