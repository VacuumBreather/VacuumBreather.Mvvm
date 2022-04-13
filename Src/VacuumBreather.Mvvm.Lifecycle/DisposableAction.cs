namespace VacuumBreather.Mvvm.Lifecycle
{
    using System;
    using CommunityToolkit.Diagnostics;

    /// <summary>Executes an action when disposed.</summary>
    internal sealed class DisposableAction : IDisposable
    {
        #region Constants and Fields

        private readonly Action action;
        private bool isDisposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>Initializes a new instance of the <see cref="DisposableAction" /> class.</summary>
        /// <param name="action">The action to execute on dispose.</param>
        public DisposableAction(Action action)
        {
            Guard.IsNotNull(action, nameof(action));

            this.action = action;
        }

        #endregion

        #region IDisposable Implementation

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

        #endregion
    }
}