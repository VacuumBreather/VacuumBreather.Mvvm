using System;

namespace VacuumBreather.Mvvm.Core;

/// <summary>A class representing an ongoing scoped asynchronous operation and allows its cancellation.</summary>
/// <seealso cref="VacuumBreather.Mvvm.Core.IAwaitable"/>
/// <seealso cref="ICancellable"/>
/// <seealso cref="System.IDisposable"/>
public interface IAsyncOperation : IAwaitable, ICancellable, IDisposable
{
}