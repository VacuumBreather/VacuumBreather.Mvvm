using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>Provides an easy way to transition from one content to another using <see cref="ITransitionWipe"/> effects.</summary>
[PublicAPI]
public class Transitioner : Selector, IZIndexController
{
    /// <summary>Identifies the <see cref="AutoApplyTransitionOrigins"/> dependency property.</summary>
    public static readonly DependencyProperty AutoApplyTransitionOriginsProperty =
        DependencyProperty.Register(nameof(AutoApplyTransitionOrigins),
                                    typeof(bool),
                                    typeof(Transitioner),
                                    new PropertyMetadata(default(bool)));

    /// <summary>Identifies the <see cref="BackwardWipe"/> dependency property.</summary>
    public static readonly DependencyProperty BackwardWipeProperty =
        DependencyProperty.Register(nameof(BackwardWipe),
                                    typeof(ITransitionWipe),
                                    typeof(Transitioner),
                                    new PropertyMetadata(default(ITransitionWipe)));

    /// <summary>Identifies the <see cref="DefaultTransitionOrigin"/> dependency property.</summary>
    public static readonly DependencyProperty DefaultTransitionOriginProperty =
        DependencyProperty.Register(nameof(DefaultTransitionOrigin),
                                    typeof(Point),
                                    typeof(Transitioner),
                                    new PropertyMetadata(new Point(x: 0.5, y: 0.5)));

    /// <summary>Identifies the <see cref="ForwardWipe"/> dependency property.</summary>
    public static readonly DependencyProperty ForwardWipeProperty = DependencyProperty.Register(
        nameof(ForwardWipe),
        typeof(ITransitionWipe),
        typeof(Transitioner),
        new PropertyMetadata(default(ITransitionWipe)));

    /// <summary>Identifies the <see cref="IsLooping"/> dependency property.</summary>
    public static readonly DependencyProperty IsLoopingProperty = DependencyProperty.Register(
        nameof(IsLooping),
        typeof(bool),
        typeof(Transitioner),
        new PropertyMetadata(default(bool)));

    /// <summary>Moves to the first item.</summary>
    public static readonly RoutedUICommand MoveFirstCommand = new(
        text: "First",
        nameof(MoveFirstCommand),
        typeof(Transitioner));

    /// <summary>Moves to the last item.</summary>
    public static readonly RoutedUICommand MoveLastCommand = new(
        text: "Last",
        nameof(MoveLastCommand),
        typeof(Transitioner));

    /// <summary>Causes the the next item to be displayed (effectively increments <see cref="Selector.SelectedIndex"/>).</summary>
    public static readonly RoutedUICommand MoveNextCommand = new(
        text: "Next",
        nameof(MoveNextCommand),
        typeof(Transitioner));

    /// <summary>Causes the the previous item to be displayed (effectively decrements <see cref="Selector.SelectedIndex"/>).</summary>
    public static readonly RoutedUICommand MovePreviousCommand = new(
        text: "Previous",
        nameof(MovePreviousCommand),
        typeof(Transitioner));

    private Point? _nextTransitionOrigin;

