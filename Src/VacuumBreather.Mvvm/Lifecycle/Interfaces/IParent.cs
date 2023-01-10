// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System.Collections;
using System.Collections.Generic;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Interface used to define an object associated to a collection of children.</summary>
    public interface IParent
    {
        /// <summary>Gets the children.</summary>
        /// <returns>The collection of children.</returns>
        IEnumerable GetChildren();
    }

    /// <summary>Interface used to define a specialized parent.</summary>
    /// <typeparam name="T">The type of children.</typeparam>
    public interface IParent<out T> : IParent
    {
        /// <summary>Gets the children.</summary>
        /// <returns>The collection of children.</returns>
        new IEnumerable<T> GetChildren();
    }
}