using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="IEnumerable{T}"/> type.</summary>
public static class EnumerableExtensions
{
    /// <summary>Applies an accumulator function over a sequence.</summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/> to aggregate over.</param>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="func">An accumulator function to be invoked on each element.</param>
    [SuppressMessage(category: "Performance",
                     checkId: "CA1806:Do not ignore method results",
                     Justification =
                         "The side effect is implied. This function returns void, so it cannot be used in the middle of a query.")]
    public static void AggregateForEach<TSource, TAccumulate>(this IEnumerable<TSource> source,
                                                              TAccumulate seed,
                                                              Func<TAccumulate, TSource, TAccumulate> func)
    {
        _ = source.Aggregate(seed, func);
    }

    /// <summary>Performs the specified operation on each item in the source sequence.</summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The action to perform on each element.</param>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}