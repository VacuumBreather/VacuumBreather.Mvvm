using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;
using VacuumBreather.Mvvm.Wpf.Transitions;

namespace VacuumBreather.Mvvm.Wpf.Notifications;

/// <summary>Represents a selectable dialog item inside a <see cref="NotificationHost"/>.</summary>
/// <seealso cref="TransitioningContentControl"/>
[PublicAPI]
[TemplatePart(Name = LayoutScaleTransformPartName, Type = typeof(ScaleTransform))]
public class NotificationItem : TransitioningContentControl
{
    /// <summary>The name of the layout scale transform template part.</summary>
    public const string LayoutScaleTransformPartName = "PART_LayoutScaleTransform";

    /// <summary>Gets the command to close a <see cref="NotificationItem"/>.</summary>
    public static readonly RoutedCommand CloseCommand = new(nameof(CloseCommand), typeof(NotificationItem));

    /// <summary>Identifies the <see cref="CloseTransitionEffect"/> dependency property.</summary>
    public static readonly DependencyProperty CloseTransitionEffectProperty =
        DependencyProperty.Register(nameof(CloseTransitionEffect),
                                    typeof(ITransition),
                                    typeof(NotificationItem),
                                    new PropertyMetadata(default(ITransition)));

    private readonly NotificationHost _host;

    private IAsyncOperation? _operation;
    private ScaleTransform? _layoutScaleTransform;

    /// <summary>Initializes static members of the <see cref="NotificationItem"/> class.</summary>
    static NotificationItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationItem),
                                                 new FrameworkPropertyMetadata(typeof(NotificationItem)));
    }

    /// <summary>Initializes a new instance of the <see cref="NotificationItem"/> class.</summary>
    /// <param name="host">The <see cref="NotificationHost"/> hosting this item.</param>
    public NotificationItem(NotificationHost host)
    {
        _host = host;
        CommandBindings.Add(new CommandBinding(CloseCommand, OnCloseExecuted));
    }

    /// <summary>Gets or sets the effect to run when closing this item.</summary>
    public ITransition? CloseTransitionEffect
    {
        get => (ITransition?)GetValue(CloseTransitionEffectProperty);
        set => SetValue(CloseTransitionEffectProperty, value);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        var nameScopeRoot = this.GetNameScopeRoot();

        _layoutScaleTransform = GetTemplateChild(LayoutScaleTransformPartName) as ScaleTransform;

        if (FindName(LayoutScaleTransformPartName) != null)
        {
            UnregisterName(LayoutScaleTransformPartName);
        }

        if (_layoutScaleTransform is not null)
        {
            nameScopeRoot.RegisterName(LayoutScaleTransformPartName, _layoutScaleTransform);
        }

        base.OnApplyTemplate();
    }

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (oldContent is IDeactivate oldDeactivate)
        {
            oldDeactivate.Deactivating -= OnContentDeactivatingAsync;
        }

        if (newContent is IDeactivate deactivate)
        {
            deactivate.Deactivating += OnContentDeactivatingAsync;
        }
        else
        {
            ThreadHelper.RunOnUIThreadAndForget(async () =>
            {
                using var operation = AsyncHelper.CreateAsyncOperation().Assign(out _operation);

                _host.Unloaded += OnHostUnloaded;

                try
                {
                    await Task.Delay(millisecondsDelay: 5000, operation.Token);
                }
                finally
                {
                    await RunClosingTransitionAsync(operation.Token);

                    if (_host.ItemsSource is IList itemsList)
                    {
                        itemsList.Remove(DataContext);
                    }

                    _host.Unloaded -= OnHostUnloaded;
                }
            });
        }
    }

    private Timeline? BuildScaleTimeline(TimeSpan duration)
    {
        if (_layoutScaleTransform is null)
        {
            return null;
        }

        const double StartScale = 1.0;
        const double EndScale = 0.0;

        var startFrame = new DiscreteDoubleKeyFrame(StartScale, TimeSpan.Zero);
        var endFrame = new EasingDoubleKeyFrame(EndScale, duration) { EasingFunction = new CubicEase() };

        var timeline = new DoubleAnimationUsingKeyFrames();
        timeline.KeyFrames.Add(startFrame);
        timeline.KeyFrames.Add(endFrame);
        timeline.Duration = duration;
        timeline.Completed += (_, _) => CancelScaling();

        _layoutScaleTransform.SetCurrentValue(ScaleTransform.ScaleYProperty, StartScale);

        Storyboard.SetTargetName(timeline, LayoutScaleTransformPartName);
        Storyboard.SetTargetProperty(timeline, new PropertyPath(ScaleTransform.ScaleYProperty));

        return timeline;
    }

    private void CancelScaling()
    {
        if (_layoutScaleTransform is null)
        {
            return;
        }

        _layoutScaleTransform.SetCurrentValue(ScaleTransform.ScaleYProperty, value: 0.0);
        _layoutScaleTransform = null;
    }

    private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (!ReferenceEquals(sender, this))
        {
            return;
        }

        if (Content is IClose closable)
        {
            ThreadHelper.RunOnUIThreadAndForget(() => closable.TryCloseAsync());
        }
        else
        {
            _operation?.Cancel();
        }
    }

    private async ValueTask OnContentDeactivatingAsync(object sender, DeactivatingEventArgs e, CancellationToken token)
    {
        if (!e.WillClose)
        {
            return;
        }

        await RunClosingTransitionAsync(token);
    }

    private void OnHostUnloaded(object sender, RoutedEventArgs e)
    {
        _operation?.Cancel();
        _host.Unloaded -= OnHostUnloaded;
    }

    private async ValueTask RunClosingTransitionAsync(CancellationToken cancellationToken)
    {
        if (!CanPerformTransition ||
            (CloseTransitionEffect is null && _layoutScaleTransform is null) ||
            cancellationToken.IsCancellationRequested)
        {
            return;
        }

        CancelTransition();

        var storyboard = new Storyboard();
        var transitionEffect = CloseTransitionEffect?.Build(this);

        var layoutScaleEffect =
            BuildScaleTimeline(CloseTransitionEffect?.Duration ?? TimeSpan.FromMilliseconds(value: 500));

        if (transitionEffect != null)
        {
            storyboard.Children.Add(transitionEffect);
        }

        if (layoutScaleEffect != null)
        {
            storyboard.Children.Add(layoutScaleEffect);
        }

        var tcs = new TaskCompletionSource();

        cancellationToken.Register(() =>
        {
            storyboard.Stop();
            tcs.TrySetCanceled();
        });

        storyboard.Completed += (_, _) => tcs.TrySetResult();

        storyboard.Begin(this.GetNameScopeRoot(), isControllable: true);

        await tcs.Task;
    }
}