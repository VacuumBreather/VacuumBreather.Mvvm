using System;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     A class representing an ongoing scoped asynchronous operation and allows its cancellation.
/// </summary>
/// <seealso cref="VacuumBreather.Mvvm.Core.IAwaitable" />
/// <seealso cref="VacuumBreather.Mvvm.Core.ICanBeCanceled" />
/// <seealso cref="System.IDisposable" />
public interface IAsyncOperation : IAwaitable, ICanBeCanceled, IDisposable
{
}