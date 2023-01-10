// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Executes an action when disposed.</summary>
    public sealed class DisposableAction : IDisposable
    {
        private readonly Action action;
        private bool isDisposed;

        /// <summary>Initializes a new instance of the <see cref="DisposableAction" /> class.</summary>
        /// <param name="action">The action to execute on dispose.</param>
        public DisposableAction(Action action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>Executes the supplied action.</summary>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.action();
            this.isDisposed = true;
        }
    }
}