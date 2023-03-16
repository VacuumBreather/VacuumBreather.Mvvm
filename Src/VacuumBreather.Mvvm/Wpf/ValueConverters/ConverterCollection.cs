using System.Windows.Data;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.ValueConverters;

/// <summary>A collection of value converters.</summary>
/// <seealso cref="VacuumBreather.Mvvm.Core.BindableCollection{IValueConverter}"/>
[PublicAPI]
public class ConverterCollection : BindableCollection<IValueConverter>
{
}