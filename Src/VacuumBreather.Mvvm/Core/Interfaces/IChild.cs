using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes a node within a parent/child hierarchy.</summary>
[PublicAPI]
public interface IChild
{
    /// <summary>Gets or sets the parent.</summary>
    object? Parent { get; set; }
}

/// <summary>Denotes a node within a parent/child hierarchy.</summary>
/// <typeparam name="TParent">The type of parent.</typeparam>
[PublicAPI]
public interface IChild<TParent> : IChild
{
    /// <summary>Gets or sets the parent.</summary>
    new TParent? Parent { get; set; }
}