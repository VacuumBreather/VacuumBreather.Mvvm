using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>
///     A converter which converts any non-zero number to <see langword="true"/> and any other number to
///     <see langword="false"/>.
/// </summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(double), typeof(bool))]
public class NotZeroConverter : ConverterBase
{
    /// <inheritdoc/>
    public override object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (double.TryParse((value ?? string.Empty).ToString(),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var val))
        {
            return Math.Abs(val) > 0.0;
        }

        Logger.LogDebug(message: "Error during not-zero conversion. Value could not be parsed. ({Value})", value);

        return DependencyProperty.UnsetValue;
    }
}