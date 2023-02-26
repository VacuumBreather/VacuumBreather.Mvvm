using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>
///     Converts a boolean value to a visibility.
/// </summary>
/// <seealso cref="System.Windows.Data.IValueConverter" />
[PublicAPI]
[ValueConversion(typeof(object), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    ///     Gets or sets the visibility that a boolean value of <see langword="false" /> is converted to.
    /// </summary>
    public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

    /// <summary>
    ///     Gets or sets the visibility that a boolean value of <see langword="true" /> is converted to.
    /// </summary>
    public Visibility TrueVisibility { get; set; } = Visibility.Visible;

    /// <summary>
    ///     Converts a boolean value to a visibility.
    /// </summary>
    /// <param name="value">The boolean.</param>
    /// <param name="targetType">The type of the binding target property. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>
    ///     <see cref="TrueVisibility" /> if the boolean is <see langword="true" />; otherwise, <see cref="FalseVisibility" />.
    /// </returns>
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? TrueVisibility : FalseVisibility;
    }

    /// <summary>
    ///     Backwards conversion is not supported by this converter.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to. Not used by this converter.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>
    ///     A converted value. If the method returns <see langword="null" />, the valid null value is used.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">Backwards conversion is not supported.</exception>
    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("Backwards conversion is not supported.");
    }
}