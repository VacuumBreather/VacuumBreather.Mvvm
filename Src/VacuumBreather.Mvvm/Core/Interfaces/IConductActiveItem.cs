﻿using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>An <see cref="IConductor"/> that also implements <see cref="IHaveActiveItem"/>.</summary>
[PublicAPI]
public interface IConductActiveItem : IConductor, IHaveActiveItem
{
}

/// <summary>An <see cref="IConductor{T}"/> that also implements <see cref="IHaveActiveItem"/>.</summary>
/// <typeparam name="T">The type of item to conduct.</typeparam>
[PublicAPI]
public interface IConductActiveItem<T> : IConductor<T>, IConductActiveItem, IHaveActiveItem<T>
{
}