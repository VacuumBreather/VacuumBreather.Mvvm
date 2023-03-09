using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an instance which conducts a collection of other objects by maintaining a strict lifecycle.</summary>
/// <typeparam name="T">The type of item to conduct.</typeparam>
/// <remarks>
///     <para>
///         Conducted instances can opt in to the lifecycle by implementing any of the following <see cref="IActivate"/>
///         , <see cref="IDeactivate"/>, <see cref="IGuardClose"/>.
///     </para>
/// </remarks>
[PublicAPI]
public interface ICollectionConductor<T> : IConductor<T>
{
    /// <summary>Gets the items that are currently being conducted.</summary>
    IBindableCollection<T> Items { get; }
}