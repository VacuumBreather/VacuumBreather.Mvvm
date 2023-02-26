using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     A base class for various implementations of <see cref="IConductor" /> that maintain an
///     active item.
/// </summary>
/// <typeparam name="T">The type that is being conducted.</typeparam>
public abstract class ConductorBaseWithActiveItem<T> : ConductorBase<T>, IConductActiveItem<T>
    where T : class
{
    private T? _activeItem;

    /// <summary>Gets or sets the currently active item.</summary>
    public T? ActiveItem
    {
        get => _activeItem;
        set => ActivateItemAsync(value, CancellationToken.None).Forget();
    }

    /// <inheritdoc />
    object? IHaveActiveItem.ActiveItem
    {
        get => ActiveItem;
        set => ActiveItem = (T?)value;
    }

    /// <inheritdoc />
    object? IHaveReadOnlyActiveItem.ActiveItem => ActiveItem;

    /// <summary>Changes the active item.</summary>
    /// <param name="newItem">The new item to activate.</param>
    /// <param name="closePrevious">Indicates whether or not to close the previous active item.</param>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous operation.</returns>
    protected virtual async ValueTask ChangeActiveItemAsync(T? newItem,
                                                            bool closePrevious,
                                                            CancellationToken cancellationToken)
    {
        await ScreenExtensions.TryDeactivateAsync(ActiveItem, closePrevious, cancellationToken).ConfigureAwait(true);

        newItem = EnsureItem(newItem);

        SetProperty(ref _activeItem, newItem, nameof(ActiveItem));

        if (IsActive)
        {
            await ScreenExtensions.TryActivateAsync(newItem, cancellationToken).ConfigureAwait(true);
        }

        if (newItem is not null)
        {
            OnActivationProcessed(newItem, true);
        }
    }
}