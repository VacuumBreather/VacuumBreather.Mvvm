using System;
using System.ComponentModel;
using JetBrains.Annotations;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Provides extension methods for the <see cref="RelayCommand"/> type.</summary>
[PublicAPI]
public static class RelayCommandExtensions
{
    /// <summary>
    ///     Causes this command to raise its <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> event whenever
    ///     the specified <see cref="INotifyPropertyChanged"/> instance raises its
    ///     <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="notifier">The <see cref="INotifyPropertyChanged"/> implementing object. </param>
    /// <returns>The command that was passed in for chaining.</returns>
    public static RelayCommand RefreshWith(this RelayCommand command, INotifyPropertyChanged notifier)
    {
        var weakReference = new WeakReference(command);
        notifier.PropertyChanged += (_, _) => ((IRaisingCommand?)weakReference.Target)?.Refresh();

        return command;
    }

    /// <summary>
    ///     Causes this command to raise its <see cref="System.Windows.Input.ICommand.CanExecuteChanged"/> event whenever
    ///     the specified <see cref="AsyncGuard"/> raises its <see cref="AsyncGuard.IsOngoingChanged"/> event.
    /// </summary>
    /// <param name="command">The command that should be refreshed.</param>
    /// <param name="guard">The <see cref="AsyncGuard"/> to subscribe to.</param>
    /// <returns>The command that was passed in for chaining.</returns>
    public static RelayCommand RefreshWith(this RelayCommand command, AsyncGuard guard)
    {
        var weakReference = new WeakReference(command);

        guard.IsOngoingChanged += (_, _) => ((IRaisingCommand?)weakReference.Target)?.Refresh();

        return command;
    }
}