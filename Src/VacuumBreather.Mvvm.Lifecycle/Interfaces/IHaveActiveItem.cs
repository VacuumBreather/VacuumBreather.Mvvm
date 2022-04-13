namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Denotes an instance which maintains an active item.</summary>
    public interface IHaveActiveItem
    {
        /// <summary>The currently active item.</summary>
        object? ActiveItem { get; }
    }
}