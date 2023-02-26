using System.Collections.Generic;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Results from the close strategy.</summary>
/// <typeparam name="T">The type of child element.</typeparam>
[PublicAPI]
public interface ICloseResult<out T>
{
    /// <summary>Gets the children which should close even if the parent cannot.</summary>
    IEnumerable<T> Children { get; }

    /// <summary>Gets a value indicating whether a close operation can occur.</summary>
    bool CloseCanOccur { get; }
}