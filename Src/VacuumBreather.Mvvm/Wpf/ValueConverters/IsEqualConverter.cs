using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a value to <see langword="true"/> if it is equal to the provided parameter value.</summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(Enum), typeof(bool))]
public class IsEqualConverter : ConverterBase
{
    /// <summary>Converts a value to <see langword="true"/> if it is equal to the provided parameter value.</summary>
    /// <param name="value">The value as produced by source binding.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns><see langword="true"/> if the value is equal to the provided parameter; otherwise, <see langword="false"/>.</returns>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return AreEqual(value, parameter);
    }

    private static bool AreEqual<T>(T first, T second)
    {
        return EqualityComparer<T>.Default.Equals(first, second);
    }
}