using System;

namespace VacuumBreather.Mvvm.Core;

public interface IAsyncOperation : IAwaitable, ICanBeCanceled, IDisposable
{
}