﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Jace;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a mathematical expression into its result.</summary>
/// <remarks>See https://github.com/pieterderycke/Jace/wiki/Getting-Started for supported expressions.</remarks>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
/// <seealso cref="System.Windows.Data.IMultiValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(object), typeof(object))]
public class MathExpressionConverter : ConverterBase, IMultiValueConverter
{
    private static readonly IDictionary<string, Func<IDictionary<string, double>, double>> CachedFormulas =
        new Dictionary<string, Func<IDictionary<string, double>, double>>(StringComparer.Ordinal);

    private static readonly CalculationEngine CalculationEngine = new(CultureInfo.InvariantCulture);

    private static readonly Dictionary<string, double> Parameters = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string expression || string.IsNullOrEmpty(expression))
        {
            return DependencyProperty.UnsetValue;
        }

        try
        {
            return CalculateResult(new[] { value }, expression);
        }
        catch (ArgumentException exception)
        {
            Logger.LogDebug(exception,
                            message: "Error during math expression conversion. ({Value}, {Parameter})",
                            value,
                            parameter);

            return DependencyProperty.UnsetValue;
        }
    }

    /// <inheritdoc/>
    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string expression || string.IsNullOrEmpty(expression) || values is null)
        {
            return DependencyProperty.UnsetValue;
        }

        try
        {
            return CalculateResult(values, expression);
        }
        catch (ArgumentException exception)
        {
            Logger.LogDebug(exception,
                            message: "Error during math expression conversion. ({Values}, {Parameter})",
                            values,
                            parameter);

            return DependencyProperty.UnsetValue;
        }
    }

    /// <inheritdoc/>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException(message: "Backwards conversion is not supported.");
    }

    private static object CalculateResult(object[] values, string expression)
    {
        object result;
        var doubleValues = ToValidDoubles(values).ToArray();

        if (!doubleValues.Any(double.IsNaN))
        {
            // Plan A - All values are doubles.
            Parameters.Clear();

            foreach (var (val, i) in doubleValues.Select((val, i) => (val, i)))
            {
                Parameters.Add($"var{i}", val);
            }

            var formula = GetFormula(values, expression);

            result = formula.Invoke(Parameters);
        }
        else
        {
            // Plan B - Values might be interpretable strings.
            //          Rebuild the expression by directly inserting the values.
            expression = string.Format(CultureInfo.InvariantCulture, expression, values);

            result = CalculationEngine.Calculate(expression);
        }

        return result;
    }

    private static string CreateVariableExpression(IEnumerable<object> values, string expression)
    {
        return string.Format(CultureInfo.InvariantCulture,
                             expression,
                             values.Select((_, i) => $"var{i}").Cast<object>().ToArray());
    }

    private static Func<IDictionary<string, double>, double> GetFormula(IEnumerable<object> values, string expression)
    {
        var variableExpression = CreateVariableExpression(values, expression);

        if (!CachedFormulas.TryGetValue(variableExpression, out var formula))
        {
            // Build formula and cache it.
            formula = CalculationEngine.Build(variableExpression);
            CachedFormulas[variableExpression] = formula;
        }

        return formula;
    }

    private static double ToValidDouble(object value)
    {
        if (value is double d1)
        {
            return d1;
        }

        return double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d2)
                   ? d2
                   : double.NaN;
    }

    private static IEnumerable<double> ToValidDoubles(IEnumerable<object> values)
    {
        return values.Select(ToValidDouble);
    }
}