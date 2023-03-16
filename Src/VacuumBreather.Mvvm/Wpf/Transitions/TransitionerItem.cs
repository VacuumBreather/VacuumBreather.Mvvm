using System.Windows;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>Host the content of an individual item within a <see cref="Transitioner"/>.</summary>
[PublicAPI]
public class TransitionerItem : TransitionSubjectBase
{
    /// <summary>Identifies the <see cref="BackwardWipe"/> dependency property.</summary>
    public static readonly DependencyProperty BackwardWipeProperty =
        DependencyProperty.Register(nameof(BackwardWipe),
                                    typeof(ITransitionWipe),
                                    typeof(TransitionerItem),
                                    new PropertyMetadata(propertyChangedCallback: default));

    /// <summary>Identifies the <see cref="ForwardWipe"/> dependency property.</summary>
    public static readonly DependencyProperty ForwardWipeProperty = DependencyProperty.Register(
        nameof(ForwardWipe),
        typeof(ITransitionWipe),
        typeof(TransitionerItem),
        new PropertyMetadata(propertyChangedCallback: default));

    /// <summary>Identifies the <see cref="State"/> dependency property.</summary>
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State),
        typeof(TransitionerItemState),
        typeof(TransitionerItem),
        new PropertyMetadata(default(TransitionerItemState)));

    /// <summary>Identifies the <see cref="TransitionOrigin"/> dependency property.</summary>
    public static readonly DependencyProperty TransitionOriginProperty =
        DependencyProperty.Register(nameof(TransitionOrigin),
                                    typeof(Point),
                                    typeof(TransitionerItem),
                                    new PropertyMetadata(new Point(x: 0.5, y: 0.5)));

    /// <summary>Initializes static members of the <see cref="TransitionerItem"/> class.</summary>
    static TransitionerItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitionerItem),
                                                 new FrameworkPropertyMetadata(typeof(TransitionerItem)));
    }

    /// <summary>Gets or sets the wipe used when transitioning backwards.</summary>
    /// <value>The wipe used when transitioning backwards.</value>
    public ITransitionWipe? BackwardWipe
    {
        get => (ITransitionWipe?)GetValue(BackwardWipeProperty);
        set => SetValue(BackwardWipeProperty, value);
    }

    /// <summary>Gets or sets the wipe used when transitioning forwards.</summary>
    /// <value>The wipe used when transitioning forwards.</value>
    public ITransitionWipe? ForwardWipe
    {
        get => (ITransitionWipe?)GetValue(ForwardWipeProperty);
        set => SetValue(ForwardWipeProperty, value);
    }

    /// <summary>Gets or sets the state of the <see cref="TransitionerItem"/>.</summary>
    /// <value>The state of the <see cref="TransitionerItem"/>.</value>
    public TransitionerItemState State
    {
        get => (TransitionerItemState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <summary>Gets or sets the origin point for the wipe transition applied to this <see cref="TransitionerItem"/>.</summary>
    /// <value>The  origin point for the wipe transition applied to this <see cref="TransitionerItem"/>.</value>
    public Point TransitionOrigin
    {
        get => (Point)GetValue(TransitionOriginProperty);
        set => SetValue(TransitionOriginProperty, value);
    }
}