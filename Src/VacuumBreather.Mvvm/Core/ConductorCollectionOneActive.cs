using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Contains implementations of <see cref="IConductor"/> that hold on many items.</summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
[PublicAPI]
public class ConductorCollectionOneActive<T> : ConductorBaseWithActiveItem<T>, ICollectionConductorWithActiveItem<T>
    where T : class
{
    private readonly BindableCollection<T> _items = new();

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="VacuumBreather.Mvvm.Core.ConductorCollectionOneActive{T}"/> class.
    /// </summary>
    public ConductorCollectionOneActive()
    {
        _items.AreChildrenOf(this);
    }

    /// <summary>Gets the items that are currently being conducted.</summary>
    public IBindableCollection<T> Items => _items;

    /// <inheritdoc/>
    public sealed override IEnumerable<T> GetChildren()
    {
        return _items;
    }

    /// <inheritdoc/>
    public override async ValueTask ActivateItemAsync(T? item, CancellationToken cancellationToken = default)
    {
        using var _ = AsyncGuard.GetToken();

        if (item?.Equals(ActiveItem) ?? false)
        {
            if (IsActive)
            {
                await ScreenExtensions.TryActivateAsync(item, cancellationToken);

                OnActivationProcessed(item, success: true);
            }

            return;
        }

        await ChangeActiveItemAsync(item, closePrevious: false, cancellationToken);
    }

    /// <inheritdoc/>
    public override async ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        using var _ = AsyncGuard.GetToken();

        ICloseResult<T> closeResult = await CloseStrategy.ExecuteAsync(_items.ToList(), cancellationToken);

        if (closeResult.CloseCanOccur || !closeResult.Children.Any())
        {
            return closeResult.CloseCanOccur;
        }

        List<T> closable = closeResult.Children.ToList();

        if (ActiveItem is not null && closable.Contains(ActiveItem))
        {
            List<T> list = _items.ToList();
            var next = ActiveItem;

            do
            {
                var previous = next;
                next = DetermineNextItemToActivate(list, list.IndexOf(previous));
                list.Remove(previous);
            } while (next is not null && closable.Contains(next));

            var previousActive = ActiveItem;

            await ChangeActiveItemAsync(next, closePrevious: true, cancellationToken);

            _items.Remove(previousActive);
            closable.Remove(previousActive);
        }

        foreach (var deactivate in closable.OfType<IDeactivate>())
        {
            await deactivate.DeactivateAsync(close: true, cancellationToken);
        }

        _items.RemoveRange(closable);

        return closeResult.CloseCanOccur;
    }

    /// <inheritdoc/>
    public override async ValueTask DeactivateItemAsync(T item,
                                                        bool close,
                                                        CancellationToken cancellationToken = default)
    {
        using var _ = AsyncGuard.GetToken();

        await this.DeactivateItemAsync(item, close, CloseItemCoreAsync, cancellationToken);
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

        var toRemoveAt = lastIndex - 1;

        if ((toRemoveAt == -1) && (list.Count > 1))
        {
            return list[index: 1];
        }

        if ((toRemoveAt > -1) && (toRemoveAt < list.Count - 1))
        {
            return list[toRemoveAt];
        }

        return default;
    }

    /// <inheritdoc/>
    protected override T? EnsureItem(T? newItem)
    {
        if (newItem == null)
        {
            newItem = DetermineNextItemToActivate(_items, ActiveItem != null ? _items.IndexOf(ActiveItem) : 0);
        }
        else
        {
            var index = _items.IndexOf(newItem);

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

    /// <inheritdoc/>
    protected override ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        return ScreenExtensions.TryActivateAsync(ActiveItem, cancellationToken);
    }

    /// <inheritdoc/>
    protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        if (close)
        {
            foreach (var deactivate in _items.OfType<IDeactivate>())
            {
                await deactivate.DeactivateAsync(close: true, cancellationToken);
            }

            _items.Clear();
        }
        else
        {
            await ScreenExtensions.TryDeactivateAsync(ActiveItem, close: false, cancellationToken);
        }
    }

    private async ValueTask CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
    {
        if (item.Equals(ActiveItem))
        {
            var index = _items.IndexOf(item);
            var next = DetermineNextItemToActivate(_items, index);

            await ChangeActiveItemAsync(next, closePrevious: true, cancellationToken);
        }
        else
        {
            await ScreenExtensions.TryDeactivateAsync(item, close: true, cancellationToken);
        }

        _items.Remove(item);
    }
}