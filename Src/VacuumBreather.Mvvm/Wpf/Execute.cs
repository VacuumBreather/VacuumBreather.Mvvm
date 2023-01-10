// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Enables easy marshalling of code to the UI thread.</summary>
    public static class Execute
    {
        private static Dispatcher Dispatcher => Application.Current.Dispatcher;

        /// <summary>Executes the action on the UI thread asynchronously.</summary>
        /// <param name="action">The action to execute.</param>
        public static void BeginOnUIThread(this Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        /// <summary>Executes the action on the UI thread.</summary>
        /// <param name="action">The action to execute.</param>
        public static void OnUIThread(this Action action)
        {
            if (Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.Invoke(action);
            }
        }

        /// <summary>Executes the action on the UI thread asynchronously.</summary>
        /// <param name="action">The action to execute.</param>
        /// <returns>A ValueTask that represents the asynchronous operation.</returns>
        public static async ValueTask OnUIThreadAsync(this Func<ValueTask> action)
        {
            await Dispatcher.InvokeAsync(action);
        }
    }
}