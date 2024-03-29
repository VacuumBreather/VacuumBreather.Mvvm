﻿using System.Windows.Input;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Defines a command which can be told to raise its <see cref="ICommand.CanExecuteChanged"/> event.</summary>
public interface IRaisingCommand : ICommand
{
    /// <summary>Raises the <see cref="ICommand.CanExecuteChanged"/> event.</summary>
    void Refresh();
}