    /// <summary>Initializes static members of the <see cref="Transitioner"/> class.</summary>
    static Transitioner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Transitioner),
                                                 new FrameworkPropertyMetadata(typeof(Transitioner)));
    }

    /// <summary>Initializes a new instance of the <see cref="Transitioner"/> class.</summary>
    public Transitioner()
    {
        CommandBindings.Add(new CommandBinding(MoveNextCommand, OnMoveNext, OnCanMoveNext));
        CommandBindings.Add(new CommandBinding(MovePreviousCommand, OnMovePrevious, OnCanMovePrevious));
        CommandBindings.Add(new CommandBinding(MoveFirstCommand, OnMoveFirst, OnCanMoveFirst));
        CommandBindings.Add(new CommandBinding(MoveLastCommand, OnMoveLast, OnCanMoveLast));

        // Set default wipes.
        BackwardWipe = new SlideWipe { Direction = SlideDirection.Right };
        ForwardWipe = new SlideWipe { Direction = SlideDirection.Left };

        AddHandler(TransitionSubjectBase.TransitionFinishedEvent, new RoutedEventHandler(OnIsTransitionFinished));

        Loaded += (_, _) =>
        {
            if (SelectedIndex != -1)
            {
                TransitionToItemIndex(SelectedIndex, previousIndex: -1);
            }
        };
    }

    /// <summary>
    ///     Gets or sets a value indicating whether transition origins will be applied to wipes, according to where a
    ///     transition was triggered from. For example the mouse position over a button which triggered the transition.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if transition origins will be applied to wipes, according to where a transition was
    ///     triggered from; otherwise, <see langword="false"/>.
    /// </value>
    public bool AutoApplyTransitionOrigins
    {
        get => (bool)GetValue(AutoApplyTransitionOriginsProperty);
        set => SetValue(AutoApplyTransitionOriginsProperty, value);
    }

    /// <summary>Gets or sets the wipe used when transitioning backwards.</summary>
    /// <value>The wipe used when transitioning backwards.</value>
    public ITransitionWipe? BackwardWipe
    {
        get => (ITransitionWipe?)GetValue(BackwardWipeProperty);
        set => SetValue(BackwardWipeProperty, value);
    }

    /// <summary>Gets or sets the default origin of the transition wipe.</summary>
    /// <value>The default origin of the transition wipe.</value>
    public Point DefaultTransitionOrigin
    {
        get => (Point)GetValue(DefaultTransitionOriginProperty);
        set => SetValue(DefaultTransitionOriginProperty, value);
    }

    /// <summary>Gets or sets the wipe used when transitioning forwards.</summary>
    /// <value>The wipe used when transitioning forwards.</value>
    public ITransitionWipe? ForwardWipe
    {
        get => (ITransitionWipe?)GetValue(ForwardWipeProperty);
        set => SetValue(ForwardWipeProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the last item should followed by the first in a loop and vice versa.</summary>
    /// <value>
    ///     <see langword="true"/> if the last item is followed by the first in a loop and vice versa; otherwise,
    ///     <see langword="false"/>.
    /// </value>
    public bool IsLooping
    {
        get => (bool)GetValue(IsLoopingProperty);
        set => SetValue(IsLoopingProperty, value);
    }

    /// <inheritdoc/>
    public void Stack(params TransitionerItem[] highestToLowest)
    {
        StackZIndices(highestToLowest);
    }

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TransitionerItem();
    }

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TransitionerItem;
    }

    /// <inheritdoc/>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (AutoApplyTransitionOrigins)
        {
            _nextTransitionOrigin = GetNavigationSourcePoint(e);
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    /// <inheritdoc/>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        var unselectedIndex = -1;

        if ((e.RemovedItems.Count == 1) && e.RemovedItems[index: 0] is { } removedItem)
        {
            unselectedIndex = Items.IndexOf(removedItem);
        }

        var selectedIndex = 1;

        if ((e.AddedItems.Count == 1) && e.AddedItems[index: 0] is { } addedItem)
        {
            selectedIndex = Items.IndexOf(addedItem);
        }

        TransitionToItemIndex(selectedIndex, unselectedIndex);

        base.OnSelectionChanged(e);
    }

    private static int GetStepSize(ExecutedRoutedEventArgs args)
    {
        var stepSize = args.Parameter switch
        {
            int i and > 0 => i,
            string s when int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var i) && (i > 0) => i,
            var _ => 1,
        };

        return stepSize;
    }

    private static bool IsSafePositive(double @double)
    {
        return !double.IsNaN(@double) && !double.IsInfinity(@double) && (@double > 0.0);
    }

    private static void StackZIndices(params TransitionerItem?[] highestToLowest)
    {
        var zIndex = highestToLowest.Length;

        foreach (var item in highestToLowest.Where(item => item is not null))
        {
            Panel.SetZIndex(item!, zIndex--);
        }
    }

    private void DecrementSelectedIndex(int stepSize)
    {
        var newIndex = IsLooping
                           ? (((SelectedIndex - stepSize) % Items.Count) + Items.Count) % Items.Count
                           : Math.Max(val1: 0, SelectedIndex - stepSize);

        SetCurrentValue(SelectedIndexProperty, newIndex);
    }

    private TransitionerItem GetItem(object item)
    {
        return IsItemItsOwnContainer(item)
                   ? (TransitionerItem)item
                   : (TransitionerItem)ItemContainerGenerator.ContainerFromItem(item);
    }

    private Point? GetNavigationSourcePoint(RoutedEventArgs executedRoutedEventArgs)
    {
        if (executedRoutedEventArgs.OriginalSource is not FrameworkElement sourceElement ||
            !IsAncestorOf(sourceElement) ||
            !IsSafePositive(ActualWidth) ||
            !IsSafePositive(ActualHeight) ||
            !IsSafePositive(sourceElement.ActualWidth) ||
            !IsSafePositive(sourceElement.ActualHeight))
        {
            return null;
        }

        var transitionOrigin = sourceElement.TranslatePoint(
            new Point(sourceElement.ActualWidth / 2, sourceElement.ActualHeight / 2),
            this);

        transitionOrigin = new Point(transitionOrigin.X / ActualWidth, transitionOrigin.Y / ActualHeight);

        return transitionOrigin;
    }

    private Point GetTransitionOrigin(TransitionerItem item)
    {
        return _nextTransitionOrigin ??
               (item.ReadLocalValue(TransitionerItem.TransitionOriginProperty) != DependencyProperty.UnsetValue
                    ? item.TransitionOrigin
                    : DefaultTransitionOrigin);
    }

    private void IncrementSelectedIndex(int stepSize)
    {
        var newIndex = IsLooping
                           ? (SelectedIndex + stepSize) % Items.Count
                           : Math.Min(Items.Count - 1, SelectedIndex + stepSize);

        SetCurrentValue(SelectedIndexProperty, newIndex);
    }

    private void OnCanMoveFirst(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (Items.Count > 0) && (SelectedIndex > 0);
    }

    private void OnCanMoveLast(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (Items.Count > 0) && (SelectedIndex < Items.Count - 1);
    }

    private void OnCanMoveNext(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (Items.Count > 0) && (IsLooping || (SelectedIndex < Items.Count - 1));
    }

    private void OnCanMovePrevious(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = (Items.Count > 0) && (IsLooping || (SelectedIndex > 0));
    }

    private void OnIsTransitionFinished(object sender, RoutedEventArgs e)
    {
        foreach (var item in Items.OfType<object>()
                                  .Select(GetItem)
                                  .Where(item => item.State == TransitionerItemState.Previous))
        {
            item.SetCurrentValue(TransitionerItem.StateProperty, TransitionerItemState.None);
        }
    }

    private void OnMoveFirst(object sender, ExecutedRoutedEventArgs e)
    {
        if (AutoApplyTransitionOrigins)
        {
            _nextTransitionOrigin = GetNavigationSourcePoint(e);
        }

        SetCurrentValue(SelectedIndexProperty, value: 0);
    }

    private void OnMoveLast(object sender, ExecutedRoutedEventArgs e)
    {
        if (AutoApplyTransitionOrigins)
        {
            _nextTransitionOrigin = GetNavigationSourcePoint(e);
        }

        SetCurrentValue(SelectedIndexProperty, Items.Count - 1);
    }

    private void OnMoveNext(object sender, ExecutedRoutedEventArgs e)
    {
        if (AutoApplyTransitionOrigins)
        {
            _nextTransitionOrigin = GetNavigationSourcePoint(e);
        }

        var stepSize = GetStepSize(e);

        IncrementSelectedIndex(stepSize);
    }

    private void OnMovePrevious(object sender, ExecutedRoutedEventArgs e)
    {
        if (AutoApplyTransitionOrigins)
        {
            _nextTransitionOrigin = GetNavigationSourcePoint(e);
        }

        var stepSize = GetStepSize(e);

        DecrementSelectedIndex(stepSize);
    }

    [SuppressMessage(category: "Minor Code Smell",
                     checkId: "S1905:Redundant casts should not be used",
                     Justification = "ItemsCollection is not guaranteed to follow nullable rules.")]
    private void SetZIndices(int currentIndex,
                             int previousIndex,
                             out TransitionerItem? newItem,
                             out TransitionerItem? oldItem)
    {
        oldItem = null;
        newItem = null;

        for (var index = 0; index < Items.Count; index++)
        {
            var item = (TransitionerItem?)GetItem(Items[index]);

            if (item is null)
            {
                continue;
            }

            if (index == currentIndex)
            {
                newItem = item;
                item.SetCurrentValue(TransitionerItem.StateProperty, TransitionerItemState.Current);
            }
            else if (index == previousIndex)
            {
                oldItem = item;
                item.SetCurrentValue(TransitionerItem.StateProperty, TransitionerItemState.Previous);
            }
            else
            {
                item.SetCurrentValue(TransitionerItem.StateProperty, TransitionerItemState.None);
            }

            item.CancelTransition();

            Panel.SetZIndex(item, value: 0);
        }
    }

    private void TransitionToItemIndex(int currentIndex, int previousIndex)
    {
        if (!IsLoaded)
        {
            return;
        }

        SetZIndices(currentIndex, previousIndex, out var newItem, out var oldItem);

        if (newItem is not null)
        {
            newItem.Opacity = 1;
        }

        if (oldItem is not null && newItem is not null)
        {
            var wipe = currentIndex > previousIndex
                           ? newItem.ForwardWipe ?? ForwardWipe
                           : newItem.BackwardWipe ?? BackwardWipe;

            if (wipe is not null)
            {
                wipe.Wipe(oldItem, newItem, GetTransitionOrigin(newItem), this);
            }
            else
            {
                StackZIndices(newItem, oldItem);
            }
        }
        else if (oldItem is not null || newItem is not null)
        {
            StackZIndices(oldItem ?? newItem);
        }

        _nextTransitionOrigin = null;
    }
}