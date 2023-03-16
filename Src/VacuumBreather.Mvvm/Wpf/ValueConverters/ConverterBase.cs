using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Base class for value converters with access to logging and a <see cref="IServiceProvider"/>.</summary>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
public abstract class ConverterBase : IValueConverter
{
    private static readonly Lazy<ILogger> LazyLogger = new(GetLogger);
    private static readonly Lazy<IServiceProvider> LazyServiceProvider = new(GetServiceProvider);

    /// <summary>Gets the <see cref="ILogger"/> for this converter.</summary>
    protected static ILogger Logger => LazyLogger.Value;

    /// <summary>Gets the <see cref="IServiceProvider"/> for this converter.</summary>
    protected static IServiceProvider ServiceProvider => LazyServiceProvider.Value;

    /// <summary>
    ///     Converts a value back. Called when moving a value from target to source. This should implement the inverse of
    ///     <see cref="Convert"/>.
    /// </summary>
    /// <param name="value">The value as produced by target.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>
    ///     The converted back value. <see cref="Binding.DoNothing"/> may be returned to indicate that no value should be
    ///     set on the source property. <see cref="System.Windows.DependencyProperty.UnsetValue"/> may be returned to indicate
    ///     that the converter is unable to provide a value for the source property, and no value will be set to it.
    /// </returns>
    [SuppressMessage(category: "ReSharper",
                     checkId: "ReturnTypeCanBeNotNullable",
                     Justification = "Can be implemented as nullable by a derived type.")]
    public virtual object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException(message: "Backwards conversion is not supported.");
    }

    /// <summary>Converts a value. Called when moving a value from source to target.</summary>
    /// <param name="value">The value as produced by source binding</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>
    ///     The converted value. <see cref="System.Windows.DependencyProperty.UnsetValue"/> may be returned to indicate
    ///     that the converter produced no value and that the fallback (if available) or default value should be used instead.
    ///     <see cref="Binding.DoNothing"/> may be returned to indicate that the binding should not transfer the value or use
    ///     the fallback or default value.
    /// </returns>
    public abstract object? Convert(object value, Type targetType, object parameter, CultureInfo culture);

    private static ILogger GetLogger()
    {
        var logger = (ILogger?)ServiceProvider.GetService(typeof(ILogger<MathExpressionConverter>));

        return logger ?? NullLogger.Instance;
    }

    private static IServiceProvider GetServiceProvider()
    {
        var serviceProvider = (IServiceProvider?)Application.Current?.TryFindResource(nameof(IServiceProvider));

        return serviceProvider ?? NullProvider.Instance;
    }

    private sealed class NullProvider : IServiceProvider
    {
        internal static IServiceProvider Instance { get; } = new NullProvider();

        public object? GetService(Type serviceType)
        {
            return null;
        }
    }
}