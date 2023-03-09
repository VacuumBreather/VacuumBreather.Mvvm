using System.Runtime.CompilerServices;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Interface defining a type that can be awaited.</summary>
public interface IAwaitable
{
    /// <summary>Gets an awaiter used to await this <see cref="IAwaitable"/>.</summary>
    /// <returns>An awaiter used to await this <see cref="IAwaitable"/> instance.</returns>
    TaskAwaiter GetAwaiter();
}