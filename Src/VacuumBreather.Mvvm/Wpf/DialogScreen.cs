// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A base class for dialog screens.</summary>
    public abstract class DialogScreen : Screen
    {
        /// <summary>Initializes a new instance of the <see cref="DialogScreen" /> class.</summary>
        protected DialogScreen()
        {
            CloseDialogCommand = new AsyncRelayCommand<bool?>(TryCloseInternalAsync, CanCloseDialog);
        }

        /// <summary>Gets the command to close the dialog.</summary>
        public AsyncRelayCommand<bool?> CloseDialogCommand { get; }

        /// <inheritdoc />
        public sealed override ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(true);
        }

        /// <inheritdoc />
        public sealed override async ValueTask TryCloseAsync(
            bool? dialogResult = null,
            CancellationToken cancellationToken = default)
        {
            if (Parent is not DialogConductor dialogConductor)
            {
                throw new InvalidOperationException($"{this} must be conducted by a {nameof(DialogConductor)}.");
            }

            await dialogConductor.CloseDialogAsync(this, dialogResult, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>Override this to define when the <see cref="CloseDialogCommand" /> can be executed.</summary>
        /// <param name="dialogResult">The dialog result parameter of the <see cref="CloseDialogCommand" />.</param>
        /// <returns>
        ///     <see langword="true" /> if the dialog can be closed with the specified result; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool CanCloseDialog(bool? dialogResult)
        {
            return true;
        }

        private async ValueTask TryCloseInternalAsync(bool? dialogResult)
        {
            await TryCloseAsync(dialogResult).ConfigureAwait(false);
        }
    }
}