using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Behaviors;

/// <summary>Used on sub-controls of an expander to bubble the mouse wheel scroll event up.</summary>
/// <seealso cref="BehaviorBase{T}"/>
/// <seealso cref="Microsoft.Xaml.Behaviors.Behavior{T}"/>
[PublicAPI]
public sealed class BubbleScrollEventBehavior : BehaviorBase<FrameworkElement>
{
    /// <inheritdoc/>
    protected override void OnCleanup()
    {
        AssociatedObject.PreviewMouseWheel -= OnAssociatedObjectPreviewMouseWheel;
    }

    /// <inheritdoc/>
    protected override void OnSetup()
    {
        AssociatedObject.PreviewMouseWheel += OnAssociatedObjectPreviewMouseWheel;
    }

    private void OnAssociatedObjectPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        var wheelEventArgs =
            new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent };

        AssociatedObject.RaiseEvent(wheelEventArgs);
    }
}