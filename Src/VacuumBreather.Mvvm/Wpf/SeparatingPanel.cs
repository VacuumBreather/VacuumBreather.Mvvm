using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>
///     A panel which stacks items vertically, drawing a separator in between them.
/// </summary>
/// <seealso cref="System.Windows.Controls.Panel" />
[PublicAPI]
public class SeparatingPanel : Panel
{
    /// <summary>
    ///     The brush used to draw the separator between panel children.
    /// </summary>
    public static readonly DependencyProperty SeparatorBrushProperty =
        DependencyProperty.Register(nameof(SeparatorBrush),
                                    typeof(Brush),
                                    typeof(SeparatingPanel),
                                    new FrameworkPropertyMetadata(Brushes.Black,
                                                                  FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    ///     The margin used by the separator between panel children.
    /// </summary>
    public static readonly DependencyProperty SeparatorMarginProperty = DependencyProperty.Register(
        nameof(SeparatorMargin),
        typeof(Thickness),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(new Thickness(0.0),
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    ///     The thickness used to draw the separator between panel children.
    /// </summary>
    public static readonly DependencyProperty SeparatorThicknessProperty = DependencyProperty.Register(
        nameof(SeparatorThickness),
        typeof(double),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(1.0,
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    ///     A value indicating whether the panel should draw a separator above its children in addition to in-between.
    /// </summary>
    public static readonly DependencyProperty DrawSeparatorAboveProperty = DependencyProperty.Register(
        nameof(DrawSeparatorAbove),
        typeof(bool),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(default(bool),
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    ///     A value indicating whether the panel should draw a separator below its children in addition to in-between.
    /// </summary>
    public static readonly DependencyProperty DrawSeparatorBelowProperty = DependencyProperty.Register(
        nameof(DrawSeparatorBelow),
        typeof(bool),
        typeof(SeparatingPanel),
        new FrameworkPropertyMetadata(default(bool),
                                      FrameworkPropertyMetadataOptions.AffectsArrange |
                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                      FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    ///     Gets or sets a value indicating whether the panel should draw a separator above its children.
    /// </summary>
    public bool DrawSeparatorAbove
    {
        get => (bool)GetValue(DrawSeparatorAboveProperty);
        set => SetValue(DrawSeparatorAboveProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the panel should draw a separator below its children.
    /// </summary>
    public bool DrawSeparatorBelow
    {
        get => (bool)GetValue(DrawSeparatorBelowProperty);
        set => SetValue(DrawSeparatorBelowProperty, value);
    }

    /// <summary>
    ///     Gets or sets the brush used to draw the separator between panel children.
    /// </summary>
    public Brush? SeparatorBrush
    {
        get => (Brush?)GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value);
    }

    /// <summary>
    ///     Gets or sets the margin used by the separator between panel children.
    /// </summary>
    public Thickness SeparatorMargin
    {
        get => (Thickness)GetValue(SeparatorMarginProperty);
        set => SetValue(SeparatorMarginProperty, value);
    }

    /// <summary>
    ///     Gets or sets the thickness used to draw the separator between panel children.
    /// </summary>
    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        WrapMeasuredChildren()
            .AggregateForEach(0.0,
                              (top, wrapper) =>
                              {
                                  var finalRect = new Rect(new Point(0.0, top + wrapper.SeparatorHeight),
                                                           new Size(finalSize.Width, wrapper.ChildHeight));

                                  wrapper.Child?.Arrange(finalRect);

                                  return top + wrapper.TotalHeight;
                              });

        return finalSize;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (InternalChildren.Count == 0)
        {
            return Size.Empty;
        }

        var children = InternalChildren.Cast<UIElement>().ToArray();

        children.ForEach(child => child.Measure(availableSize));

        var wrappers = WrapMeasuredChildren();

        var maxWidth = wrappers.Max(wrapper => wrapper.ChildWidth);
        var totalHeight = wrappers.Sum(wrapper => wrapper.TotalHeight);

        return new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc />
    protected override void OnChildDesiredSizeChanged(UIElement child)
    {
        InvalidateMeasure();
        InvalidateArrange();
        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext dc)
    {
        if (Background is not null)
        {
            dc.DrawRectangle(Background, null, new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
        }

        if (SeparatorBrush is null || !(SeparatorThickness > 0.0))
        {
            return;
        }

        WrapMeasuredChildren()
            .AggregateForEach(0.0,
                              (top, wrapper) =>
                              {
                                  if (wrapper.HasSeparator)
                                  {
                                      var separatorTop = top + SeparatorMargin.Top;
                                      var topLeft = new Point(SeparatorMargin.Left, separatorTop);

                                      var bottomRight = new Point(ActualWidth - SeparatorMargin.Right,
                                                                  separatorTop + SeparatorThickness);

                                      dc.DrawRectangle(SeparatorBrush, null, new Rect(topLeft, bottomRight));
                                  }

                                  return top + wrapper.TotalHeight;
                              });
    }

    private IList<ChildWrapper> WrapMeasuredChildren()
    {
        if (InternalChildren.Count == 0)
        {
            return Array.Empty<ChildWrapper>();
        }

        var children = InternalChildren.Cast<UIElement>().ToArray();

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
        public UIElement? Child { get; init; }

        public double ChildHeight { get; init; }

        public double ChildWidth { get; init; }

        public bool HasSeparator { get; set; }

        public double SeparatorHeight { get; set; }

        public double TotalHeight { get; set; }
    }
}