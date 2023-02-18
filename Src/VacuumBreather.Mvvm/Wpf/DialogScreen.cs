// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A base class for dialog screens.</summary>
    public abstract class DialogScreen : Screen
    {
        private readonly TaskCompletionSource<bool?> taskCompletionSource = new();
        private bool? result;

        /// <summary>Initializes a new instance of the <see cref="DialogScreen" /> class.</summary>
        protected DialogScreen()
        {
            CloseDialogCommand = new AsyncRelayCommand<bool?>(r => TryCloseAsync(r));
        }

        /// <summary>Gets the command to close the dialog.</summary>
        public AsyncRelayCommand<bool?> CloseDialogCommand { get; }

        /// <summary>
        ///     Gets the result this dialog was closed with.
        /// </summary>
        /// <returns>
        ///     A <see cref="ValueTask" /> representing the asynchronous operation.
        ///     The ValueTask result contains the result this dialog was closed with.
        /// </returns>
        public async ValueTask<bool?> GetDialogResultAsync()
        {
            return await this.taskCompletionSource.Task;
        }

        /// <inheritdoc />
        public sealed override ValueTask TryCloseAsync(bool? dialogResult = null,
            CancellationToken cancellationToken = default)
        {
            this.result = dialogResult;

            return base.TryCloseAsync(dialogResult, cancellationToken);
        }

        /// <inheritdoc />
        protected override ValueTask OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this.taskCompletionSource.TrySetResult(this.result);
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        /// <inheritdoc />
        public sealed override ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(true);
        }
    }
}