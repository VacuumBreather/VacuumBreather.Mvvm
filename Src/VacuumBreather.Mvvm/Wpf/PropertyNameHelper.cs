using System;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Helper class to get the name of a dependency property without using hardcoded strings.</summary>
public static class PropertyNameHelper
{
    /// <summary>Gets the name of a dependency property minus the "Property" part.</summary>
    /// <param name="dependencyPropertyName">The full name of the dependency property.</param>
    /// <returns>The name of a dependency property minus the "Property" part.</returns>
    public static string GetName(string dependencyPropertyName)
    {
        return dependencyPropertyName.Replace(oldValue: "Property", string.Empty, StringComparison.InvariantCulture);
    }

    /// <summary>Gets the name of a routed event minus the "Event" part.</summary>
    /// <param name="routedEventName">The full name of the outed event.</param>
    /// <returns>The name of a outed event minus the "Event" part.</returns>
    public static string GetRoutedEventName(string routedEventName)
    {
        return routedEventName.Replace(oldValue: "Event", string.Empty, StringComparison.InvariantCulture);
    }
}