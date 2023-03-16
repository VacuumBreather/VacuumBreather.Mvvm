using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Dialogs;

/// <summary>Represents a control that contains opened <see cref="DialogItem"/> items.</summary>
/// <seealso cref="ContentControl"/>
[PublicAPI]
[TemplatePart(Name = RootGridPartName, Type = typeof(Grid))]
[StyleTypedProperty(Property = nameof(DialogContainerStyle), StyleTargetType = typeof(DialogItem))]
public class DialogHost : ContentControl
{
    /// <summary>The name of the root grid template part.</summary>
    public const string RootGridPartName = "PART_RootGrid";

    /// <summary>Identifies the <see cref="DialogContainerStyle"/> dependency property.</summary>
    public static readonly DependencyProperty DialogContainerStyleProperty =
        DependencyProperty.Register(nameof(DialogContainerStyle),
                                    typeof(Style),
                                    typeof(DialogHost),
                                    new PropertyMetadata(default(Style)));

    /// <summary>Identifies the <see cref="OverlayBackgroundBrush"/> dependency property.</summary>
    public static readonly DependencyProperty OverlayBackgroundBrushProperty =
        DependencyProperty.Register(nameof(OverlayBackgroundBrush),
                                    typeof(Brush),
                                    typeof(DialogHost),
                                    new PropertyMetadata(Brushes.Transparent));

    private Grid? _rootGrid;

    /// <summary>Initializes static members of the <see cref="DialogHost"/> class.</summary>
    static DialogHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogHost), new FrameworkPropertyMetadata(typeof(DialogHost)));
    }

    /// <summary>Initializes a new instance of the <see cref="DialogHost"/> class.</summary>
    public DialogHost()
    {
        Loaded += OnLoaded;
    }

    /// <summary>Gets or sets the <see cref="Style"/> to be used on the <see cref="DialogItem"/> container.</summary>
    public Style? DialogContainerStyle
    {
        get => (Style?)GetValue(DialogContainerStyleProperty);
        set => SetValue(DialogContainerStyleProperty, value);
    }

    /// <summary>Gets or sets the brush used for the background which overlays the regular UI while a dialog is open.</summary>
    public Brush OverlayBackgroundBrush
    {
        get => (Brush)GetValue(OverlayBackgroundBrushProperty);
        set => SetValue(OverlayBackgroundBrushProperty, value);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        _rootGrid = GetTemplateChild(RootGridPartName) as Grid;

        base.OnApplyTemplate();
    }

    /// <inheritdoc/>
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateOverlayVisibility();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateOverlayVisibility();
    }

    private void UpdateOverlayVisibility()
    {
        _rootGrid?.SetCurrentValue(VisibilityProperty, Content is null ? Visibility.Collapsed : Visibility.Visible);
    }
}