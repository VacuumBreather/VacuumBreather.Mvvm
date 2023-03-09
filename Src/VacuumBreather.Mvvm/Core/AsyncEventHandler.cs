﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Represents the asynchronous method that will handle an event when the event provides data.</summary>
/// <typeparam name="TEventArgs">The type of the event data generated by the event.</typeparam>
/// <param name="sender">The source of the event.</param>
/// <param name="e">An object that contains the event data.</param>
/// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
[SuppressMessage(category: "Naming",
                 checkId: "CA1711:Identifiers should not have incorrect suffix",
                 Justification = "This is a special type of event handler delegate. The name is appropriate.")]
public delegate ValueTask AsyncEventHandler<in TEventArgs>(object sender,
                                                           TEventArgs e,
                                                           CancellationToken cancellationToken = default)
    where TEventArgs : EventArgs;