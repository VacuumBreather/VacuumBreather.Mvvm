using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a value by running it through a collection of converters in a chain.</summary>
/// <seealso cref="ConverterBase"/>
[PublicAPI]
[ValueConversion(typeof(object), typeof(object))]
public class ChainConverter : ConverterBase
{
    /// <summary>Initializes a new instance of the <see cref="ChainConverter"/> class.</summary>
    public ChainConverter()
    {
        Converters ??= new ConverterCollection();
    }

    /// <summary>Gets the collection of value converters that should be chained.</summary>
    public ConverterCollection Converters { get; init; }

    /// <summary>Converts a value by running it through a collection of converters in a chain.</summary>
    /// <param name="value">The value as produced by source binding.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>The converted value.</returns>
    public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var convertedValue = value;

        foreach (var converter in Converters)
        {
            convertedValue = converter.Convert(convertedValue, targetType, parameter, culture);
        }

        return convertedValue;
    }
}