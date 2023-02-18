// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A conductor for dialogs.</summary>
    public class DialogConductor : ConductorCollectionOneActive<DialogScreen>, IDialogConductor
    {
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
            await ActivateItemAsync(dialog, cancellationToken).ConfigureAwait(false);

            return await dialog.GetDialogResultAsync().ConfigureAwait(false);
        }
    }
}