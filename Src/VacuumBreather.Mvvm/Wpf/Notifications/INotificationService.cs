using System;
using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.Notifications;

/// <summary>Interface for a service handling notifications.</summary>
public interface INotificationService : ICollectionConductor<NotificationScreen>
{
    /// <summary>
    ///     Gets or sets the expiration time after which notifications are automatically closed. The minimum is one
    ///     second.
    /// </summary>
    TimeSpan ExpirationTime { get; set; }

    /// <summary>Shows the specified <see cref="NotificationScreen"/> as a notification.</summary>
    /// <param name="notification">The notification to show.</param>
    /// <param name="cancellationToken">
    ///     (Optional) A cancellation token that can be used by other objects or threads to receive
    ///     notice of cancellation.
    /// </param>
    /// <returns>A ValueTask that represents the asynchronous save operation.</returns>
    ValueTask ShowNotificationAsync(NotificationScreen notification, CancellationToken cancellationToken = default);
}