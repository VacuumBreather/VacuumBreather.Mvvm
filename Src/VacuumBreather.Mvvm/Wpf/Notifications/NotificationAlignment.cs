using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Notifications;

/// <summary>Defines how notifications are aligned within their host control.</summary>
[PublicAPI]
public enum NotificationAlignment
{
    /// <summary>The notifications not shown at all.</summary>
    None = 0,

    /// <summary>The notifications are shown top left aligned.</summary>
    TopLeft = 1,

    /// <summary>The notifications are shown top center aligned.</summary>
    TopCenter = 2,

    /// <summary>The notifications are shown top right aligned.</summary>
    TopRight = 3,

    /// <summary>The notifications are shown bottom left aligned.</summary>
    BottomLeft = 4,

    /// <summary>The notifications are shown bottom center aligned.</summary>
    BottomCenter = 5,

    /// <summary>The notifications are shown bottom right aligned.</summary>
    BottomRight = 6,
}