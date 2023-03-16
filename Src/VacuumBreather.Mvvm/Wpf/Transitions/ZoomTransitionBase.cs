using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A <see cref="ITransition"/> effect which zooms and fades the <see cref="ITransitionSubject"/>.</summary>
/// <seealso cref="TransitionBase"/>
/// <seealso cref="ITransition"/>
[PublicAPI]
public abstract class ZoomTransitionBase : TransitionBase
{
    private readonly double _endOpacity;
    private readonly double _endScale;

    private readonly double _startOpacity;
    private readonly double _startScale;

    private ScaleTransform? _scaleTransform;

    /// <summary>Initializes a new instance of the <see cref="ZoomTransitionBase"/> class.</summary>
    /// <param name="startScale">The scale at the start of the effect.</param>
    /// <param name="endScale">The scale at the end of the effect.</param>
    /// <param name="startOpacity">The opacity at the start of the effect.</param>
    /// <param name="endOpacity">The opacity at the end of the effect.</param>
    protected ZoomTransitionBase(double startScale, double endScale, double startOpacity, double endOpacity)
    {
        _startScale = startScale;
        _endScale = endScale;
        _startOpacity = startOpacity;
        _endOpacity = endOpacity;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="effectSubject"/> is <see langword="null"/>.</exception>
    public override Timeline? Build<TSubject>(TSubject effectSubject)
    {
        Guard.IsNotNull(effectSubject);

        if (!(effectSubject.GetNameScopeRoot().FindName(effectSubject.ScaleTransformName) is ScaleTransform transform))
        {
            return null;
        }

        _scaleTransform = transform;

        var subjectDelay = GetTotalSubjectDelay(effectSubject);

        var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
        var startKeyTime = KeyTime.FromTimeSpan(subjectDelay + Delay);
        var endKeyTime = KeyTime.FromTimeSpan(subjectDelay + Delay + Duration);

        var scaleXAnimation = new DoubleAnimationUsingKeyFrames();
        scaleXAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(_startScale, zeroKeyTime));
        scaleXAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(_startScale, startKeyTime));
        scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(_endScale, endKeyTime, EasingFunction));

        var scaleYAnimation = new DoubleAnimationUsingKeyFrames();
        scaleYAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(_startScale, zeroKeyTime));
        scaleYAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(_startScale, startKeyTime));
        scaleYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(_endScale, endKeyTime, EasingFunction));

        var opacityAnimation = new DoubleAnimationUsingKeyFrames();
        opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(_startOpacity, zeroKeyTime));
        opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(_startOpacity, startKeyTime));
        opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(_endOpacity, endKeyTime, EasingFunction));

        var timeline = new ParallelTimeline();
        timeline.Children.Add(scaleXAnimation);
        timeline.Children.Add(scaleYAnimation);
        timeline.Children.Add(opacityAnimation);
        timeline.Completed += (_, _) => Cancel(effectSubject);

        _scaleTransform.SetCurrentValue(ScaleTransform.ScaleXProperty, _startScale);
        _scaleTransform.SetCurrentValue(ScaleTransform.ScaleYProperty, _startScale);

        Storyboard.SetTargetName(scaleXAnimation, effectSubject.ScaleTransformName);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTargetName(scaleYAnimation, effectSubject.ScaleTransformName);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        Storyboard.SetTarget(opacityAnimation, effectSubject);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

        return timeline;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="effectSubject"/> is <see langword="null"/>.</exception>
    public override void Cancel<TSubject>(TSubject effectSubject)
    {
        Guard.IsNotNull(effectSubject);

        effectSubject.SetCurrentValue(UIElement.OpacityProperty, value: 1.0);

        if (_scaleTransform is null)
        {
            return;
        }

        _scaleTransform.SetCurrentValue(ScaleTransform.ScaleXProperty, value: 1.0);
        _scaleTransform.SetCurrentValue(ScaleTransform.ScaleYProperty, value: 1.0);
        _scaleTransform = null;
    }
}