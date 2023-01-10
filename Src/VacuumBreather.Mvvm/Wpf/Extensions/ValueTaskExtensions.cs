// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Provides extension methods for the <see cref="ValueTask" /> type.</summary>
    public static class ValueTaskExtensions
    {
        /// <summary>Uses the provided ticket to guard this task while it is running.</summary>
        /// <param name="task">The task to guard.</param>
        /// <param name="asyncGuard">The <see cref="AsyncGuard" /> to track the task with.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public static async ValueTask Using(this ValueTask task, AsyncGuard asyncGuard)
        {
            using (asyncGuard.GetToken())
            {
                await task.ConfigureAwait(false);
            }
        }
    }
}