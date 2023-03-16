using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a <see cref="System.DateTime"/> to a general date/time pattern (short time) string.</summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(DateTime), typeof(string))]
public class DateTimeToLocalizedStringConverter : ConverterBase
{
    /// <inheritdoc/>
    [SuppressMessage(category: "Security",
                     checkId: "MA0009:Add regex evaluation timeout",
                     Justification = "Not security relevant.")]
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime)
        {
            Logger.LogDebug(message: "Error during date time string conversion. Value is not a DateTime. ({Value})",
                            value);

            return DependencyProperty.UnsetValue;
        }

        var monthDayPattern = culture.DateTimeFormat.MonthDayPattern;
        var timePattern = culture.DateTimeFormat.ShortTimePattern;

        var fullPattern =
            monthDayPattern.IndexOf(value: 'M', StringComparison.Ordinal) <
            monthDayPattern.IndexOf(value: 'd', StringComparison.Ordinal)
                ? "MM/dd/yyyy hh:mm"
                : "dd/MM/yyyy hh:mm";

        if (timePattern.Contains(value: "tt", StringComparison.Ordinal))
        {
            fullPattern += " tt";
        }

        return dateTime.ToString(fullPattern, culture);
    }
}