using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;

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
    public Brush SeparatorBrush
    {
        get => (Brush)GetValue(SeparatorBrushProperty);
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
        var top = 0.0;

        if (DrawSeparatorAbove)
        {
            top += SeparatorThickness + SeparatorMargin.Top + SeparatorMargin.Bottom;
        }

        for (var i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];

            if (i != 0)
            {
                top += SeparatorThickness + SeparatorMargin.Top + SeparatorMargin.Bottom;
            }

            var finalRect = new Rect(new Point(0.0, top), new Size(finalSize.Width, child.DesiredSize.Height));

            child.Arrange(finalRect);
            top += child.DesiredSize.Height;
        }

        return finalSize;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var totalHeight = 0.0;
        var maxWidth = 0.0;

        if (DrawSeparatorAbove)
        {
            totalHeight += SeparatorThickness + SeparatorMargin.Top + SeparatorMargin.Bottom;
        }

        maxWidth = Math.Max(maxWidth, SeparatorMargin.Left + SeparatorMargin.Right);

        for (var i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];
            child.Measure(availableSize);
            totalHeight += child.DesiredSize.Height;

            if (i != 0)
            {
                totalHeight += SeparatorThickness + SeparatorMargin.Top + SeparatorMargin.Bottom;
            }

            maxWidth = Math.Max(maxWidth, child.DesiredSize.Width);
        }

        if (DrawSeparatorBelow)
        {
            totalHeight += SeparatorThickness + SeparatorMargin.Top + SeparatorMargin.Bottom;
        }

        return new Size(maxWidth, totalHeight);
    }

    /// <inheritdoc />
    protected override void OnChildDesiredSizeChanged(UIElement child)
    {
        InvalidateVisual();

        base.OnChildDesiredSizeChanged(child);
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext dc)
    {
        if (Background is not null)
        {
            dc.DrawRectangle(Background, null, new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
        }

        var top = 0.0;

        if (DrawSeparatorAbove)
        {
            top += SeparatorMargin.Top;

            var topLeft = new Point(SeparatorMargin.Left, top);
            var bottomRight = new Point(ActualWidth - SeparatorMargin.Right, top + SeparatorThickness);

            dc.DrawRectangle(SeparatorBrush, null, new Rect(topLeft, bottomRight));

            top += SeparatorThickness + SeparatorMargin.Bottom;
        }

        int i;

        for (i = 0; i < InternalChildren.Count - 1; i++)
        {
            UIElement child = InternalChildren[i];

            top += child.DesiredSize.Height;

            if (InternalChildren[i + 1].DesiredSize.Height > 0.0)
            {
                top += SeparatorMargin.Top;

                var topLeft = new Point(SeparatorMargin.Left, top);
                var bottomRight = new Point(ActualWidth - SeparatorMargin.Right, top + SeparatorThickness);

                dc.DrawRectangle(SeparatorBrush, null, new Rect(topLeft, bottomRight));

                top += SeparatorThickness + SeparatorMargin.Bottom;
            }
        }

        if (DrawSeparatorBelow && (InternalChildren[i].DesiredSize.Height > 0.0))
        {
            top += InternalChildren[i].DesiredSize.Height;
            top += SeparatorMargin.Top;

            var topLeft = new Point(SeparatorMargin.Left, top);
            var bottomRight = new Point(ActualWidth - SeparatorMargin.Right, top + SeparatorThickness);

            dc.DrawRectangle(SeparatorBrush, null, new Rect(topLeft, bottomRight));
        }
    }
}