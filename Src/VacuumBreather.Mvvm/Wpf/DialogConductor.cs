// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A conductor for dialogs.</summary>
    public class DialogConductor : Conductor<DialogScreen>, IDialogConductor
    {
        private TaskCompletionSource<bool?>? taskCompletionSource;

        /// <summary>Closes the specified dialog.</summary>
        /// <param name="dialog">The dialog to close.</param>
        /// <param name="dialogResult">The dialog result to pass up the chain.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects
        ///     or threads to receive notice of cancellation.
        /// </param>
        /// <returns>
        ///     A ValueTask that represents the asynchronous save operation.
        /// </returns>
        public async ValueTask CloseDialogAsync(
            DialogScreen dialog,
            bool? dialogResult,
            CancellationToken cancellationToken = default)
        {
            await DeactivateItemAsync(dialog, true, cancellationToken).ConfigureAwait(false);

            if (ActiveItem != dialog)
            {
                this.taskCompletionSource?.TrySetResult(dialogResult);
            }
        }

        /// <summary>Shows the specified <see cref="DialogScreen" /> as a dialog.</summary>
        /// <param name="dialog">The dialog to show.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects
        ///     or threads to receive notice of cancellation.
        /// </param>
        /// <returns>
        ///     A ValueTask that represents the asynchronous save operation. The ValueTask result contains the
        ///     dialog result.
        /// </returns>
        public async ValueTask<bool?> ShowDialogAsync(
            DialogScreen dialog,
            CancellationToken cancellationToken = default)
        {
            this.taskCompletionSource = new TaskCompletionSource<bool?>();

            await ActivateItemAsync(dialog, cancellationToken).ConfigureAwait(false);

            if (ActiveItem == dialog)
            {
                return await this.taskCompletionSource.Task.ConfigureAwait(false);
            }

            return default;
        }

        /// <inheritdoc />
        protected override async ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await ChangeActiveItemAsync(null, true, cancellationToken).ConfigureAwait(false);

            this.taskCompletionSource?.TrySetResult(null);
        }
    }
}