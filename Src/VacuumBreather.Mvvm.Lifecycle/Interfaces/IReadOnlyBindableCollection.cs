// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace VacuumBreather.Mvvm.Lifecycle;

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