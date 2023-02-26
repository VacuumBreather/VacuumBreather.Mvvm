using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Notifying base class for view models.
/// </summary>
/// <seealso cref="VacuumBreather.Mvvm.Core.IBindableObject" />
/// <seealso cref="System.ComponentModel.INotifyPropertyChanging" />
public abstract class BindableObject : IBindableObject, INotifyPropertyChanging
{
    private int _suspensionCount;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <inheritdoc />
    public bool IsNotifying => _suspensionCount == 0;

    /// <summary>Raises a change notification indicating that all bindings should be refreshed.</summary>
    public virtual void Refresh()
    {
        OnPropertyChanging(string.Empty);
        OnPropertyChanged(string.Empty);
    }

    /// <summary>Suspends the change notifications.</summary>
    /// <returns>A guard resuming the notifications when it goes out of scope.</returns>
    /// <remarks>
    ///     <para>Use the guard in a using statement.</para>
    /// </remarks>
    public IDisposable SuspendNotifications()
    {
        Interlocked.Increment(ref _suspensionCount);

        return new DisposableAction(ResumeNotifications);
    }

    /// <summary>Assigns a new value to the property. Then, raises the PropertyChanged event if needed.</summary>
    /// <typeparam name="T">The type of the property that changed.</typeparam>
    /// <param name="field">The field storing the property's value.</param>
    /// <param name="newValue">The property's value after the change occurred.</param>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <returns>
    ///     <see langword="true" /> if the PropertyChanged event has been raised, otherwise, <see langword="false" />. The
    ///     event is not raised if the old value is equal to the new value.
    /// </returns>
    protected virtual bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
        {
            return false;
        }

        OnPropertyChanging(propertyName);
        field = newValue;
        OnPropertyChanged(propertyName);

        return true;
    }

    /// <summary>Raises the PropertyChanged event if needed.</summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged(string? propertyName = null)
    {
        if (IsNotifying)
        {
            ThreadHelper.RunOnUIThread(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }

    /// <summary>Raises the PropertyChanging event if needed.</summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    protected void OnPropertyChanging(string? propertyName = null)
    {
        if (IsNotifying)
        {
            ThreadHelper.RunOnUIThread(
                () => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName)));
        }
    }

    private void ResumeNotifications()
    {
        Interlocked.Decrement(ref _suspensionCount);
    }
}