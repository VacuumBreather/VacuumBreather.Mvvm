using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a enum flags value to a boolean value indicating whether the flag, given as a parameter, is set.</summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(Enum), typeof(bool))]
public class EnumFlagSetToBooleanConverter : ConverterBase
{
    /// <summary>Converts a enum flags value to a boolean value indicating whether the flag, given as a parameter, is set.</summary>
    /// <param name="value">The active enum flags.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The enum flag to check for.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns><see langword="true"/> if the parameter flag is set; otherwise, <see langword="false"/>.</returns>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Enum enumFlags || value.GetType().GetCustomAttribute(typeof(FlagsAttribute)) is null)
        {
            Logger.LogDebug(
                message: "Error during enum flags check conversion. Value is not a valid flags enum: ({Value})",
                value);

            return DependencyProperty.UnsetValue;
        }

        if (parameter is not Enum parameterFlag ||
            parameter.GetType().GetCustomAttribute(typeof(FlagsAttribute)) is null ||
            (parameter.GetType() != value.GetType()))
        {
            Logger.LogDebug(
                message:
                "Error during enum flags check conversion. Parameter is not a valid flag of the value enum: ({Parameter})",
                parameter);

            return DependencyProperty.UnsetValue;
        }

        return enumFlags.HasFlag(parameterFlag);
    }
}