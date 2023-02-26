using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Contains implementations of <see cref="IConductor" /> that hold on many items.</summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
[PublicAPI]
public class ConductorCollectionOneActive<T> : ConductorBaseWithActiveItem<T>, ICollectionConductorWithActiveItem<T>
    where T : class
{
    private readonly BindableCollection<T> _items = new();

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="VacuumBreather.Mvvm.Core.ConductorCollectionOneActive{T}" /> class.
    /// </summary>
    public ConductorCollectionOneActive()
    {
        _items.AreChildrenOf(this);
    }

    /// <summary>Gets the items that are currently being conducted.</summary>
    public IBindableCollection<T> Items => _items;

    /// <inheritdoc />
    public sealed override IEnumerable<T> GetChildren()
    {
        return _items;
    }

    /// <inheritdoc />
    public override async ValueTask ActivateItemAsync(T? item, CancellationToken cancellationToken = default)
    {
        if (item?.Equals(ActiveItem) ?? false)
        {
            if (IsActive)
            {
                await ScreenExtensions.TryActivateAsync(item, cancellationToken).ConfigureAwait(true);
                OnActivationProcessed(item, true);
            }

            return;
        }

        await ChangeActiveItemAsync(item, false, cancellationToken).ConfigureAwait(true);
    }

    /// <inheritdoc />
    public override async ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        ICloseResult<T> closeResult = await CloseStrategy.ExecuteAsync(_items.ToList(), cancellationToken)
                                                         .ConfigureAwait(true);

        if (closeResult.CloseCanOccur || !closeResult.Children.Any())
        {
            return closeResult.CloseCanOccur;
        }

        List<T> closable = closeResult.Children.ToList();

        if (ActiveItem is not null && closable.Contains(ActiveItem))
        {
            List<T> list = _items.ToList();
            T? next = ActiveItem;

            do
            {
                T? previous = next;
                next = DetermineNextItemToActivate(list, list.IndexOf(previous));
                list.Remove(previous);
            } while (next is not null && closable.Contains(next));

            T? previousActive = ActiveItem;
            await ChangeActiveItemAsync(next, true, cancellationToken).ConfigureAwait(true);
            _items.Remove(previousActive);
            closable.Remove(previousActive);
        }

        foreach (IDeactivate deactivate in closable.OfType<IDeactivate>())
        {
            await deactivate.DeactivateAsync(true, cancellationToken).ConfigureAwait(true);
        }

        _items.RemoveRange(closable);

        return closeResult.CloseCanOccur;
    }

    /// <inheritdoc />
    public override async ValueTask DeactivateItemAsync(T item,
                                                        bool close,
                                                        CancellationToken cancellationToken = default)
    {
        await this.DeactivateItemAsync(item, close, CloseItemCoreAsync, cancellationToken).ConfigureAwait(true);
    }

    /// <summary>Determines the next item to activate based on the last active index.</summary>
    /// <param name="list">The list of possible active items.</param>
    /// <param name="lastIndex">The index of the last active item.</param>
    /// <returns>The next item to activate.</returns>
    /// <remarks>
    ///     <para>Called after an active item is closed.</para>
    /// </remarks>
    protected virtual T? DetermineNextItemToActivate(IList<T> list, int lastIndex)
    {
        Guard.IsNotNull(list);

        int toRemoveAt = lastIndex - 1;

        if ((toRemoveAt == -1) && (list.Count > 1))
        {
            return list[1];
        }

        if ((toRemoveAt > -1) && (toRemoveAt < list.Count - 1))
        {
            return list[toRemoveAt];
        }

        return default;
    }

    /// <inheritdoc />
    protected override T? EnsureItem(T? newItem)
    {
        if (newItem == null)
        {
            newItem = DetermineNextItemToActivate(_items, ActiveItem != null ? _items.IndexOf(ActiveItem) : 0);
        }
        else
        {
            int index = _items.IndexOf(newItem);

            if (index == -1)
            {
                _items.Add(newItem);
            }
            else
            {
                newItem = _items[index];
            }
        }

        return base.EnsureItem(newItem);
    }

    /// <inheritdoc />
    protected override ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        return ScreenExtensions.TryActivateAsync(ActiveItem, cancellationToken);
    }

    /// <inheritdoc />
    protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        if (close)
        {
            foreach (IDeactivate deactivate in _items.OfType<IDeactivate>())
            {
                await deactivate.DeactivateAsync(true, cancellationToken).ConfigureAwait(true);
            }

            _items.Clear();
        }
        else
        {
            await ScreenExtensions.TryDeactivateAsync(ActiveItem, false, cancellationToken).ConfigureAwait(true);
        }
    }

    private async ValueTask CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
    {
        if (item.Equals(ActiveItem))
        {
            int index = _items.IndexOf(item);
            T? next = DetermineNextItemToActivate(_items, index);

            await ChangeActiveItemAsync(next, true, cancellationToken).ConfigureAwait(true);
        }
        else
        {
            await ScreenExtensions.TryDeactivateAsync(item, true, cancellationToken).ConfigureAwait(true);
        }

        _items.Remove(item);
    }
}