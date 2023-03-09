using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a boolean value to its inverse value.</summary>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(bool), typeof(bool))]
public class BoolInversionConverter : MarkupExtension, IValueConverter
{
    private static readonly IValueConverter Instance = new BoolInversionConverter();

    /// <summary>Returns the singleton instance of this converter.</summary>
    /// <param name="serviceProvider">Unused by this markup extension.</param>
    /// <returns>The singleton instance of this converter.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Instance;
    }

    /// <summary>Converts a boolean value to its inverse value.</summary>
    /// <param name="value">The initial boolean value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>A converted value. If the method returns <see langword="null"/>, the valid null value is used.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool booleanValue)
        {
            return DependencyProperty.UnsetValue;
        }

        return !booleanValue;
    }

    /// <summary>Converts the inverse of a a boolean value back to the original value.</summary>
    /// <param name="value">The initial boolean value produced by the binding source.</param>
    /// <param name="targetType">The type to convert to. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>A converted value. If the method returns <see langword="null"/>, the valid null value is used.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}