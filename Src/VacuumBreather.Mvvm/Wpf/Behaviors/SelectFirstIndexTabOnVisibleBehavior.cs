using System.Windows;
using System.Windows.Controls.Primitives;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Behaviors;

/// <summary>Used on <see cref="Selector"/> controls to select the first index whenever the control becomes visible.</summary>
/// <seealso cref="BehaviorBase{T}"/>
/// <seealso cref="Microsoft.Xaml.Behaviors.Behavior{T}"/>
[PublicAPI]
public sealed class SelectFirstIndexTabOnVisibleBehavior : BehaviorBase<Selector>
{
    /// <inheritdoc/>
    protected override void OnCleanup()
    {
        AssociatedObject.IsVisibleChanged -= OnAssociatedObjectIsVisibleChanged;
    }

    /// <inheritdoc/>
    protected override void OnSetup()
    {
        AssociatedObject.IsVisibleChanged += OnAssociatedObjectIsVisibleChanged;
    }

    private void OnAssociatedObjectIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        AssociatedObject.SetCurrentValue(Selector.SelectedIndexProperty, value: 0);
    }
}