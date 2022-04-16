// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using CommunityToolkit.Diagnostics;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>
///     A dynamic data collection that provides notifications when items get added, removed, or
///     when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The type of elements contained in the collection.</typeparam>
public class BindableCollection<T> : ObservableCollection<T>, IBindableCollection<T>, IReadOnlyBindableCollection<T>
{
    private int suspensionCount;

    /// <summary>Initializes a new instance of the <see cref="BindableCollection{T}" /> class.</summary>
    public BindableCollection()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BindableCollection{T}" /> class that contains
    ///     elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection from which the elements are copied.</param>
    /// <exception cref="ArgumentNullException">
    ///     The <paramref name="collection" /> parameter cannot be
    ///     <see langword="null" />.
    /// </exception>
    public BindableCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <inheritdoc />
    public virtual void AddRange(IEnumerable<T> items)
    {
        Guard.IsNotNull(items, nameof(items));

        CheckReentrancy();

        using var _ = SuspendNotifications();

        var index = Count;

        foreach (var item in items)
        {
            InsertItem(index, item);
            index++;
        }

        OnCollectionRefreshed();
    }

    /// <summary>
    ///     Raises a property and collection changed event that notifies that all of the properties on
    ///     this object have changed.
    /// </summary>
    public void Refresh()
    {
        CheckReentrancy();
        OnCollectionRefreshed();
    }

    /// <inheritdoc />
    public virtual void RemoveRange(IEnumerable<T> items)
    {
        Guard.IsNotNull(items, nameof(items));

        CheckReentrancy();

        using var _ = SuspendNotifications();

        foreach (var item in items)
        {
            var index = IndexOf(item);

            if (index >= 0)
            {
                RemoveItem(index);
            }
        }

        OnCollectionRefreshed();
    }

    /// <summary>Suspends the change notifications.</summary>
    /// <returns>A guard resuming the notifications when it goes out of scope.</returns>
    /// <remarks><para>Use the guard in a using statement.</para></remarks>
    public IDisposable SuspendNotifications()
    {
        Interlocked.Increment(ref this.suspensionCount);

        return new DisposableAction(ResumeNotifications);
    }

    /// <summary>
    ///     Raises a property and collection changed event that notifies that all of the properties on
    ///     this object have changed.
    /// </summary>
    protected virtual void OnCollectionRefreshed()
    {
        if (AreNotificationsSuspended())
        {
            return;
        }

        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>Clears the items contained by the collection.</summary>
    protected override sealed void ClearItems()
    {
        CheckReentrancy();
        base.ClearItems();
    }

    /// <summary>
    ///     Raises the
    ///     <see cref="System.Collections.ObjectModel.ObservableCollection{T}.CollectionChanged" /> event
    ///     with the provided arguments.
    /// </summary>
    /// <param name="e">Arguments of the event being raised.</param>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (AreNotificationsSuspended())
        {
            return;
        }

        base.OnCollectionChanged(e);
    }

    /// <summary>Raises the PropertyChanged event with the provided arguments.</summary>
    /// <param name="e">The event data to report in the event.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (AreNotificationsSuspended())
        {
            return;
        }

        base.OnPropertyChanged(e);
    }

    /// <summary>Determines whether notifications are suspended.</summary>
    /// <returns>A value indicating whether notifications are currently suspended.</returns>
    protected bool AreNotificationsSuspended()
    {
        return this.suspensionCount > 0;
    }

    private void ResumeNotifications()
    {
        Interlocked.Decrement(ref this.suspensionCount);
    }
}