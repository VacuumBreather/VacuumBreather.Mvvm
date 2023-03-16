using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Dialogs;

/// <summary>
///     An <see cref="AdornerDecorator"/> which allows to overlay elements with a <see cref="DialogHost"/>. The dialog
///     host will be rendered on top of the regular <see cref="AdornerLayer"/>.
/// </summary>
[PublicAPI]
public class DialogHostAdornerDecorator : AdornerDecorator
{
    /// <summary>Identifies the <see cref="DialogHost"/> dependency property.</summary>
    public static readonly DependencyProperty DialogHostProperty =
        DependencyProperty.Register(nameof(DialogHost),
                                    typeof(DialogHost),
                                    typeof(DialogHostAdornerDecorator),
                                    new PropertyMetadata(default(DialogHost), OnDialogHostChanged));

    /// <summary>Gets or sets the child of the AdornerDecorator.</summary>
    public override UIElement? Child
    {
        get => base.Child;
        set
        {
            Visual old = base.Child;

            if (old == value)
            {
                return;
            }

            if (value is null)
            {
                base.Child = null;
                RemoveDialogHost();
            }
            else
            {
                base.Child = value;

                if (old is null)
                {
                    AddDialogHost();
                }
            }
        }
    }

    /// <summary>
    ///     Gets or sets the <see cref="DialogHost"/> which should be layered on top of the child element and the
    ///     <see cref="AdornerLayer"/>.
    /// </summary>
    public DialogHost? DialogHost
    {
        get => (DialogHost?)GetValue(DialogHostProperty);
        set => SetValue(DialogHostProperty, value);
    }

    /// <summary>Gets the <see cref="Visual"/> children count.</summary>
    protected override int VisualChildrenCount
    {
        get
        {
            if (base.Child is null)
            {
                return 0;
            }

            return DialogHost is null ? 2 : 3;
        }
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var inkSize = base.ArrangeOverride(finalSize);

        var finalSizeRect = new Rect(finalSize);

        if (VisualTreeHelper.GetParent(AdornerLayer) != null)
        {
            AdornerLayer.Arrange(finalSizeRect);
        }

        if (DialogHost is not null && VisualTreeHelper.GetParent(DialogHost) is not null)
        {
            DialogHost.Arrange(finalSizeRect);
        }

        return inkSize;
    }

    /// <inheritdoc/>
    protected override Visual GetVisualChild(int index)
    {
        if (base.Child == null)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
        }

        switch (index)
        {
            case 0:
                return base.Child;

            case 1:
                return AdornerLayer;

            case 2:
            {
                if (DialogHost is null)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
                }

                return DialogHost;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        var desiredSize = base.MeasureOverride(constraint);

        if (VisualTreeHelper.GetParent(AdornerLayer) is not null)
        {
            AdornerLayer.Measure(constraint);
        }

        if (DialogHost is not null && VisualTreeHelper.GetParent(DialogHost) is not null)
        {
            DialogHost.Measure(constraint);
        }

        return desiredSize;
    }

    private static void OnDialogHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DialogHostAdornerDecorator decorator || decorator.Child is null)
        {
            return;
        }

        if (e.OldValue is DialogHost oldHost)
        {
            decorator.RemoveVisualChild(oldHost);
        }

        if (e.NewValue is DialogHost newHost)
        {
            decorator.AddVisualChild(newHost);
        }
    }

    private void AddDialogHost()
    {
        if (DialogHost is not null)
        {
            AddVisualChild(DialogHost);
        }
    }

    private void RemoveDialogHost()
    {
        if (DialogHost is not null)
        {
            RemoveVisualChild(DialogHost);
        }
    }
}