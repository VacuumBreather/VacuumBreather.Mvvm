using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf.Notifications;

/// <summary>Represents a notification that is displayed to inform the user.</summary>
[PublicAPI]
public class NotificationScreen : Screen
{
    private static readonly TimeSpan MinimumExpirationTime = TimeSpan.FromSeconds(value: 1);
    private IAsyncOperation? _showOperation;

    /// <summary>Initializes a new instance of the <see cref="NotificationScreen"/> class.</summary>
    /// <param name="title">The title of the notification.</param>
    /// <param name="content">The content of the notification.</param>
    /// <param name="type">The type of the notification.</param>
    /// <param name="expirationTime">
    ///     (Optional) The expiration time after which notifications are automatically closed. The
    ///     minimum is one second. If this is not provided the global expiration time set on the
    ///     <see cref="INotificationService"/> will be used.
    /// </param>
    public NotificationScreen(string title,
                              string content,
                              NotificationType type = NotificationType.Information,
                              TimeSpan? expirationTime = null)
    {
        Title = title;
        Content = content;
        Type = type;

        if (expirationTime is not null)
        {
            expirationTime = expirationTime.Value < MinimumExpirationTime
                                 ? MinimumExpirationTime
                                 : expirationTime.Value;
        }

        ExpirationTime = expirationTime;
        CloseCommand = new RelayCommand(Close);
    }

    /// <summary>Gets the command to close the notification.</summary>
    public IRaisingCommand CloseCommand { get; }

    /// <summary>Gets the content of the notification.</summary>
    public string Content { get; }

    /// <summary>
    ///     Gets the expiration time after which notifications are automatically closed. The minimum is one second. If
    ///     this is <see cref="TimeSpan.Zero"/>, the global expiration time set on the <see cref="INotificationService"/> will
    ///     be used.
    /// </summary>
    public TimeSpan? ExpirationTime { get; }

    /// <summary>Gets the title of the dialog.</summary>
    public string Title { get; }

    /// <summary>Gets the type of the notification.</summary>
    public NotificationType Type { get; }

    /// <inheritdoc/>
    protected override ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        if (Parent is not NotificationConductor notificationConductor)
        {
            throw new InvalidOperationException(
                $"A {nameof(NotificationScreen)} must be conducted by a {nameof(NotificationConductor)}.");
        }

        var usedExpirationTime = ExpirationTime ?? notificationConductor.ExpirationTime;

        ThreadHelper.RunOnUIThreadAndForget(async () =>
                                            {
                                                using var operation = AsyncHelper
                                                                      .CreateAsyncOperation(cancellationToken)
                                                                      .Assign(out _showOperation);

                                                try
                                                {
                                                    await Task.Delay(usedExpirationTime, operation.Token);
                                                }
                                                finally
                                                {
                                                    await TryCloseAsync(CancellationToken.None);
                                                }
                                            },
                                            cancellationToken: cancellationToken);

        return ValueTask.CompletedTask;
    }

    private void Close()
    {
        _showOperation?.Cancel();
    }
}