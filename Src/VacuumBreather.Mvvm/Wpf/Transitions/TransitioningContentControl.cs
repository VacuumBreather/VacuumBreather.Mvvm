using System.Windows;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>Enables transition effects for content.</summary>
[PublicAPI]
public class TransitioningContentControl : TransitionSubjectBase
{
    /// <summary>Identifies the <see cref="TransitionTriggers"/> dependency property.</summary>
    public static readonly DependencyProperty TransitionTriggersProperty =
        DependencyProperty.Register(nameof(TransitionTriggers),
                                    typeof(TransitionTriggers),
                                    typeof(TransitioningContentControl),
                                    new PropertyMetadata(TransitionTriggers.Default));

    /// <summary>Initializes static members of the <see cref="TransitioningContentControl"/> class.</summary>
    static TransitioningContentControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitioningContentControl),
                                                 new FrameworkPropertyMetadata(typeof(TransitioningContentControl)));
    }

    /// <summary>Initializes a new instance of the <see cref="TransitioningContentControl"/> class.</summary>
    public TransitioningContentControl()
    {
        Loaded += (_, _) => Run(TransitionTriggers.Loaded);
        IsVisibleChanged += OnIsVisibleChanged;
        IsEnabledChanged += OnIsEnabledChanged;
    }

    /// <summary>Gets or sets the triggers that start the transition.</summary>
    /// <value>The triggers that start the transition.</value>
    public TransitionTriggers TransitionTriggers
    {
        get => (TransitionTriggers)GetValue(TransitionTriggersProperty);
        set => SetValue(TransitionTriggersProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        Run(TransitionTriggers.ContentChanged);
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Run(TransitionTriggers.IsEnabled);
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        Run(TransitionTriggers.IsVisible);
    }

    private void Run(TransitionTriggers requiredHint)
    {
        if (((TransitionTriggers & requiredHint) != 0) && IsEnabled && (Visibility == Visibility.Visible))
        {
            PerformTransition();
        }
    }
}