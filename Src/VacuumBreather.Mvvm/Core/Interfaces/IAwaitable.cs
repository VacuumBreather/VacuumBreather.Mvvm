using System.Runtime.CompilerServices;

namespace VacuumBreather.Mvvm.Core;

public interface IAwaitable
{
    TaskAwaiter GetAwaiter();
}