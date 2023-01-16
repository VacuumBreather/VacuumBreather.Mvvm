// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A command which relays its execution to an asynchronous delegate.</summary>
    public sealed class AsyncRelayCommand : IAsyncCommand
    {
        /// <summary>A command which does nothing and can always be executed.</summary>
        public static readonly AsyncRelayCommand DoNothing = new(() => ValueTask.CompletedTask);

        private readonly AsyncGuard asyncGuard = new();

        private readonly Func<bool>? canExecute;

        private readonly Func<ValueTask> execute;

        /// <summary>Initializes a new instance of the <see cref="AsyncRelayCommand" /> class.</summary>
        /// <param name="execute">The asynchronous action to perform when the command is executed.</param>
        /// <param name="canExecute">(Optional) The predicate which checks if the command can be executed.</param>
        public AsyncRelayCommand(Func<ValueTask> execute, Func<bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;

            this.asyncGuard.IsOngoingChanged += (_, _) => RaiseCanExecuteChanged();
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public bool CanExecute()
        {
            return !this.asyncGuard.IsOngoing && (this.canExecute?.Invoke() ?? true);
        }

        /// <inheritdoc />
        public async ValueTask ExecuteAsync()
        {
            if (CanExecute())
            {
                await this.execute().Using(this.asyncGuard).ConfigureAwait(false);
            }
        }

        /// <summary>Raises the <see cref="CanExecuteChanged" /> event.</summary>
        public void RaiseCanExecuteChanged()
        {
            Execute.OnUIThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
        }

        /// <inheritdoc />
        bool ICommand.CanExecute(object? parameter)
        {
            return CanExecute();
        }

        /// <inheritdoc />
        void ICommand.Execute(object? parameter)
        {
            ExecuteAsync().Forget();
        }
    }

    /// <summary>A command which relays its execution to an asynchronous delegate.</summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
#pragma warning disable SA1402 // File may only contain a single type
    public sealed class AsyncRelayCommand<T> : IAsyncCommand<T>
#pragma warning restore SA1402 // File may only contain a single type
    {
        /// <summary>A command which does nothing and can always be executed.</summary>
        public static readonly AsyncRelayCommand<T> DoNothing = new(_ => ValueTask.CompletedTask);

        private readonly AsyncGuard asyncGuard = new();

        private readonly Func<T?, bool>? canExecute;

        private readonly Func<T?, ValueTask> execute;

        /// <summary>Initializes a new instance of the <see cref="AsyncRelayCommand{T}" /> class.</summary>
        /// <param name="execute">The asynchronous action to perform when the command is executed.</param>
        /// <param name="canExecute">(Optional) The predicate which checks if the command can be executed.</param>
        public AsyncRelayCommand(Func<T?, ValueTask> execute, Func<T?, bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;

            this.asyncGuard.IsOngoingChanged += (_, _) => RaiseCanExecuteChanged();
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public bool CanExecute(T? parameter)
        {
            return !this.asyncGuard.IsOngoing && (this.canExecute?.Invoke(parameter) ?? true);
        }

        /// <inheritdoc />
        public async ValueTask ExecuteAsync(T? parameter)
        {
            if (CanExecute(parameter))
            {
                await this.execute(parameter).Using(this.asyncGuard).ConfigureAwait(false);
            }
        }

        /// <summary>Raises the <see cref="CanExecuteChanged" /> event.</summary>
        public void RaiseCanExecuteChanged()
        {
            Execute.OnUIThread(() => CanExecuteChanged?.Invoke(this, EventArgs.Empty));
        }

        /// <inheritdoc />
        bool ICommand.CanExecute(object? parameter)
        {
            return CanExecute((T?)parameter);
        }

        /// <inheritdoc />
        void ICommand.Execute(object? parameter)
        {
            ExecuteAsync((T?)parameter).Forget();
        }
    }
}