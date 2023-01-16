﻿// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Provides extension methods for the <see cref="IEnumerable{T}" /> type.</summary>
    public static class EnumerableExtensions
    {
        /// <summary>Returns every element of the sequence that is not null.</summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        public static IEnumerable<T?> NotNull<T>(this IEnumerable<T?> source)
            where T : class
        {
            return source.Where(item => item != null);
        }
    }
}