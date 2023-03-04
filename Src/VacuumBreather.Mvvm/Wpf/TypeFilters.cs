using System;
using System.ComponentModel;
using System.Windows;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A helper class providing filters to check if a type is a view or a view-model.</summary>
[PublicAPI]
public static class TypeFilters
{
    /// <summary>
    ///     Gets or sets a filter which checks if a type is a valid view-model type.
    /// </summary>
    /// <value>
    ///     A filter which checks if a type is a valid view-model type.
    /// </value>
    public static Predicate<Type> IsViewModelType { get; set; } = type =>
        IsValidType(type) &&
        type.IsDerivedFromOrImplements(typeof(INotifyPropertyChanged)) &&
        !type.IsDerivedFromOrImplements(typeof(IDialogService)) &&
        type.Name.EndsWith("ViewModel", StringComparison.InvariantCulture);

    /// <summary>
    ///     Gets or sets a filter which checks if a type is a valid view type.
    /// </summary>
    /// <value>
    ///     A filter which checks if a type is a valid view type.
    /// </value>
    public static Predicate<Type> IsViewType { get; set; } = type =>
        IsValidType(type) &&
        type.IsDerivedFromOrImplements(typeof(FrameworkElement)) &&
        !type.IsDerivedFromOrImplements(typeof(Window)) &&
        type.Name.EndsWith("View", StringComparison.InvariantCulture);

    /// <summary>
    ///     Determines whether a type is a valid type for a view or view-model check. A type is invalid if it is generic,
    ///     abstract, nested or an interface.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>
    ///     <see langword="true" /> if a type is a valid type for a view or view-model check; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool IsValidType(Type type) =>
        type is
        {
            IsPublic: true,
            IsGenericType: false,
            IsInterface: false,
            IsAbstract: false,
            IsNested: false
        };
}