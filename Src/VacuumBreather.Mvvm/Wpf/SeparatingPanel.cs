using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A panel which stacks items vertically, drawing a separator in between them.</summary>
/// <seealso cref="System.Windows.Controls.Panel"/>
[PublicAPI]
public class SeparatingPanel : Panel
{
    /// <summary>Identifies the <see cref="ReverseOrder"/> dependency property.</summary>
    public static readonly DependencyProperty ReverseOrderProperty = DependencyProperty.Register(
        nameof(ReverseOrder),
        typeof(bool),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(default(bool),
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SeparatorBrush"/> dependency property.</summary>
    public static readonly DependencyProperty SeparatorBrushProperty =
        DependencyProperty.Register(nameof(SeparatorBrush),
                                    typeof(Brush),
                                    typeof(SeparatingPanel),
                                    new FrameworkPropertyMetadata(Brushes.Black,
                                                                  FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SeparatorMargin"/> dependency property.</summary>
    public static readonly DependencyProperty SeparatorMarginProperty = DependencyProperty.Register(
        nameof(SeparatorMargin),
        typeof(Thickness),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(new Thickness(uniformLength: 0.0),
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SeparatorThickness"/> dependency property.</summary>
    public static readonly DependencyProperty SeparatorThicknessProperty = DependencyProperty.Register(
        nameof(SeparatorThickness),
        typeof(double),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(defaultValue: 1.0,
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="DrawSeparatorAbove"/> dependency property.</summary>
    public static readonly DependencyProperty DrawSeparatorAboveProperty = DependencyProperty.Register(
        nameof(DrawSeparatorAbove),
        typeof(bool),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(default(bool),
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="DrawSeparatorBelow"/> dependency property.</summary>
    public static readonly DependencyProperty DrawSeparatorBelowProperty = DependencyProperty.Register(
        nameof(DrawSeparatorBelow),
        typeof(bool),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(default(bool),
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Gets or sets a value indicating whether the panel should draw a separator above its children.</summary>
    public bool DrawSeparatorAbove
    {
        get => (bool)GetValue(DrawSeparatorAboveProperty);
        set => SetValue(DrawSeparatorAboveProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the panel should draw a separator below its children.</summary>
    public bool DrawSeparatorBelow
    {
        get => (bool)GetValue(DrawSeparatorBelowProperty);
        set => SetValue(DrawSeparatorBelowProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the order of elements should be reversed by the panel.</summary>
    public bool ReverseOrder
    {
        get => (bool)GetValue(ReverseOrderProperty);
        set => SetValue(ReverseOrderProperty, value);
    }

    /// <summary>Gets or sets the brush used to draw the separator between panel children.</summary>
    public Brush? SeparatorBrush
    {
        get => (Brush?)GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value);
    }

    /// <summary>Gets or sets the margin used by the separator between panel children.</summary>
    public Thickness SeparatorMargin
    {
        get => (Thickness)GetValue(SeparatorMarginProperty);
        set => SetValue(SeparatorMarginProperty, value);
    }

    /// <summary>Gets or sets the thickness used to draw the separator between panel children.</summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }

    /// <summary>Gets the list of internal children cast to <see cref="UIElement"/>.</summary>
    /// <returns>The list of internal children.</returns>
    protected virtual IList<UIElement> GetInternalChildElements()
    {
        return InternalChildren.Cast<UIElement>().ToArray();
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        WrapMeasuredChildren()
            .AggregateForEach(seed: 0.0,
                              (top, wrapper) =>
                              {
                                  var finalRect = new Rect(new Point(x: 0.0, top + wrapper.SeparatorHeight),
                                                           new Size(finalSize.Width, wrapper.ChildHeight));

                                  wrapper.Child?.Arrange(finalRect);

                                  return top + wrapper.TotalHeight;
                              });

        return finalSize;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (InternalChildren.Count == 0)
        {
            return Size.Empty;
        }

        var children = GetInternalChildElements();

        children.ForEach(child => child.Measure(availableSize));

        var wrappers = WrapMeasuredChildren();

        var maxWidth = wrappers.Max(wrapper => wrapper.ChildWidth);
        var totalHeight = wrappers.Sum(wrapper => wrapper.TotalHeight);

        return new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc/>
    protected override void OnChildDesiredSizeChanged(UIElement child)
    {
        InvalidateMeasure();
        InvalidateArrange();
        InvalidateVisual();
    }

    /// <inheritdoc/>
    protected override void OnRender(DrawingContext dc)
    {
        if (Background is not null)
        {
            dc.DrawRectangle(Background,
                             pen: null,
                             new Rect(new Point(x: 0, y: 0), new Size(ActualWidth, ActualHeight)));
        }

        if (SeparatorBrush is null || (SeparatorThickness <= 0.0))
        {
            return;
        }

        WrapMeasuredChildren()
            .AggregateForEach(seed: 0.0,
                              (top, wrapper) =>
                              {
                                  if (wrapper.HasSeparator)
                                  {
                                      var separatorTop = top + SeparatorMargin.Top;
                                      var topLeft = new Point(SeparatorMargin.Left, separatorTop);

                                      var bottomRight = new Point(ActualWidth - SeparatorMargin.Right,
                                                                  separatorTop + SeparatorThickness);

                                      dc.DrawRectangle(SeparatorBrush, pen: null, new Rect(topLeft, bottomRight));
                                  }

                                  return top + wrapper.TotalHeight;
                              });
    }

    /// <inheritdoc/>
    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        InvalidateMeasure();
        InvalidateArrange();
        InvalidateVisual();
    }

    private IList<ChildWrapper> WrapMeasuredChildren()
    {
        if (InternalChildren.Count == 0)
        {
            return Array.Empty<ChildWrapper>();
        }

        var children = GetInternalChildElements();

        if (ReverseOrder)
        {
            children = children.Reverse().ToArray();
        }

        var separatorHeight = SeparatorThickness + SeparatorMargin.Top + SeparatorMargin.Bottom;

        var wrappers = children
                       .Select(child => new ChildWrapper
                                        {
                                            Child = child,
                                            ChildWidth = child.DesiredSize.Width,
                                            ChildHeight = child.DesiredSize.Height,
                                            SeparatorHeight = child.DesiredSize.Height > 0.0 ? separatorHeight : 0.0,
                                            HasSeparator = child.DesiredSize.Height > 0.0,
                                        })
                       .Prepend(new ChildWrapper
                                {
                                    Child = null,
                                    ChildWidth = SeparatorMargin.Left + SeparatorMargin.Right,
                                    ChildHeight = 0.0,
                                    SeparatorHeight = DrawSeparatorAbove ? separatorHeight : 0.0,
                                    HasSeparator = DrawSeparatorAbove,
                                })
                       .Append(new ChildWrapper
                               {
                                   Child = null,
                                   ChildWidth = SeparatorMargin.Left + SeparatorMargin.Right,
                                   ChildHeight = 0.0,
                                   SeparatorHeight = DrawSeparatorBelow ? separatorHeight : 0.0,
                                   HasSeparator = DrawSeparatorBelow,
                               })
                       .ToArray();

        // The first child has no separator above it, aside from the optional draw-above separator.
        wrappers[1].HasSeparator = false;
        wrappers[1].SeparatorHeight = 0.0;

        wrappers.ForEach(wrapper => { wrapper.TotalHeight = wrapper.ChildHeight + wrapper.SeparatorHeight; });

        return wrappers;
    }

    private sealed class ChildWrapper
    {
        internal UIElement? Child { get; init; }

        internal double ChildHeight { get; init; }

        internal double ChildWidth { get; init; }

        internal bool HasSeparator { get; set; }

        internal double SeparatorHeight { get; set; }

        internal double TotalHeight { get; set; }
    }
}