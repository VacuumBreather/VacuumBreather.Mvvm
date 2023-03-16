using System.Windows;
using VacuumBreather.Mvvm.Wpf.Transitions;

namespace VacuumBreather.Mvvm.Wpf.Dialogs;

/// <summary>Represents a selectable dialog item inside a <see cref="DialogHost"/>.</summary>
/// <seealso cref="TransitioningContentControl"/>
public class DialogItem : TransitioningContentControl
{
    /// <summary>Initializes static members of the <see cref="DialogItem"/> class.</summary>
    static DialogItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogItem), new FrameworkPropertyMetadata(typeof(DialogItem)));
    }
}