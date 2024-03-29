﻿using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Interface used to define an object associated to a collection of children.</summary>
[PublicAPI]
public interface IParent
{
    /// <summary>Gets the children.</summary>
    /// <returns>The collection of children.</returns>
    IEnumerable GetChildren();
}

/// <summary>Interface used to define a specialized parent.</summary>
/// <typeparam name="T">The type of children.</typeparam>
[PublicAPI]
public interface IParent<out T> : IParent
{
    /// <summary>Gets the children.</summary>
    /// <returns>The collection of children.</returns>
    new IEnumerable<T> GetChildren();
}