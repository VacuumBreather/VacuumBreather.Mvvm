using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Wpf.Dialogs;
using VacuumBreather.Mvvm.Wpf.Notifications;

namespace VacuumBreather.Mvvm.Wpf.Controls;

/// <summary>
///     An <see cref="AdornerDecorator"/> which allows to overlay elements with a <see cref="NotificationHost"/> and a
///     <see cref="DialogHost"/>. The additional elements will be rendered on top of the regular <see cref="AdornerLayer"/>
///     .
/// </summary>
[PublicAPI]
public class DialogAndNotificationAdornerDecorator : AdornerDecorator
{
    private readonly DialogHost _dialogHost = new();
    private readonly NotificationHost _notificationHost = new();

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
                RemoveOverlays();
            }
            else
            {
                base.Child = value;

                if (old is null)
                {
                    AddOverlays();
                }
            }
        }
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

            return 4;
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

        _dialogHost.Arrange(finalSizeRect);
        _notificationHost.Arrange(finalSizeRect);

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
                return _notificationHost;

            case 3:
                return _dialogHost;

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

        _dialogHost.Measure(constraint);
        _notificationHost.Measure(constraint);

        return desiredSize;
    }

    private void AddOverlays()
    {
        AddVisualChild(_notificationHost);
        AddVisualChild(_dialogHost);
    }

    private void RemoveOverlays()
    {
        RemoveVisualChild(_dialogHost);
        RemoveVisualChild(_notificationHost);
    }
}