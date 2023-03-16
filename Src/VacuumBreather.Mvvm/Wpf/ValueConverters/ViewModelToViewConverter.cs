using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>Converts a view-model into the associated view and establishes a data binding.</summary>
/// <seealso cref="ConverterBase"/>
/// <seealso cref="System.Windows.Data.IValueConverter"/>
[PublicAPI]
[ValueConversion(typeof(BindableObject), typeof(FrameworkElement))]
public class ViewModelToViewConverter : ConverterBase
{
    /// <summary>Converts a view-model into the associated view and establishes a data binding.</summary>
    /// <param name="value">The view-model.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use. Not used by this converter.</param>
    /// <param name="culture">The culture to use in the converter. Not used by this converter.</param>
    /// <returns>
    ///     The view associated with the provided view-model. If the method returns <see langword="null"/>, the valid null
    ///     value is used.
    /// </returns>
    public override object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!targetType.IsDerivedFromOrImplements(typeof(FrameworkElement)))
        {
            return DependencyProperty.UnsetValue;
        }

        if (value is not BindableObject viewModel)
        {
            return DependencyProperty.UnsetValue;
        }

        if (ServiceProvider?.GetService(typeof(ViewLocator)) is not ViewLocator viewLocator)
        {
            return DependencyProperty.UnsetValue;
        }

        return viewLocator.LocateViewForViewModel(viewModel, SetDataContext);
    }

    private static void SetDataContext(object viewModel, FrameworkElement view)
    {
        view.DataContext = viewModel;
    }
}