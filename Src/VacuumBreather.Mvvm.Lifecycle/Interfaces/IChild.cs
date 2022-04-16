// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>Denotes a node within a parent/child hierarchy.</summary>
public interface IChild
{
    /// <summary>Gets or sets the parent.</summary>
    object? Parent { get; set; }
}

/// <summary>Denotes a node within a parent/child hierarchy.</summary>
/// <typeparam name="TParent">The type of parent.</typeparam>
public interface IChild<TParent> : IChild
{
    /// <summary>Gets or sets the parent.</summary>
    new TParent? Parent { get; set; }
}