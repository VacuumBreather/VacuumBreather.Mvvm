using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Transitions;

/// <summary>A base class for a <see cref="ITransitionWipe"/>. This is an abstract class.</summary>
/// <seealso cref="MarkupExtension"/>
/// <seealso cref="ITransitionWipe"/>
[PublicAPI]
[MarkupExtensionReturnType(typeof(ITransitionWipe))]
public abstract class TransitionWipeBase : MarkupExtension, ITransitionWipe
{
    /// <inheritdoc/>
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

    /// <inheritdoc/>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(value: 500);

    /// <inheritdoc/>
    public IEasingFunction EasingFunction { get; set; } = new SineEase();

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <inheritdoc/>
    public void Wipe(TransitionerItem fromItem,
                     TransitionerItem toItem,
                     Point origin,
                     IZIndexController zIndexController)
    {
        Guard.IsNotNull(fromItem);
        Guard.IsNotNull(toItem);
        Guard.IsNotNull(zIndexController);

        ConfigureItems(fromItem, toItem, origin);

        fromItem.PerformTransition(includeAdditionalEffects: false);
        toItem.PerformTransition();

        zIndexController.Stack(toItem, fromItem);
    }

    /// <summary>
    ///     Called every time the wipe is performed. Override this to configure both items and their
    ///     <see cref="ITransition"/> effects.
    /// </summary>
    /// <param name="fromItem">The content to transition from.</param>
    /// <param name="toItem">To content to transition to.</param>
    /// <param name="origin">The origin point for the wipe transition.</param>
    protected virtual void ConfigureItems(TransitionerItem fromItem, TransitionerItem toItem, Point origin)
    {
    }
}