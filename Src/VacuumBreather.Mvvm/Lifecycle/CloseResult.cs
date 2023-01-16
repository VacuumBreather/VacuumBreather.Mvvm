// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>The result of a test whether an instance can be closed.</summary>
    /// <typeparam name="T">The type of the children of the instance.</typeparam>
    public class CloseResult<T> : ICloseResult<T>
    {
        /// <summary>Initializes a new instance of the <see cref="CloseResult{T}" /> class.</summary>
        /// <param name="closeCanOccur">Whether of not a close operation should occur.</param>
        /// <param name="children">The children of the instance that can be closed.</param>
        public CloseResult(bool closeCanOccur, IEnumerable<T> children)
        {
            CloseCanOccur = closeCanOccur;
            Children = children;
        }

        /// <summary>Gets the children of the instance that can be closed.</summary>
        public IEnumerable<T> Children { get; }

        /// <summary>Gets a value indicating whether a close operation should occur.</summary>
        public bool CloseCanOccur { get; }
    }
}