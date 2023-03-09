namespace VacuumBreather.Mvvm.Core;

/// <summary>Interface for types which can have asynchronous operations and needs to notify about their ongoing state.</summary>
public interface IHaveAsynchronousOperations
{
    /// <summary>Gets a value indicating whether this <see cref="BindableObject"/> has any ongoing asynchronous operations.</summary>
    /// <value>
    ///     <see langword="true"/> if this <see cref="BindableObject"/> has any ongoing asynchronous operations; otherwise,
    ///     <see langword="false"/>.
    /// </value>
    bool IsBusy { get; }
}