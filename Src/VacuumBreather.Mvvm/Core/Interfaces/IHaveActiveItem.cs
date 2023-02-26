using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an instance which maintains an active item.</summary>
[PublicAPI]
public interface IHaveActiveItem : IHaveReadOnlyActiveItem
{
    /// <summary>Gets or sets the currently active item.</summary>
    new object? ActiveItem { get; set; }
}

/// <summary>Denotes an instance which maintains an active item.</summary>
/// <typeparam name="T">The type of the active item.</typeparam>
[PublicAPI]
public interface IHaveActiveItem<T> : IHaveReadOnlyActiveItem<T>, IHaveActiveItem
{
    /// <summary>Gets or sets the currently active item.</summary>
    new T? ActiveItem { get; set; }
}