using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts an object to a visibility based on whether it is null or not.</summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : ConverterBase
{
    /// <summary>Gets or sets the visibility that a non-null object is converted to.</summary>
    public Visibility NotNullVisibility { get; set; } = Visibility.Visible;

    /// <summary>Gets or sets the visibility that null is converted to.</summary>
    public Visibility NullVisibility { get; set; } = Visibility.Collapsed;

    /// <summary>Converts the null state of an object to a visibility.</summary>
    /// <param name="value">The object.</param>
    /// <param name="targetType">The type of the binding target property. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns><see cref="NullVisibility"/> if the object is null; otherwise, <see cref="NotNullVisibility"/>.</returns>
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? NullVisibility : NotNullVisibility;
    }
}