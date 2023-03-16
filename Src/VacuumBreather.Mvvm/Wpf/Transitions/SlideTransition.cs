using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>
///     A <see cref="ITransition"/> effect which slides content from outside its containing area in the specified
///     direction into its resting position.
/// </summary>
/// <seealso cref="TransitionBase"/>
/// <seealso cref="ITransition"/>
[PublicAPI]
public class SlideTransition : TransitionBase
{
    private double _endX;
    private double _endY;
    private TranslateTransform? _translateTransform;

    /// <summary>
    ///     Gets or sets the name of the container element which defines the outer edges at which the slide transition
    ///     should start.
    /// </summary>
    /// <remarks>If this is not specified, the bounding box of the <see cref="ITransitionSubject"/> itself is used.</remarks>
    /// <value>The name of the container element which defines the outer edges at which the slide transition should start.</value>
    public string? ContainerElementName { get; set; }

    /// <summary>Gets or sets the direction of the transition.</summary>
    /// <value>The direction of the transition.</value>
    public SlideDirection Direction { get; set; }

    /// <summary>Gets or sets a value indicating whether this <see cref="SlideTransition"/> should be reversed.</summary>
    /// <value>
    ///     <see langword="true"/> if this <see cref="SlideTransition"/> is reversed, i.e. the content is moved out of the
    ///     frame instead; otherwise, <see langword="false"/>.
    /// </value>
    public bool Reverse { get; set; }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="effectSubject"/> is <see langword="null"/>.</exception>
    public override Timeline? Build<TSubject>(TSubject effectSubject)
    {
        Guard.IsNotNull(effectSubject);

        if (effectSubject.GetNameScopeRoot().FindName(effectSubject.TranslateTransformName) is not TranslateTransform
            transform)
        {
            return null;
        }

        _translateTransform = transform;

        var container = string.IsNullOrEmpty(ContainerElementName)
                            ? effectSubject
                            : effectSubject.FindName(ContainerElementName) as FrameworkElement ?? effectSubject;

        // Set up coordinates
        _endX = 0.0;
        _endY = 0.0;
        var startX = 0.0;
        var startY = 0.0;

        switch (Direction)
        {
            case SlideDirection.Left:
                startX = container.ActualWidth;

                break;

            case SlideDirection.Right:
                startX = -container.ActualWidth;

                break;

            case SlideDirection.Up:
                startY = container.ActualHeight;

                break;

            case SlideDirection.Down:
                startY = (float)-container.ActualHeight;

                break;
        }

        if (Reverse)
        {
            var tempEndX = _endX;
            var tempEndY = _endY;

            _endX = startX;
            _endY = startY;
            startX = tempEndX;
            startY = tempEndY;
        }

        var timeline = BuildTimeline(effectSubject, startX, startY);

        return timeline;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="effectSubject"/> is <see langword="null"/>.</exception>
    public override void Cancel<TSubject>(TSubject effectSubject)
    {
        Guard.IsNotNull(effectSubject);

        if (_translateTransform is null)
        {
            return;
        }

        _translateTransform.SetCurrentValue(TranslateTransform.XProperty, _endX);
        _translateTransform.SetCurrentValue(TranslateTransform.YProperty, _endY);
        _translateTransform = null;
    }

    private ParallelTimeline BuildTimeline<TSubject>(TSubject effectSubject, double startX, double startY)
        where TSubject : FrameworkElement, ITransitionSubject
    {
        var subjectDelay = GetTotalSubjectDelay(effectSubject);

        var zeroKeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero);
        var startKeyTime = KeyTime.FromTimeSpan(subjectDelay + Delay);
        var endKeyTime = KeyTime.FromTimeSpan(subjectDelay + Delay + Duration);

        var xAnimation = new DoubleAnimationUsingKeyFrames();
        xAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(startX, zeroKeyTime));
        xAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(startX, startKeyTime));
        xAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(_endX, endKeyTime, EasingFunction));

        var yAnimation = new DoubleAnimationUsingKeyFrames();
        yAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(startY, zeroKeyTime));
        yAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(startY, startKeyTime));
        yAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(_endY, endKeyTime, EasingFunction));

        var timeline = new ParallelTimeline();
        timeline.Children.Add(xAnimation);
        timeline.Children.Add(yAnimation);
        timeline.Completed += (_, _) => Cancel(effectSubject);

        _translateTransform!.SetCurrentValue(TranslateTransform.XProperty, startX);
        _translateTransform!.SetCurrentValue(TranslateTransform.YProperty, startY);

        Storyboard.SetTargetName(xAnimation, effectSubject.TranslateTransformName);
        Storyboard.SetTargetProperty(xAnimation, new PropertyPath(TranslateTransform.XProperty));

        Storyboard.SetTargetName(yAnimation, effectSubject.TranslateTransformName);
        Storyboard.SetTargetProperty(yAnimation, new PropertyPath(TranslateTransform.YProperty));

        return timeline;
    }
}