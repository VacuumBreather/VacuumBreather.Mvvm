using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>
///     Converter to debug data bindings. The value is not changed but allows setting a breakpoint and logs out the
///     value.
/// </summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(object), typeof(object))]
public class DebugConverter : ConverterBase
{
    /// <inheritdoc/>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Logger.LogDebug(message: "Value: {Value}", value);

        return value;
    }

    /// <inheritdoc/>
    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Convert(value, targetType, parameter, culture);
    }
}