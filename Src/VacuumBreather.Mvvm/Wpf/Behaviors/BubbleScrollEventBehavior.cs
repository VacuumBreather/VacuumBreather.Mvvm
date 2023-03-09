using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace VacuumBreather.Mvvm.Wpf.Behaviors;

/// <summary>Used on sub-controls of an expander to bubble the mouse wheel scroll event up.</summary>
/// <seealso cref="Microsoft.Xaml.Behaviors.Behavior{UIElement}"/>
public sealed class BubbleScrollEventBehavior : Behavior<UIElement>
{
    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseWheel += OnAssociatedObjectPreviewMouseWheel;
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseWheel -= OnAssociatedObjectPreviewMouseWheel;
        base.OnDetaching();
    }

    private void OnAssociatedObjectPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        var wheelEventArgs =
            new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent };

        AssociatedObject.RaiseEvent(wheelEventArgs);
    }
}