// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     Event arguments for the <see cref="IConductor.ActivationProcessed" /> event. Contains
    ///     details about the success or failure of an item's activation through an
    ///     <see cref="IConductor" />.
    /// </summary>
    public class ActivationProcessedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ActivationProcessedEventArgs" /> class.
        /// </summary>
        /// <param name="item">The item whose activation was processed.</param>
        /// <param name="success">A value indicating whether the activation was a success.</param>
        public ActivationProcessedEventArgs(object item, bool success)
        {
            Item = item;
            Success = success;
        }

        /// <summary>Gets the item whose activation was processed.</summary>
        public object Item { get; }

        /// <summary>Gets a value indicating whether the activation was a success.</summary>
        /// <value><see langword="true" /> if success; otherwise, <see langword="false" />.</value>
        public bool Success { get; }
    }
}