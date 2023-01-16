// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A logger writing to the standard debug output.</summary>
    public class DebugLogger : ILogger
    {
        private readonly string categoryName;
        private readonly LogLevel minimumLogLevel;

        /// <summary>Initializes a new instance of the <see cref="DebugLogger" /> class.</summary>
        /// <param name="categoryName">The category this logger is used by.</param>
        /// <param name="minimumLogLevel">The minimum <see cref="LogLevel" /> this logger will handle.</param>
        public DebugLogger(string categoryName, LogLevel minimumLogLevel = LogLevel.Information)
        {
            this.minimumLogLevel = minimumLogLevel;
            this.categoryName = string.IsNullOrEmpty(categoryName) ? nameof(DebugLogger) : categoryName;
        }

        /// <summary>Scoped logging is not supported by this logger.</summary>
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
            return Disposable.Empty;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return Debugger.IsAttached &&
                   logLevel != LogLevel.None &&
                   logLevel >= this.minimumLogLevel;
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            message = $"[{this.categoryName}] [{logLevel}] {message}";

            Debug.WriteLine(message);
        }
    }
}