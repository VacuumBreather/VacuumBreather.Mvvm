using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A ResourceDictionary that ensures that a source is only loaded once and otherwise retrieved from a cache.</summary>
/// <seealso cref="System.Windows.ResourceDictionary"/>
[PublicAPI]
[SuppressMessage(category: "Design",
                 checkId: "CA1010:Generic interface should also be implemented",
                 Justification =
                     "It is a special kind of ResourceDictionary. The additional interfaces are not required.")]
public sealed class SharedResourceDictionary : ResourceDictionary
{
    private static readonly object LockObject = new();

    private static readonly ConcurrentDictionary<Uri, Entry> ResourceDictionaries = new();

    private Entry? _entry;

    /// <summary>Gets or sets the source.</summary>
    /// <value>The source.</value>
    public new Uri? Source
    {
        get => _entry?.SourceResourceDictionary.Source;

        set
        {
            if (Source == value)
            {
                return;
            }

            if ((Source != null) && (_entry != null))
            {
                MergedDictionaries.Remove(_entry.SourceResourceDictionary);

                lock (LockObject)
                {
                    _entry.ReferencingResourceDictionaries.Remove(this);

                    if (IsFirstSourceReference)
                    {
                        var newFirstSource = _entry.ReferencingResourceDictionaries.FirstOrDefault();

                        if (newFirstSource != null)
                        {
                            _entry.FirstReferencingResourceDictionary = newFirstSource;
                        }
                        else
                        {
                            ResourceDictionaries.TryRemove(Source, out _);
                        }
                    }
                }
            }

            if (value != null)
            {
                _entry = ResourceDictionaries.AddOrUpdate(value,
                                                          _ =>
                                                          {
                                                              var actualResourceDictionary =
                                                                  new ResourceDictionary { Source = value };

                                                              MergedDictionaries.Add(actualResourceDictionary);

                                                              return new Entry(actualResourceDictionary, this);
                                                          },
                                                          (_, existingEntry) =>
                                                          {
                                                              lock (LockObject)
                                                              {
                                                                  existingEntry.ReferencingResourceDictionaries.Add(
                                                                      this);
                                                              }

                                                              MergedDictionaries.Add(
                                                                  existingEntry.SourceResourceDictionary);

                                                              return existingEntry;
                                                          });
            }
        }
    }

    private bool IsFirstSourceReference => ReferenceEquals(_entry?.FirstReferencingResourceDictionary, this);

    /// <summary>Tries to remove a resources by <see cref="Uri"/> from the cache, if there are no references.</summary>
    /// <param name="source">The source.</param>
    /// <returns><c>true</c>, if the item could be removed, otherwise <c>false</c>.</returns>
    public static bool TryRemoveFromCache(Uri source)
    {
        if (ResourceDictionaries.TryGetValue(source, out var entry))
        {
            lock (LockObject)
            {
                if (entry.ReferencingResourceDictionaries.Count == 0)
                {
                    return ResourceDictionaries.TryRemove(source, out _);
                }

                var onlyReferencingResourceDictionary = entry.ReferencingResourceDictionaries.SingleOrDefault();

                if (ReferenceEquals(onlyReferencingResourceDictionary, entry.FirstReferencingResourceDictionary))
                {
                    return ResourceDictionaries.TryRemove(source, out _);
                }
            }
        }

        return false;
    }

    private sealed class Entry
    {
        internal Entry(ResourceDictionary sourceResourceDictionary,
                       SharedResourceDictionary firstReferencingResourceDictionary)
        {
            SourceResourceDictionary = sourceResourceDictionary;
            FirstReferencingResourceDictionary = firstReferencingResourceDictionary;
            ReferencingResourceDictionaries = new List<SharedResourceDictionary> { firstReferencingResourceDictionary };
        }

        internal SharedResourceDictionary FirstReferencingResourceDictionary { get; set; }

        internal List<SharedResourceDictionary> ReferencingResourceDictionaries { get; }

        internal ResourceDictionary SourceResourceDictionary { get; }
    }
}