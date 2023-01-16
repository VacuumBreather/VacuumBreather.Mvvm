// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     Used to gather the results from multiple child elements which may or may not prevent
    ///     closing.
    /// </summary>
    /// <typeparam name="T">The type of child element.</typeparam>
    public class DefaultCloseStrategy<T> : ICloseStrategy<T>
    {
        private readonly bool closeConductedItemsWhenConductorCannotClose;

        /// <summary>Initializes a new instance of the <see cref="DefaultCloseStrategy{T}" /> class.</summary>
        /// <param name="closeConductedItemsWhenConductorCannotClose">
        ///     Indicates that even if all conducted
        ///     items are not closable, those that are should be closed. The default is <see langword="false" />.
        /// </param>
        public DefaultCloseStrategy(bool closeConductedItemsWhenConductorCannotClose = false)
        {
            this.closeConductedItemsWhenConductorCannotClose = closeConductedItemsWhenConductorCannotClose;
        }

        /// <inheritdoc />
        public async ValueTask<ICloseResult<T>> ExecuteAsync(
            IEnumerable<T> toClose,
            CancellationToken cancellationToken = default)
        {
            List<T> closeableChildren = new();
            bool closeCanOccur = true;

            foreach (T child in toClose)
            {
                if (child is IGuardClose guarded)
                {
                    bool canClose = await guarded.CanCloseAsync(cancellationToken).ConfigureAwait(false);

                    if (canClose)
                    {
                        closeableChildren.Add(child);
                    }

                    closeCanOccur = closeCanOccur && canClose;
                }
                else
                {
                    closeableChildren.Add(child);
                }
            }

            if (!this.closeConductedItemsWhenConductorCannotClose && !closeCanOccur)
            {
                closeableChildren.Clear();
            }

            return new CloseResult<T>(closeCanOccur, closeableChildren);
        }
    }
}