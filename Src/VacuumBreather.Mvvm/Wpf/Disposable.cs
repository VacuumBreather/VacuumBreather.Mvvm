// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Provides a disposable that does nothing when disposed.</summary>
    public static class Disposable
    {
        /// <summary>Gets a disposable that does nothing when disposed.</summary>
        public static IDisposable Empty => EmptyDisposable.Instance;

        private class EmptyDisposable : IDisposable
        {
            public static readonly IDisposable Instance = new EmptyDisposable();

            public void Dispose()
            {
            }
        }
    }
}