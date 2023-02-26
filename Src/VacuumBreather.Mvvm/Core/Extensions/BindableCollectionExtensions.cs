using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="IBindableCollection{T}" /> type.</summary>
internal static class BindableCollectionExtensions
{
    /// <summary>
    ///     Assigns a <see cref="IParent" /> to items being added to the collection and sets it to
    ///     <see langword="null" /> from removed items.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    /// <param name="children">The collection of child items.</param>
    /// <param name="parent">The parent.</param>
    public static void AreChildrenOf<T>(this IBindableCollection<T> children, IParent parent)
        where T : class
    {
        children.CollectionChanged += (s, e) =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    SetParent(e.NewItems, parent);

                    break;

                case NotifyCollectionChangedAction.Remove:
                    SetParent(e.OldItems, null);

                    break;

                case NotifyCollectionChangedAction.Replace:
                    SetParent(e.OldItems, null);
                    SetParent(e.NewItems, parent);

                    break;

                case NotifyCollectionChangedAction.Reset:
                    SetParent((IBindableCollection<T>?)s, parent);

                    break;
            }
        };
    }

    private static void SetParent(IEnumerable? children, object? parent)
    {
        children?.OfType<IChild>().ForEach(child => child.Parent = parent);
    }
}