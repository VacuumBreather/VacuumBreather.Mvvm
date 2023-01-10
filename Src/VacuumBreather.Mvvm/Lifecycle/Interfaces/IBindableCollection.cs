// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     Represents a dynamic data collection that provides notifications when items get added,
    ///     removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    public interface IBindableCollection<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>Adds a range of items to this collection.</summary>
        /// <param name="items">The items to add.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="items" /> parameter cannot be
        ///     <see langword="null" />.
        /// </exception>
        void AddRange(IEnumerable<T> items);

        /// <summary>Raises a change notification indicating that all bindings should be refreshed.</summary>
        void Refresh();

        /// <summary>Removes a range of items from this collection.</summary>
        /// <param name="items">The items to remove.</param>
        /// <exception cref="ArgumentNullException">
        ///     The <paramref name="items" /> parameter cannot be
        ///     <see langword="null" />.
        /// </exception>
        void RemoveRange(IEnumerable<T> items);

        /// <summary>Suspends property change notifications.</summary>
        /// <returns>An <see cref="IDisposable" /> instance which, when disposed, will cause the notifications to resume.</returns>
        IDisposable SuspendNotifications();
    }
}