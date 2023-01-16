// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     Event arguments for the <see cref="IDeactivate.Deactivating" />
    ///     <see cref="IDeactivate.Deactivated" /> events.
    /// </summary>
    public class DeactivationEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeactivationEventArgs" /> class.
        /// </summary>
        /// <param name="wasClosed">A value indicating whether the sender was closed in addition to being deactivated.</param>
        public DeactivationEventArgs(bool wasClosed)
        {
            WasClosed = wasClosed;
        }

        /// <summary>Gets a value indicating whether the sender was closed in addition to being deactivated.</summary>
        public bool WasClosed { get; }
    }
}