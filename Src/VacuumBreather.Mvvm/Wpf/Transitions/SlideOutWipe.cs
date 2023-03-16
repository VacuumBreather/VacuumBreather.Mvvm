using System;
using System.Windows;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>
///     A <see cref="ITransitionWipe"/> which shrinks and fades out the old content while sliding in the new one from
///     the bottom.
/// </summary>
/// <seealso cref="TransitionWipeBase"/>
/// <seealso cref="ITransitionWipe"/>
[PublicAPI]
public class SlideOutWipe : TransitionWipeBase
{
    private readonly ITransition _shrinkOutTransition = new ShrinkOutTransition();
    private readonly ITransition _slideUpTransition = new SlideTransition { Direction = SlideDirection.Up };

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="fromItem"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="toItem"/> is <see langword="null"/>.</exception>
    protected override void ConfigureItems(TransitionerItem fromItem, TransitionerItem toItem, Point origin)
    {
        Guard.IsNotNull(fromItem);
        Guard.IsNotNull(toItem);

        fromItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _shrinkOutTransition);
        _shrinkOutTransition.Duration = Duration;
        _shrinkOutTransition.Delay = Delay;
        _shrinkOutTransition.EasingFunction = EasingFunction;

        toItem.SetCurrentValue(TransitionSubjectBase.TransitionEffectProperty, _slideUpTransition);
        _slideUpTransition.Duration = TimeSpan.FromTicks((long)(Duration.Ticks / 2.0));
        _slideUpTransition.Delay = Delay + TimeSpan.FromTicks((long)(Duration.Ticks / 2.0));
        _slideUpTransition.EasingFunction = EasingFunction;
    }
}