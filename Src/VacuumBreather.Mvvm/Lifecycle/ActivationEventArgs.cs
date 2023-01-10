// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Event arguments for the <see cref="IActivate.Activated" /> event.</summary>
    public class ActivationEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ActivationEventArgs" /> class.
        /// </summary>
        /// <param name="wasInitialized">A value indicating whether the sender was initialized in addition to being activated.</param>
        public ActivationEventArgs(bool wasInitialized)
        {
            WasInitialized = wasInitialized;
        }

        /// <summary>Gets a value indicating whether the sender was initialized in addition to being activated.</summary>
        public bool WasInitialized { get; }
    }
}