using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EnumsNET;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Notifications;

/// <summary>Represents a control that contains opened <see cref="NotificationItem"/> items.</summary>
/// <seealso cref="ContentControl"/>
[PublicAPI]
[TemplatePart(Name = RootPanelPartName, Type = typeof(DockPanel))]
[TemplatePart(Name = ItemsPresenterPartName, Type = typeof(NotificationPanel))]
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(NotificationItem))]
public class NotificationHost : ItemsControl
{
    /// <summary>The name of the root panel template part.</summary>
    public const string RootPanelPartName = "PART_RootPanel";

    /// <summary>The name of the <see cref="ItemsPresenter"/> template part.</summary>
    public const string ItemsPresenterPartName = "PART_ItemsPresenter";

    /// <summary>Identifies the <see cref="MaxDisplayedElements"/> dependency property.</summary>
    public static readonly DependencyProperty MaxDisplayedElementsProperty = DependencyProperty.Register(
        nameof(MaxDisplayedElements),
        typeof(ushort),
        typeof(NotificationHost),
        new PropertyMetadata((ushort)5));

    /// <summary>Identifies the <see cref="NotificationAlignment"/> dependency property.</summary>
    public static readonly DependencyProperty NotificationAlignmentProperty =
        DependencyProperty.Register(nameof(NotificationAlignment),
                                    typeof(NotificationAlignment),
                                    typeof(NotificationHost),
                                    new PropertyMetadata(NotificationAlignment.TopCenter,
                                                         OnNotificationAlignmentChanged));

    private DockPanel? _rootPanel;
    private NotificationPanel? _itemsPresenter;

    /// <summary>Initializes static members of the <see cref="NotificationHost"/> class.</summary>
    static NotificationHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationHost),
                                                 new FrameworkPropertyMetadata(typeof(NotificationHost)));
    }

    /// <summary>Initializes a new instance of the <see cref="NotificationHost"/> class.</summary>
    public NotificationHost()
    {
        Loaded += OnLoaded;

        if (TryFindResource(nameof(IServiceProvider)) is IServiceProvider serviceProvider &&
            serviceProvider.GetService(typeof(INotificationService)) is INotificationService service)
        {
            DataContext = service;
            var itemsSourceBinding = new Binding(nameof(INotificationService.Items)) { Mode = BindingMode.OneWay };
            SetBinding(ItemsSourceProperty, itemsSourceBinding);
        }
    }

    /// <summary>Gets or sets the maximum number of elements the panel should display. The default is 5.</summary>
    public ushort MaxDisplayedElements
    {
        get => (ushort)GetValue(MaxDisplayedElementsProperty);
        set => SetValue(MaxDisplayedElementsProperty, value);
    }

    /// <summary>
    ///     Gets or sets the positioning of the notifications inside the <see cref="NotificationHost"/>. The default is
    ///     Notifications.NotificationAlignment="Alignment.TopCenter"/>.
    /// </summary>
    public NotificationAlignment NotificationAlignment
    {
        get => (NotificationAlignment)GetValue(NotificationAlignmentProperty);
        set => SetValue(NotificationAlignmentProperty, value);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        _rootPanel = GetTemplateChild(RootPanelPartName) as DockPanel;
        _itemsPresenter = GetTemplateChild(ItemsPresenterPartName) as NotificationPanel;

        base.OnApplyTemplate();
    }

    /// <inheritdoc/>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new NotificationItem(this);
    }

    /// <inheritdoc/>
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is NotificationItem;
    }

    private static void AlignBottomCenter(SeparatingPanel itemsPresenter)
    {
        itemsPresenter.SetCurrentValue(SeparatingPanel.ReverseOrderProperty, value: true);
        itemsPresenter.SetCurrentValue(DockPanel.DockProperty, Dock.Bottom);
        itemsPresenter.SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        itemsPresenter.SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Bottom);
    }

    private static void AlignBottomLeft(SeparatingPanel itemsPresenter)
    {
        itemsPresenter.SetCurrentValue(SeparatingPanel.ReverseOrderProperty, value: true);
        itemsPresenter.SetCurrentValue(DockPanel.DockProperty, Dock.Bottom);
        itemsPresenter.SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
        itemsPresenter.SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Bottom);
    }

    private static void AlignBottomRight(SeparatingPanel itemsPresenter)
    {
        itemsPresenter.SetCurrentValue(SeparatingPanel.ReverseOrderProperty, value: true);
        itemsPresenter.SetCurrentValue(DockPanel.DockProperty, Dock.Bottom);
        itemsPresenter.SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
        itemsPresenter.SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Bottom);
    }

    private static void AlignTopCenter(SeparatingPanel itemsPresenter)
    {
        itemsPresenter.SetCurrentValue(SeparatingPanel.ReverseOrderProperty, value: false);
        itemsPresenter.SetCurrentValue(DockPanel.DockProperty, Dock.Top);
        itemsPresenter.SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
        itemsPresenter.SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Top);
    }

    private static void AlignTopLeft(SeparatingPanel itemsPresenter)
    {
        itemsPresenter.SetCurrentValue(SeparatingPanel.ReverseOrderProperty, value: false);
        itemsPresenter.SetCurrentValue(DockPanel.DockProperty, Dock.Top);
        itemsPresenter.SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Left);
        itemsPresenter.SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Top);
    }

    private static void AlignTopRight(SeparatingPanel itemsPresenter)
    {
        itemsPresenter.SetCurrentValue(SeparatingPanel.ReverseOrderProperty, value: false);
        itemsPresenter.SetCurrentValue(DockPanel.DockProperty, Dock.Top);
        itemsPresenter.SetCurrentValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
        itemsPresenter.SetCurrentValue(VerticalAlignmentProperty, VerticalAlignment.Top);
    }

    private static void OnNotificationAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NotificationHost host)
        {
            return;
        }

        host.UpdatePositioning();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdatePositioning();
    }

    private void UpdatePositioning()
    {
        if (_rootPanel is null || _itemsPresenter is not { } itemsPresenter || !NotificationAlignment.IsValid())
        {
            return;
        }

        switch (NotificationAlignment)
        {
            case NotificationAlignment.TopLeft:
                AlignTopLeft(itemsPresenter);

                break;

            case NotificationAlignment.TopCenter:
                AlignTopCenter(itemsPresenter);

                break;

            case NotificationAlignment.TopRight:
                AlignTopRight(itemsPresenter);

                break;

            case NotificationAlignment.BottomLeft:
                AlignBottomLeft(itemsPresenter);

                break;

            case NotificationAlignment.BottomCenter:
                AlignBottomCenter(itemsPresenter);

                break;

            case NotificationAlignment.BottomRight:
                AlignBottomRight(itemsPresenter);

                break;
        }
    }
}