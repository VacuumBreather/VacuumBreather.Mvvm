using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Contains implementations of <see cref="IConductor"/> that hold on many items.</summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
public class ConductorCollectionAllActive<T> : ConductorBase<T>, ICollectionConductor<T>
    where T : class
{
    private readonly bool _conductPublicItems;

    private readonly BindableCollection<T> _items = new();

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="VacuumBreather.Mvvm.Core.ConductorCollectionAllActive{T}"/> class.
    /// </summary>
    /// <param name="conductPublicItems">
    ///     If set to <see langword="true"/> public items that are properties of this class will
    ///     be conducted.
    /// </param>
    public ConductorCollectionAllActive(bool conductPublicItems)
        : this()
    {
        _conductPublicItems = conductPublicItems;
    }

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="VacuumBreather.Mvvm.Core.ConductorCollectionAllActive{T}"/> class.
    /// </summary>
    public ConductorCollectionAllActive()
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
            await ScreenExtensions.TryActivateAsync(item, cancellationToken);
        }

        OnActivationProcessed(item, success: true);
    }

    /// <inheritdoc/>
    public override async ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
    {
        ICloseResult<T> closeResult = await CloseStrategy.ExecuteAsync(_items.ToList(), cancellationToken);

        if (closeResult.CloseCanOccur || !closeResult.Children.Any())
        {
            return closeResult.CloseCanOccur;
        }

        foreach (var deactivate in closeResult.Children.OfType<IDeactivate>())
        {
            await deactivate.DeactivateAsync(close: true, cancellationToken);
        }

        _items.RemoveRange(closeResult.Children);

        return closeResult.CloseCanOccur;
    }

    /// <inheritdoc/>
    public override ValueTask DeactivateItemAsync(T item, bool close, CancellationToken cancellationToken = default)
    {
        return this.DeactivateItemAsync(item, close, CloseItemCoreAsync, cancellationToken);
    }

    /// <inheritdoc/>
    protected override T? EnsureItem(T? newItem)
    {
        if (newItem is null)
        {
            return base.EnsureItem(newItem);
        }

        var index = _items.IndexOf(newItem);

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

    /// <inheritdoc/>
    protected override async ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        foreach (var activate in _items.OfType<IActivate>())
        {
            await activate.ActivateAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
    {
        foreach (var deactivate in _items.OfType<IDeactivate>())
        {
            await deactivate.DeactivateAsync(close, cancellationToken);
        }

        if (close)
        {
            _items.Clear();
        }
    }

    /// <inheritdoc/>
    protected override async ValueTask OnInitializeAsync(CancellationToken cancellationToken)
    {
        if (_conductPublicItems)
        {
            var publicItems = GetType()
                              .GetTypeInfo()
                              .DeclaredProperties
                              .Where(propertyInfo =>
                                         !string.Equals(propertyInfo.Name, nameof(Parent), StringComparison.Ordinal) &&
                                         typeof(T).GetTypeInfo()
                                                  .IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
                              .Select(propertyInfo => propertyInfo.GetValue(this, index: null))
                              .Cast<T>()
                              .ToList();

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                foreach (var item in publicItems)
                {
                    Logger.LogDebug(message: "Will conduct public item: {Item}", item);
                }
            }

            await Task.WhenAll(publicItems.Select(item => ActivateItemAsync(item, cancellationToken))
                                          .Select(t => t.AsTask()));
        }
    }

    private async ValueTask CloseItemCoreAsync(T item, CancellationToken cancellationToken = default)
    {
        await ScreenExtensions.TryDeactivateAsync(item, close: true, cancellationToken);

        _items.Remove(item);
    }
}