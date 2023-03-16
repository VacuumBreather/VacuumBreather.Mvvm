using System.Windows;
using System.Windows.Media;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Provides extension methods for the <see cref="FrameworkElement"/> type.</summary>
public static class FrameworkElementExtensions
{
    /// <summary>Gets or creates a valid name scope for this root element.</summary>
    /// <param name="rootElement">The root element.</param>
    /// <returns>A valid name scope for this root element.</returns>
    public static FrameworkElement GetNameScopeRoot(this FrameworkElement rootElement)
    {
        // Only set the name scope if the child does not already have a template XAML name scope set.
        if ((VisualTreeHelper.GetChildrenCount(rootElement) > 0) &&
            VisualTreeHelper.GetChild(rootElement, childIndex: 0) is FrameworkElement frameworkElement &&
            (NameScope.GetNameScope(frameworkElement) != null))
        {
            return frameworkElement;
        }

        if (NameScope.GetNameScope(rootElement) is null)
        {
            NameScope.SetNameScope(rootElement, new NameScope());
        }

        return rootElement;
    }
}