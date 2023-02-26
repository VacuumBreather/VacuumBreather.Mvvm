using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Contains implementations of <see cref="IConductor" /> that hold on many items.</summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
public class ConductorCollectionAllActive<T> : ConductorBase<T>, ICollectionConductor<T>
    where T : class
{
    private readonly bool _conductPublicItems;

    private readonly BindableCollection<T> _items = new();

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="VacuumBreather.Mvvm.Core.ConductorCollectionAllActive{T}" /> class.
    /// </summary>
    /// <param name="conductPublicItems">
    ///     If set to <see langword="true" /> public items that are properties of this
    ///     class will be conducted.
    /// </param>
    public ConductorCollectionAllActive(bool conductPublicItems)
        : this()
    {
        _conductPublicItems = conductPublicItems;
    }

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="VacuumBreather.Mvvm.Core.ConductorCollectionAllActive{T}" /> class.
    /// </summary>
    public ConductorCollectionAllActive()
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
        if (item is null)
        {
            return;
        }

        item = EnsureItem(item);

        if (item is null)
        {
            return;
        }

        if (IsActive)
        {
            await ScreenExtensions.TryActivateAsync(item, cancellationToken).ConfigureAwait(true);
        }

        OnActivationProcessed(item, true);
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

        foreach (IDeactivate deactivate in closeResult.Children.OfType<IDeactivate>())
        {
            await deactivate.DeactivateAsync(true, cancellationToken).ConfigureAwait(true);
        }

        _items.RemoveRange(closeResult.Children);

        return closeResult.CloseCanOccur;
    }

    /// <inheritdoc />
    public override async ValueTask DeactivateItemAsync(T item,
                                                        bool close,
                                                        CancellationToken cancellationToken = default)
    {
        await this.DeactivateItemAsync(item, close, CloseItemCoreAsync, cancellationToken).ConfigureAwait(true);
    }

    /// <inheritdoc />
    protected override T? EnsureItem(T? newItem)
    {
        if (newItem is null)
        {
            return base.EnsureItem(newItem);
        }

        int index = _items.IndexOf(newItem);

        if (index == -1)
        {
            _items.Add(newItem);
        }
        else
        {
            newItem = _items[index];
        }

        return base.EnsureItem(newItem);
    }

    /// <inheritdoc />
    protected override async ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        foreach (IActivate activate in _items.OfType<IActivate>())
        {
            await activate.ActivateAsync(cancellationToken).ConfigureAwait(true);
        }
    }

    /// <inheritdoc />
    protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        foreach (IDeactivate deactivate in _items.OfType<IDeactivate>())
        {
            await deactivate.DeactivateAsync(close, cancellationToken).ConfigureAwait(true);
        }

        if (close)
        {
            _items.Clear();
        }
    }

    /// <inheritdoc />
    protected override async ValueTask OnInitializeAsync(CancellationToken cancellationToken)
    {
        if (_conductPublicItems)
        {
            var publicItems = GetType()
                              .GetTypeInfo()
                              .DeclaredProperties
                              .Where(propertyInfo =>
                                         (propertyInfo.Name != nameof(Parent)) &&
                                         typeof(T).GetTypeInfo()
                                                  .IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
                              .Select(propertyInfo => propertyInfo.GetValue(this, null))
                              .Cast<T>()
                              .ToList();

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var item in publicItems)
                {
                    Logger.LogDebug("Will conduct public item: {Item}", item);
                }
            }

            await Task.WhenAll(publicItems.Select(item => ActivateItemAsync(item, cancellationToken))
                                          .Select(t => t.AsTask()))
                      .ConfigureAwait(true);
        }
    }

    private async ValueTask CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
    {
        await ScreenExtensions.TryDeactivateAsync(item, true, cancellationToken).ConfigureAwait(true);

        _items.Remove(item);
    }
}