using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.Notifications;

/// <summary>Defines the possible types of notifications.</summary>
[PublicAPI]
public enum NotificationType
{
    /// <summary>Default value indicating no notification type has been provided.</summary>
    None = 0,

    /// <summary>A purely informational notification.</summary>
    Information = 1,

    /// <summary>A notification informing the user about a success.</summary>
    Success = 2,

    /// <summary>A notification warning the user about an issue.</summary>
    Warning = 3,

    /// <summary>A notification informing the user about an error that has occured.</summary>
    Error = 4,
}