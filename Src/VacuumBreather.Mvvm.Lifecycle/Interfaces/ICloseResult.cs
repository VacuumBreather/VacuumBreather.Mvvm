// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Results from the close strategy.</summary>
/// <typeparam name="T">The type of child element.</typeparam>
public interface ICloseResult<out T>
{
    /// <summary>Gets the children which should close even if the parent cannot.</summary>
    IEnumerable<T> Children { get; }

    /// <summary>Gets a value indicating whether a close operation can occur.</summary>
    bool CloseCanOccur { get; }
}