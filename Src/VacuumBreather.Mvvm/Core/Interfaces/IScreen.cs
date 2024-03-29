﻿using System.ComponentModel;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Denotes an instance which implements <see cref="IHaveDisplayName"/>, <see cref="IActivate"/>,
///     <see cref="IDeactivate"/>, <see cref="IGuardClose"/> and <see cref="INotifyPropertyChanged"/>.
/// </summary>
[PublicAPI]
public interface IScreen : IHaveDisplayName, IActivate, IDeactivate, IGuardClose, INotifyPropertyChanged
{
}