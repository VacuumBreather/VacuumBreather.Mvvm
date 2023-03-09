using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts the empty state of a collection to a visibility.</summary>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(ICollection), typeof(Visibility))]
public class CollectionNullOrEmptyToVisibilityConverter : IValueConverter
{
    /// <summary>Gets or sets the visibility that a non-null or empty collection is converted to.</summary>
    public Visibility NotNullOrEmptyVisibility { get; set; } = Visibility.Visible;

    /// <summary>Gets or sets the visibility that a null or empty collection is converted to.</summary>
    public Visibility NullOrEmptyVisibility { get; set; } = Visibility.Collapsed;

    /// <summary>Converts the empty state of a collection to a visibility.</summary>
    /// <param name="value">The collection.</param>
    /// <param name="targetType">The type of the binding target property. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>
    ///     <see cref="NullOrEmptyVisibility"/> is the collection is null or empty; otherwise,
    ///     <see cref="NotNullOrEmptyVisibility"/>.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ICollection enumerable)
        {
            return DependencyProperty.UnsetValue;
        }

        return enumerable.Count == 0 ? NullOrEmptyVisibility : NotNullOrEmptyVisibility;
    }

    /// <summary>Backwards conversion is not supported by this converter.</summary>
    /// <param name="value">The value that is produced by the binding target. Not used by this converter.</param>
    /// <param name="targetType">The type to convert to. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>A converted value. If the method returns <see langword="null"/>, the valid null value is used.</returns>
    /// <exception cref="System.NotSupportedException">Backwards conversion is not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException(message: "Backwards conversion is not supported.");
    }
}