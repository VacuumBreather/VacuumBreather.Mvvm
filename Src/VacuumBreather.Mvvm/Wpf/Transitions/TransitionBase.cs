﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A base class for transition effects. This is an abstract class.</summary>
/// <seealso cref="MarkupExtension"/>
/// <seealso cref="ITransition"/>
[PublicAPI]
[MarkupExtensionReturnType(typeof(ITransition))]
public abstract class TransitionBase : MarkupExtension, ITransition
{
    /// <inheritdoc/>
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

    /// <inheritdoc/>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(value: 250);

    /// <inheritdoc/>
    public IEasingFunction EasingFunction { get; set; } = new CubicEase();

    /// <inheritdoc/>
    public Point Origin { get; set; } = new(x: 0.5, y: 0.5);

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <inheritdoc/>
    public abstract Timeline? Build<TSubject>(TSubject effectSubject)
        where TSubject : FrameworkElement, ITransitionSubject;

    /// <inheritdoc/>
    public abstract void Cancel<TSubject>(TSubject effectSubject)
        where TSubject : FrameworkElement, ITransitionSubject;

    /// <summary>Gets the total delay defined by the <see cref="ITransitionSubject"/>.</summary>
    /// <remarks>
    ///     This can include a potential cascading delay if the subject is hosted inside an <see cref="ItemsControl"/>.
    ///     Use this to define the key frame at which your transition animation starts.
    /// </remarks>
    /// <example>
    ///     <code>
    ///         var zeroFrame = new DiscreteDoubleKeyFrame(0, TimeSpan.Zero);
    ///         var startFrame = new DiscreteDoubleKeyFrame(0, GetTotalSubjectDelay(effectSubject) + Delay);
    ///         var endFrame =
    ///         new EasingDoubleKeyFrame(1, GetTotalSubjectDelay(effectSubject) + Delay + Duration)
    ///         {
    ///             EasingFunction = EasingFunction
    ///         };
    ///     </code>
    /// </example>
    /// <typeparam name="TSubject">The type of the <see cref="ITransitionSubject"/>.</typeparam>
    /// <param name="effectSubject">The <see cref="ITransitionSubject"/> of the effect.</param>
    /// <returns>The total delay defined by the <see cref="ITransitionSubject"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="effectSubject"/> is <see langword="null"/>.</exception>
    protected static TimeSpan GetTotalSubjectDelay<TSubject>(TSubject effectSubject)
        where TSubject : FrameworkElement, ITransitionSubject
    {
        if (effectSubject is null)
        {
            throw new ArgumentNullException(nameof(effectSubject));
        }

        var totalDelay = effectSubject.TransitionDelay;

        var interval = (TimeSpan)effectSubject.GetValue(TransitionSubjectBase.CascadingDelayProperty);

        if (TimeSpan.Zero.Equals(interval))
        {
            return totalDelay;
        }

        var itemsControl = ItemsControl.ItemsControlFromItemContainer(effectSubject);

        if (itemsControl is null)
        {
            DependencyObject? ancestor = effectSubject;

            while ((ancestor != null) && itemsControl is null)
            {
                ancestor = VisualTreeHelper.GetParent(ancestor);
                itemsControl = ItemsControl.ItemsControlFromItemContainer(ancestor);
            }
        }

        if (itemsControl is null)
        {
            return totalDelay;
        }

        var container = itemsControl.IsItemItsOwnContainer(effectSubject)
                            ? effectSubject
                            : itemsControl.ItemContainerGenerator.ContainerFromItem(effectSubject);

        if (container is null && effectSubject.DataContext is { } dataContext)
        {
            container = itemsControl.ItemContainerGenerator.ContainerFromItem(dataContext);
        }

        if (container is null)
        {
            return totalDelay;
        }

        return GetTotalDelayFromContainer(container,
                                          itemsControl,
                                          effectSubject,
                                          interval,
                                          totalDelay);
    }

    private static TimeSpan GetTotalDelayFromContainer<TSubject>(DependencyObject container,
                                                                 ItemsControl itemsControl,
                                                                 TSubject effectSubject,
                                                                 TimeSpan interval,
                                                                 TimeSpan totalDelay)
        where TSubject : FrameworkElement, ITransitionSubject
    {
        var index = itemsControl.ItemContainerGenerator.IndexFromContainer(container);

        if (index == -1)
        {
            // Container generation may not have completed.
            index = itemsControl.Items.IndexOf(effectSubject);
        }

        if ((index == -1) && effectSubject is FrameworkElement frameworkElement)
        {
            // Still not found, repeat now using DataContext.
            index = itemsControl.Items.IndexOf(frameworkElement.DataContext);
        }

        var cascadingDelay = index >= 0 ? TimeSpan.FromTicks(interval.Ticks * index) : TimeSpan.Zero;

        totalDelay += cascadingDelay;

        return totalDelay;
    }
}