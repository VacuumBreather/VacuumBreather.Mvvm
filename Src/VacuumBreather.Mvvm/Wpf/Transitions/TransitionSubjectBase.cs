using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>
///     A base class for a <see cref="ContentControl"/> supporting <see cref="ITransition"/> effects. This is an
///     abstract class.
/// </summary>
/// <seealso cref="ContentControl"/>
/// <seealso cref="ITransitionSubject"/>
[PublicAPI]
[TemplatePart(Name = MatrixTransformPartName, Type = typeof(MatrixTransform))]
[TemplatePart(Name = RotateTransformPartName, Type = typeof(RotateTransform))]
[TemplatePart(Name = ScaleTransformPartName, Type = typeof(ScaleTransform))]
[TemplatePart(Name = SkewTransformPartName, Type = typeof(SkewTransform))]
[TemplatePart(Name = TranslateTransformPartName, Type = typeof(TranslateTransform))]
public abstract class TransitionSubjectBase : ContentControl, ITransitionSubject
{
    /// <summary>The name of the matrix transform template part.</summary>
    public const string MatrixTransformPartName = "PART_MatrixTransform";

    /// <summary>The name of the rotate transform template part.</summary>
    public const string RotateTransformPartName = "PART_RotateTransform";

    /// <summary>The name of the scale transform template part.</summary>
    public const string ScaleTransformPartName = "PART_ScaleTransform";

    /// <summary>The name of the skew transform template part.</summary>
    public const string SkewTransformPartName = "PART_SkewTransform";

    /// <summary>The name of the translate transform template part.</summary>
    public const string TranslateTransformPartName = "PART_TranslateTransform";

    /// <summary>The CascadingDelay property.</summary>
    public static readonly DependencyProperty CascadingDelayProperty = DependencyProperty.RegisterAttached(
        PropertyNameHelper.GetName(nameof(CascadingDelayProperty)),
        typeof(TimeSpan),
        typeof(TransitionSubjectBase),
        new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>Identifies the <see cref="TransitionDelay"/> dependency property.</summary>
    public static readonly DependencyProperty TransitionDelayProperty =
        DependencyProperty.Register(nameof(TransitionDelay),
                                    typeof(TimeSpan),
                                    typeof(TransitionSubjectBase),
                                    new PropertyMetadata(default(TimeSpan)));

    /// <summary>Identifies the <see cref="TransitionEffect"/> dependency property.</summary>
    public static readonly DependencyProperty TransitionEffectProperty =
        DependencyProperty.Register(nameof(TransitionEffect),
                                    typeof(ITransition),
                                    typeof(TransitionSubjectBase),
                                    new PropertyMetadata(default(ITransition)));

    /// <summary>The IsTransitionFinished event.</summary>
    public static readonly RoutedEvent TransitionFinishedEvent = EventManager.RegisterRoutedEvent(
        PropertyNameHelper.GetRoutedEventName(nameof(TransitionFinishedEvent)),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TransitionSubjectBase));

    private readonly RoutedEventArgs _transitionFinishedEventArgs;

    private MatrixTransform? _matrixTransform;
    private Storyboard? _storyboard;

