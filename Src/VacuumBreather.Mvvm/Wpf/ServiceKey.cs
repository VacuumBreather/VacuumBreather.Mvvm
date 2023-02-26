using System.Windows;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>
///     This enumeration can be used to provide dependency injection service keys for essential components during
///     registration.
/// </summary>
public enum ServiceKey
{
    /// <summary>
    ///     The key for the main <see cref="Window" />.
    /// </summary>
    MainView,

    /// <summary>
    ///     The key for the main view-model <see cref="BindableObject" />.
    /// </summary>
    MainViewModel,

    /// <summary>
    ///     The key for the <see cref="ResourceDictionary" /> containing the theme resources.
    /// </summary>
    ThemeResources,
}