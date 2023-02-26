using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     A dynamic data collection that provides notifications when items get added, removed, or
///     when the whole list is refreshed.
/// </summary>
/// <typeparam name="T">The type of elements contained in the collection.</typeparam>
[PublicAPI]
public class BindableCollection<T> : ObservableCollection<T>, IBindableCollection<T>, IReadOnlyBindableCollection<T>,
                                     IBindableObject
{
    private int _suspensionCount;

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
    public bool IsNotifying => _suspensionCount == 0;

    /// <inheritdoc />
    public virtual void AddRange(IEnumerable<T> items)
    {
        void LocalAddRange()
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.IsNotNull(items);

            CheckReentrancy();

            using (var _ = SuspendNotifications())
            {
                int index = Count;

                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var item in items)
                {
                    base.InsertItem(index, item);
                    index++;
                }
            }

            OnCollectionRefreshed();
        }

        ThreadHelper.RunOnUIThread(LocalAddRange);
    }

    /// <inheritdoc />
    public virtual void RemoveRange(IEnumerable<T> items)
    {
        void LocalRemoveRange()
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.IsNotNull(items);

            CheckReentrancy();

            using (var _ = SuspendNotifications())
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var item in items)
                {
                    int index = IndexOf(item);

                    if (index >= 0)
                    {
                        base.RemoveItem(index);
                    }
                }
            }

            OnCollectionRefreshed();
        }

        ThreadHelper.RunOnUIThread(LocalRemoveRange);
    }

    /// <summary>
    ///     Raises a property and collection changed event that notifies that all of the properties on
    ///     this object have changed.
    /// </summary>
    public void Refresh()
    {
        ThreadHelper.RunOnUIThread(OnCollectionRefreshed);
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

    /// <summary>Clears the items contained by the collection.</summary>
    protected sealed override void ClearItems()
    {
        ThreadHelper.RunOnUIThread(() => base.ClearItems());
    }

    /// <inheritdoc />
    protected sealed override void InsertItem(int index, T item)
    {
        ThreadHelper.RunOnUIThread(() => base.InsertItem(index, item));
    }

    /// <summary>
    ///     Raises the
    ///     <see cref="ObservableCollection{T}.CollectionChanged" /> event
    ///     with the provided arguments.
    /// </summary>
    /// <param name="e">Arguments of the event being raised.</param>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (IsNotifying)
        {
            base.OnCollectionChanged(e);
        }
    }

    /// <summary>Raises the PropertyChanged event with the provided arguments.</summary>
    /// <param name="e">The event data to report in the event.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (IsNotifying)
        {
            base.OnPropertyChanged(e);
        }
    }

    /// <inheritdoc />
    protected sealed override void RemoveItem(int index)
    {
        ThreadHelper.RunOnUIThread(() => base.RemoveItem(index));
    }

    /// <inheritdoc />
    protected sealed override void SetItem(int index, T item)
    {
        ThreadHelper.RunOnUIThread(() => base.SetItem(index, item));
    }

    private void OnCollectionRefreshed()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void ResumeNotifications()
    {
        Interlocked.Decrement(ref _suspensionCount);
    }
}