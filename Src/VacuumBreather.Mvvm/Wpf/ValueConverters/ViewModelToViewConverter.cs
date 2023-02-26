using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>
///     Converts a view-model into the associated view and establishes a data binding.
/// </summary>
/// <seealso cref="System.Windows.Data.IValueConverter" />
[PublicAPI]
[ValueConversion(typeof(BindableObject), typeof(FrameworkElement))]
public class ViewModelToViewConverter : MarkupExtension, IValueConverter
{
    private static readonly IValueConverter Instance = new ViewModelToViewConverter();
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    ///     Returns the singleton instance of this converter.
    /// </summary>
    /// <param name="serviceProvider">Unused by this markup extension.</param>
    /// <returns>
    ///     The singleton instance of this converter.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Instance;
    }

    /// <summary>
    ///     Converts a view-model into the associated view and establishes a data binding.
    /// </summary>
    /// <param name="value">The view-model.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>
    ///     The view associated with the provided view-model. If the method returns <see langword="null" />, the valid null
    ///     value is used.
    /// </returns>
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!targetType.IsDerivedFromOrImplements(typeof(FrameworkElement)) || value is not BindableObject viewModel)
        {
            return DependencyProperty.UnsetValue;
        }

        _serviceProvider ??= GetServiceProvider();

        if (_serviceProvider?.GetService(typeof(ViewLocator)) is not ViewLocator viewLocator)
        {
            return DependencyProperty.UnsetValue;
        }

        return viewLocator.LocateViewForViewModel(viewModel, SetDataContext);
    }

    /// <summary>
    ///     Backwards conversion is not supported by this converter.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    ///     A converted value. If the method returns <see langword="null" />, the valid null value is used.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">Backwards conversion is not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("Backwards conversion is not supported.");
    }

    private static IServiceProvider? GetServiceProvider()
    {
        return Application.Current?.FindResource(typeof(IServiceProvider)) as IServiceProvider;
    }

    private static void SetDataContext(object viewModel, FrameworkElement view)
    {
        view.DataContext = viewModel;
    }
}