﻿using System.Collections.Generic;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="IDictionary{TKey,TValue}"/> type.</summary>
[PublicAPI]
public static class DictionaryExtensions
{
    /// <summary>Gets the value for a key or default(TValue) if the key does not exist.</summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to call this method on.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value if the key exists in the dictionary; default(TValue) otherwise.</returns>
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out var result) ? result : default;
    }
}