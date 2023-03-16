using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts an integer to a visibility based on whether the integer is zero or not.</summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(int), typeof(Visibility))]
public class ZeroToVisibilityConverter : ConverterBase
{
    /// <summary>Gets or sets the visibility that ny non-zero integer is converted to.</summary>
    public Visibility NotZeroVisibility { get; set; } = Visibility.Visible;

    /// <summary>Gets or sets the visibility that an integer equal to zero is converted to.</summary>
    public Visibility ZeroVisibility { get; set; } = Visibility.Collapsed;

    /// <summary>Converts an integer to a visibility based on whether it is zero or not.</summary>
    /// <param name="value">The integer value.</param>
    /// <param name="targetType">The type of the binding target property. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns><see cref="ZeroVisibility"/> if the integer is equal to zero; otherwise, <see cref="NotZeroVisibility"/>.</returns>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int integer)
        {
            return integer == 0 ? ZeroVisibility : NotZeroVisibility;
        }

        Logger.LogDebug(message: "Error during integer zero check. Value is not an integer. ({Value})", value);

        return DependencyProperty.UnsetValue;
    }
}