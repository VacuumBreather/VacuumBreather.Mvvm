// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Responsible for creating <see cref="Microsoft.Extensions.Logging.ILogger" /> instances.</summary>
    public static class LogManager
    {
        /// <summary>The category name for logs created by the framework itself.</summary>
        public const string FrameworkCategoryName = nameof(VacuumBreather);

        /// <summary>
        ///     Gets or sets the global logger for the framework.
        /// </summary>
        public static ILogger FrameworkLogger { get; set; } = NullLogger.Instance;
    }
}