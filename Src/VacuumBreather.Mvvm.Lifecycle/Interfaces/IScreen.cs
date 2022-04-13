namespace VacuumBreather.Mvvm.Lifecycle
{
    using System.ComponentModel;

    /// <summary>
    ///     Denotes an instance which implements <see cref="IHaveDisplayName" />,
    ///     <see cref="IActivate" />, <see cref="IDeactivate" />, <see cref="IGuardClose" /> and
    ///     <see cref="INotifyPropertyChanged" />
    /// </summary>
    public interface IScreen : IHaveDisplayName, IActivate, IDeactivate, IGuardClose, INotifyPropertyChanged
    {
    }
}