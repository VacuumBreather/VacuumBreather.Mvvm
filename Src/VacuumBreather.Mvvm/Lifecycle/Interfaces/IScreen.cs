// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System.ComponentModel;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>
    ///     Denotes an instance which implements <see cref="IHaveDisplayName" />,
    ///     <see cref="IActivate" />, <see cref="IDeactivate" />, <see cref="IGuardClose" /> and
    ///     <see cref="INotifyPropertyChanged" />.
    /// </summary>
    public interface IScreen : IHaveDisplayName, IActivate, IDeactivate, IGuardClose, INotifyPropertyChanged
    {
    }
}