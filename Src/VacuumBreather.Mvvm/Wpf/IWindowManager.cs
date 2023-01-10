// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System.Threading;
using System.Threading.Tasks;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A service that manages windows.</summary>
    public interface IWindowManager
    {
        /// <summary>Shows a modal dialog for the specified model.</summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects
        ///     or threads to receive notice of cancellation.
        /// </param>
        /// <returns>The dialog result.</returns>
        ValueTask<bool?> ShowDialogAsync(DialogScreen rootModel, CancellationToken cancellationToken = default);

        /// <summary>Shows the specified model as the main content.</summary>
        /// <param name="rootModel">The root model.</param>
        /// <param name="cancellationToken">
        ///     (Optional) A cancellation token that can be used by other objects
        ///     or threads to receive notice of cancellation.
        /// </param>
        ValueTask ShowMainContentAsync(Screen rootModel, CancellationToken cancellationToken = default);
    }
}