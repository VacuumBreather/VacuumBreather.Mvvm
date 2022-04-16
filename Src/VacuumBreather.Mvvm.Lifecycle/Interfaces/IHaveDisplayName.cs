// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Denotes an instance which has a display name.</summary>
public interface IHaveDisplayName
{
    /// <summary>Gets or sets the display name.</summary>
    string DisplayName { get; set; }
}