using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Denotes an instance which has a display name.</summary>
[PublicAPI]
public interface IHaveDisplayName
{
    /// <summary>Gets or sets the display name.</summary>
    string DisplayName { get; set; }
}