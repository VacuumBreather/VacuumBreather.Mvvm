using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a boolean value to its inverse value.</summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(bool), typeof(bool))]
public class BoolInversionConverter : ConverterBase
{
    /// <summary>Converts a boolean value to its inverse value.</summary>
    /// <param name="value">The initial boolean value produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>A converted value. If the method returns <see langword="null"/>, the valid null value is used.</returns>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return !booleanValue;
        }

        Logger.LogDebug(message: "Error during bool inversion conversion. Value is not a boolean. ({Value})", value);

        return DependencyProperty.UnsetValue;
    }

    /// <summary>Converts the inverse of a a boolean value back to the original value.</summary>
    /// <param name="value">The initial boolean value produced by the binding source.</param>
    /// <param name="targetType">The type to convert to. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>A converted value. If the method returns <see langword="null"/>, the valid null value is used.</returns>
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}