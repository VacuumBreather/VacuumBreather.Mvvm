using System;
using System.Windows;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A <see cref="ITransitionWipe"/> which takes the shape of a growing circle.</summary>
/// <seealso cref="TransitionWipeBase"/>
/// <seealso cref="ITransitionWipe"/>
[PublicAPI]
public class CircleWipe : TransitionWipeBase
{
    private readonly ITransition _circleTransition = new CircleTransition();
    private readonly ITransition _fadeOutTransition = new FadeOutTransition();

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="fromItem"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="toItem"/> is <see langword="null"/>.</exception>
    protected override void ConfigureItems(TransitionerItem fromItem, TransitionerItem toItem, Point origin)
    {
        Guard.IsNotNull(fromItem);
        Guard.IsNotNull(toItem);

        fromItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _fadeOutTransition);
        fromItem.TransitionEffect!.Duration = TimeSpan.FromTicks((long)(Duration.Ticks / 2.0));
        fromItem.TransitionEffect!.Delay = Delay + TimeSpan.FromTicks((long)(Duration.Ticks / 2.0));
        fromItem.TransitionEffect!.EasingFunction = EasingFunction;

        toItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _circleTransition);
        toItem.TransitionEffect!.Duration = Duration;
        toItem.TransitionEffect!.Delay = Delay;
        toItem.TransitionEffect!.EasingFunction = EasingFunction;
        toItem.TransitionEffect!.Origin = origin;
    }
}