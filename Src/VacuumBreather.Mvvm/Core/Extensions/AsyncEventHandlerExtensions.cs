﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="AsyncEventHandler{TEventArgs}"/> type.</summary>
[PublicAPI]
public static class AsyncEventHandlerExtensions
{
    /// <summary>Gets all the event handlers attached to this delegate instance.</summary>
    /// <param name="handler">The event handler delegate instance.</param>
    /// <typeparam name="TEventArgs">The type of the event data generated by the event.</typeparam>
    /// <returns>All event handlers attached to the delegate instance.</returns>
    public static IEnumerable<AsyncEventHandler<TEventArgs>> GetHandlers<TEventArgs>(
        this AsyncEventHandler<TEventArgs> handler)
        where TEventArgs : EventArgs
    {
        return handler.GetInvocationList().Cast<AsyncEventHandler<TEventArgs>>();
    }

    /// <summary>Invokes all event handlers asynchronously.</summary>
    /// <param name="handler">The event handler delegate instance.</param>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An object that contains the event data.</param>
    /// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
    /// <typeparam name="TEventArgs">The type of the event data generated by the event.</typeparam>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous execution of all event handlers.</returns>
    public static async ValueTask InvokeAllAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handler,
                                                             object sender,
                                                             TEventArgs e,
                                                             CancellationToken cancellationToken = default)
        where TEventArgs : EventArgs
    {
        await Task.WhenAll(handler.GetHandlers()
                                  .Select(handleAsync => handleAsync(sender, e, cancellationToken))
                                  .Select(t => t.AsTask()));
    }
}