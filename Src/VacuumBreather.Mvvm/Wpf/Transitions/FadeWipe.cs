using System;
using System.Windows;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A <see cref="ITransitionWipe"/> that fades out the previous content and fades in the new content.</summary>
/// <seealso cref="VacuumBreather.Mvvm.Wpf.Transitions.TransitionWipeBase"/>
/// <seealso cref="TransitionWipeBase"/>
/// <seealso cref="ITransitionWipe"/>
[PublicAPI]
public class FadeWipe : TransitionWipeBase
{
    private readonly ITransition _fadeInTransition = new FadeInTransition();
    private readonly ITransition _fadeOutTransition = new FadeOutTransition();

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="fromItem"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="toItem"/> is <see langword="null"/>.</exception>
    protected override void ConfigureItems(TransitionerItem fromItem, TransitionerItem toItem, Point origin)
    {
        Guard.IsNotNull(fromItem);
        Guard.IsNotNull(toItem);

        fromItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _fadeOutTransition);
        fromItem.TransitionEffect!.Duration = Duration;
        fromItem.TransitionEffect!.Delay = Delay;
        fromItem.TransitionEffect!.EasingFunction = EasingFunction;

        toItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _fadeInTransition);
        toItem.TransitionEffect!.Duration = Duration;
        toItem.TransitionEffect!.Delay = Delay;
        toItem.TransitionEffect!.EasingFunction = EasingFunction;
    }
}