// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Denotes an instance which maintains an active item.</summary>
public interface IHaveActiveItem
{
    /// <summary>Gets the currently active item.</summary>
    object? ActiveItem { get; }
}