    /// <summary>Initializes static members of the <see cref="TransitionSubjectBase"/> class.</summary>
    static TransitionSubjectBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionSubjectBase),
                                                 new FrameworkPropertyMetadata(typeof(TransitionSubjectBase)));
    }

    /// <summary>Initializes a new instance of the <see cref="TransitionSubjectBase"/> class.</summary>
    protected TransitionSubjectBase()
    {
        _transitionFinishedEventArgs = new RoutedEventArgs(TransitionFinishedEvent, this);
    }

    /// <summary>
    ///     Gets or sets the cascading delay between <see cref="ITransitionSubject"/> elements inside an
    ///     <see cref="ItemsControl"/>.
    /// </summary>
    /// <value>The cascading delay between <see cref="ITransitionSubject"/> elements inside an <see cref="ItemsControl"/>.</value>
    public TimeSpan CascadingDelay
    {
        get => (TimeSpan)GetValue(CascadingDelayProperty);
        set => SetValue(CascadingDelayProperty, value);
    }

    /// <inheritdoc/>
    public TransitionCollection AdditionalTransitionEffects { get; } = new();

    /// <inheritdoc/>
    public string MatrixTransformName => MatrixTransformPartName;

    /// <inheritdoc/>
    public string RotateTransformName => RotateTransformPartName;

    /// <inheritdoc/>
    public string ScaleTransformName => ScaleTransformPartName;

    /// <inheritdoc/>
    public string SkewTransformName => SkewTransformPartName;

    /// <inheritdoc/>
    public TimeSpan TransitionDelay
    {
        get => (TimeSpan)GetValue(TransitionDelayProperty);
        set => SetValue(TransitionDelayProperty, value);
    }

    /// <inheritdoc/>
    public ITransition? TransitionEffect
    {
        get => (ITransition?)GetValue(TransitionEffectProperty);
        set => SetValue(TransitionEffectProperty, value);
    }

    /// <inheritdoc/>
    public string TranslateTransformName => TranslateTransformPartName;

    /// <summary>
    ///     Gets the cascading delay between <see cref="ITransitionSubject"/> elements inside an
    ///     <see cref="ItemsControl"/>.
    /// </summary>
    /// <param name="element">The element from which to read the property value.</param>
    /// <returns>The value of the CascadingDelay attached property.</returns>
    public static TimeSpan GetCascadingDelay(DependencyObject element)
    {
        return (TimeSpan)element.GetValue(CascadingDelayProperty);
    }

    /// <summary>
    ///     Sets the cascading delay between <see cref="ITransitionSubject"/> elements inside an
    ///     <see cref="ItemsControl"/>.
    /// </summary>
    /// <param name="element">The element on which to set the attached property.</param>
    /// <param name="value">The property value to set.</param>
    public static void SetCascadingDelay(DependencyObject element, TimeSpan value)
    {
        element.SetValue(CascadingDelayProperty, value);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        var nameScopeRoot = this.GetNameScopeRoot();

        _matrixTransform = GetTemplateChild(MatrixTransformPartName) as MatrixTransform;
        var rotateTransform = GetTemplateChild(RotateTransformPartName) as RotateTransform;
        var scaleTransform = GetTemplateChild(ScaleTransformPartName) as ScaleTransform;
        var skewTransform = GetTemplateChild(SkewTransformPartName) as SkewTransform;
        var translateTransform = GetTemplateChild(TranslateTransformPartName) as TranslateTransform;

        UnregisterNames(MatrixTransformPartName,
                        RotateTransformPartName,
                        ScaleTransformPartName,
                        SkewTransformPartName,
                        TranslateTransformPartName);

        if (_matrixTransform != null)
        {
            nameScopeRoot.RegisterName(MatrixTransformPartName, _matrixTransform);
        }

        if (rotateTransform != null)
        {
            nameScopeRoot.RegisterName(RotateTransformPartName, rotateTransform);
        }

        if (scaleTransform != null)
        {
            nameScopeRoot.RegisterName(ScaleTransformPartName, scaleTransform);
        }

        if (skewTransform != null)
        {
            nameScopeRoot.RegisterName(SkewTransformPartName, skewTransform);
        }

        if (translateTransform != null)
        {
            nameScopeRoot.RegisterName(TranslateTransformPartName, translateTransform);
        }

        base.OnApplyTemplate();

        PerformTransition();

        void UnregisterNames(params string[] names)
        {
            foreach (var name in names.Where(name => FindName(name) != null))
            {
                UnregisterName(name);
            }
        }
    }

    /// <inheritdoc/>
    public void CancelTransition()
    {
        _storyboard?.Stop(this.GetNameScopeRoot());
        _storyboard = null;
        TransitionEffect?.Cancel(this);

        foreach (var effect in AdditionalTransitionEffects)
        {
            effect.Cancel(this);
        }
    }

    /// <inheritdoc/>
    public void PerformTransition(bool includeAdditionalEffects = true)
    {
        if (!IsLoaded || _matrixTransform is null)
        {
            return;
        }

        CancelTransition();

        _storyboard = new Storyboard();
        var transitionEffect = TransitionEffect?.Build(this);

        if (transitionEffect != null)
        {
            Timeline.SetDesiredFrameRate(transitionEffect, desiredFrameRate: 30);
            _storyboard.Children.Add(transitionEffect);
        }

        if (includeAdditionalEffects)
        {
            foreach (var effect in AdditionalTransitionEffects.Select(effect => effect.Build(this))
                                                              .Where(timeline => timeline is not null))
            {
                Timeline.SetDesiredFrameRate(effect!, desiredFrameRate: 30);
                _storyboard.Children.Add(effect!);
            }
        }

        _storyboard.Completed += (_, _) => OnTransitionFinished();
        _storyboard.Begin(this.GetNameScopeRoot(), isControllable: true);
    }

    /// <summary>Called when the transition has finished.</summary>
    protected virtual void OnTransitionFinished()
    {
        RaiseEvent(_transitionFinishedEventArgs);
    }
}