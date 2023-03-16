using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>An adorner which allows placing any control on the adorner layer.</summary>
[PublicAPI]
public class ControlAdorner : Adorner
{
    private Control? _child;

    /// <summary>Initializes a new instance of the <see cref="ControlAdorner"/> class.</summary>
    /// <param name="adornedElement">The element that should be adorned.</param>
    public ControlAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
    }

    /// <summary>Gets or sets the child control that should be rendered by the adorner.</summary>
    public Control? Child
    {
        get => _child;
        set
        {
            if (_child != null)
            {
                RemoveVisualChild(_child);
            }

            _child = value;

            if (_child != null)
            {
                AddVisualChild(_child);
            }
        }
    }

    /// <inheritdoc/>
    protected override int VisualChildrenCount => 1;

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        _child?.Arrange(new Rect(new Point(x: 0, y: 0), finalSize));

        return finalSize;
    }

    /// <inheritdoc/>
    protected override Visual? GetVisualChild(int index)
    {
        Guard.IsInRange(index, minimum: 0, maximum: 1);

        return _child;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        _child?.Measure(constraint);

        return _child?.DesiredSize ?? Size.Empty;
    }
}