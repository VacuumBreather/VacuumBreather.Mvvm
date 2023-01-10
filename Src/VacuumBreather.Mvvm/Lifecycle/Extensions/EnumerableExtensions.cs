// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Collections.Generic;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Provides extension methods for the <see cref="IEnumerable{T}" /> type.</summary>
    public static class EnumerableExtensions
    {
        /// <summary>Performs the specified operation on each item in the source sequence.</summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="action">The action to perform on each element.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
            {
                action(item);
            }
        }
    }
}