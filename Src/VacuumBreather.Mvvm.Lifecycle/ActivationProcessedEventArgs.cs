namespace VacuumBreather.Mvvm.Lifecycle
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    ///     Event arguments for the <see cref="IConductor.ActivationProcessed" /> event. Contains
    ///     details about the success or failure of an item's activation through an
    ///     <see cref="IConductor" />.
    /// </summary>
    [PublicAPI]
    public class ActivationProcessedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <inheritdoc />
        public ActivationProcessedEventArgs(object item, bool success)
        {
            Item = item;
            Success = success;
        }

        #endregion

        #region Public Properties

        /// <summary>The item whose activation was processed.</summary>
        public object Item { get; set; }

        /// <summary>Gets or sets a value indicating whether the activation was a success.</summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get; set; }

        #endregion
    }
}