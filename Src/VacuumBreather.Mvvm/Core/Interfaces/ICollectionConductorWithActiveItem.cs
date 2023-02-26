using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     An <see cref="ICollectionConductor{T}" /> that also implements <see cref="IHaveActiveItem{T}" />.
/// </summary>
/// <typeparam name="T">The type of item to conduct.</typeparam>
[PublicAPI]
public interface ICollectionConductorWithActiveItem<T> : ICollectionConductor<T>, IConductActiveItem<T>
{
}