﻿// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Represents the asynchronous method that will handle an event when the event provides data.</summary>
/// <typeparam name="TEventArgs">The type of the event data generated by the event.</typeparam>
/// <param name="sender">The source of the event.</param>
/// <param name="e">An object that contains the event data.</param>
/// <param name="cancellationToken">(Optional) The cancellation token to cancel operation.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is a special type of event handler delegate. The name is appropriate.")]
public delegate Task AsyncEventHandler<in TEventArgs>(
    object sender,
    TEventArgs e,
    CancellationToken cancellationToken = default)
    where TEventArgs : EventArgs;