using System;
using System.Windows;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A directional slide <see cref="ITransitionWipe"/>.</summary>
/// <seealso cref="TransitionWipeBase"/>
/// <seealso cref="ITransitionWipe"/>
[PublicAPI]
public class SlideWipe : TransitionWipeBase
{
    private readonly SlideTransition _slideInTransition = new();
    private readonly SlideTransition _slideOutTransition = new() { Reverse = true };

    /// <summary>
    ///     Gets or sets the direction of the slide wipe transition, i.e. the direction the new content is sliding
    ///     towards.
    /// </summary>
    /// <value>The direction of the slide wipe transition.</value>
    public SlideDirection Direction { get; set; }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="fromItem"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="toItem"/> is <see langword="null"/>.</exception>
    protected override void ConfigureItems(TransitionerItem fromItem, TransitionerItem toItem, Point origin)
    {
        Guard.IsNotNull(fromItem);
        Guard.IsNotNull(toItem);

        fromItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _slideOutTransition);
        _slideOutTransition.Direction = GetOppositeDirection(Direction);
        _slideOutTransition.Duration = Duration;
        _slideOutTransition.Delay = Delay;
        _slideOutTransition.EasingFunction = EasingFunction;

        toItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _slideInTransition);
        _slideInTransition.Direction = Direction;
        _slideInTransition.Duration = Duration;
        _slideInTransition.Delay = Delay;
        _slideInTransition.EasingFunction = EasingFunction;
    }

    private static SlideDirection GetOppositeDirection(SlideDirection direction)
    {
        return direction switch
        {
            SlideDirection.Down => SlideDirection.Up,
            SlideDirection.Left => SlideDirection.Right,
            SlideDirection.Up => SlideDirection.Down,
            SlideDirection.Right => SlideDirection.Left,
            var _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
    }
}