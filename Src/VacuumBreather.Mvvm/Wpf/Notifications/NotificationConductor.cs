using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.Notifications;

/// <summary>A conductor handling notifications.</summary>
[PublicAPI]
public class NotificationConductor : ConductorCollectionAllActive<NotificationScreen>, INotificationService
{
    private static readonly TimeSpan MinimumExpirationTime = TimeSpan.FromSeconds(value: 1);

    private TimeSpan _expirationTime = TimeSpan.FromSeconds(value: 10);

    /// <inheritdoc/>
    public TimeSpan ExpirationTime
    {
        get => _expirationTime;
        set => SetProperty(ref _expirationTime, value < MinimumExpirationTime ? MinimumExpirationTime : value);
    }

    /// <inheritdoc/>
    public ValueTask ShowNotificationAsync(NotificationScreen notification,
                                           CancellationToken cancellationToken = default)
    {
        Guard.IsFalse(Items.Contains(notification),
                      nameof(notification),
                      $"Attempting to show a {notification.GetType().Name} notification with the same instance multiple times simultaneously.");

        return ActivateItemAsync(notification, cancellationToken);
    }
}