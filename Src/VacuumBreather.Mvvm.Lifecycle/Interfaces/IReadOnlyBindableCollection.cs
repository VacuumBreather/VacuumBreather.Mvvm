﻿namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    ///     Represents a readonly dynamic data collection that provides notifications when items get
    ///     added, removed, or when the whole list is refreshed.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    public interface IReadOnlyBindableCollection<out T> : IReadOnlyList<T>,
                                                          INotifyCollectionChanged,
                                                          INotifyPropertyChanged
    {
    }
}