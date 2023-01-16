// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace VacuumBreather.Mvvm.Lifecycle
{
    /// <summary>Provides extension methods for the <see cref="ValueTask" /> type.</summary>
    public static class ValueTaskExtensions
    {
        public static void Forget(this ValueTask task)
        {
        }
    }
}