using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A circular <see cref="ITransition"/> effect.</summary>
/// <seealso cref="TransitionBase"/>
/// <seealso cref="ITransition"/>
[PublicAPI]
public class CircleTransition : TransitionBase
{
    private const string ScaleTransformName = "__scaleTransform";

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="effectSubject"/> is <see langword="null"/>.</exception>
    public override Timeline Build<TSubject>(TSubject effectSubject)
    {
        Guard.IsNotNull(effectSubject);

        // Calculate the radius of the clipping circle.
        var horizontalProportion = Math.Max(1.0 - Origin.X, Origin.X);
        var verticalProportion = Math.Max(1.0 - Origin.Y, Origin.Y);

        var radius = (float)Math.Sqrt(Math.Pow(effectSubject.ActualWidth * horizontalProportion, y: 2) +
                                      Math.Pow(effectSubject.ActualHeight * verticalProportion, y: 2));

        // Set up the clipping circle geometry and its transforms.
        var scaleTransform = new ScaleTransform(scaleX: 0, scaleY: 0);

        var translateTransform = new TranslateTransform(
            effectSubject.ActualWidth * Origin.X,
            effectSubject.ActualHeight * Origin.Y);

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(scaleTransform);
        transformGroup.Children.Add(translateTransform);

        var circleGeometry = new EllipseGeometry { RadiusX = radius, RadiusY = radius, Transform = transformGroup };

        // Register the scale transformation so we can access it from the timeline.
        effectSubject.GetNameScopeRoot().RegisterName(ScaleTransformName, scaleTransform);

        var subjectDelay = GetTotalSubjectDelay(effectSubject);

        var zeroFrame = new DiscreteDoubleKeyFrame(value: 0, TimeSpan.Zero);
        var startFrame = new DiscreteDoubleKeyFrame(value: 0, subjectDelay + Delay);

        var endFrame =
            new EasingDoubleKeyFrame(value: 1, subjectDelay + Delay + Duration) { EasingFunction = EasingFunction };

        var scaleXAnimation = new DoubleAnimationUsingKeyFrames();
        scaleXAnimation.KeyFrames.Add(zeroFrame);
        scaleXAnimation.KeyFrames.Add(startFrame);
        scaleXAnimation.KeyFrames.Add(endFrame);

        var scaleYAnimation = new DoubleAnimationUsingKeyFrames();
        scaleYAnimation.KeyFrames.Add(zeroFrame);
        scaleYAnimation.KeyFrames.Add(startFrame);
        scaleYAnimation.KeyFrames.Add(endFrame);

        var timeline = new ParallelTimeline();
        timeline.Children.Add(scaleXAnimation);
        timeline.Children.Add(scaleYAnimation);
        timeline.Completed += (_, _) => Cancel(effectSubject);

        effectSubject.SetCurrentValue(UIElement.ClipProperty, circleGeometry);

        Storyboard.SetTargetName(scaleXAnimation, ScaleTransformName);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTargetName(scaleYAnimation, ScaleTransformName);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        return timeline;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="effectSubject"/> is <see langword="null"/>.</exception>
    public override void Cancel<TSubject>(TSubject effectSubject)
    {
        Guard.IsNotNull(effectSubject);

        effectSubject.SetCurrentValue(UIElement.ClipProperty, value: null);
        effectSubject.GetNameScopeRoot().UnregisterName(ScaleTransformName);
    }
}