using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>Represents the direction of a <see cref="SlideTransition"/>.</summary>
[PublicAPI]
public enum SlideDirection
{
    /// <summary>The content is sliding to the left.</summary>
    Left,

    /// <summary>The content is sliding to the right.</summary>
    Right,

    /// <summary>The content is sliding up.</summary>
    Up,

    /// <summary>The content is sliding down.</summary>
    Down,
}