// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace VacuumBreather.Mvvm.Lifecycle;

/// <summary>An <see cref="IConductor" /> that also implements <see cref="IHaveActiveItem" />.</summary>
public interface IConductActiveItem : IConductor, IHaveActiveItem
{
}

/// <summary>An <see cref="IConductor{T}" /> that also implements <see cref="IHaveActiveItem" />.</summary>
/// <typeparam name="T">The type of item to conduct.</typeparam>
public interface IConductActiveItem<T> : IConductor<T>, IConductActiveItem
{
}