using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>Defines a type which can stack the z-index of a series of <see cref="TransitionerItem"/> objects.</summary>
[PublicAPI]
public interface IZIndexController
{
    /// <summary>Stacks the specified items by z-index.</summary>
    /// <param name="highestToLowest">The items which should be stacked, ordered from highest to lowest.</param>
    void Stack(params TransitionerItem[] highestToLowest);
}