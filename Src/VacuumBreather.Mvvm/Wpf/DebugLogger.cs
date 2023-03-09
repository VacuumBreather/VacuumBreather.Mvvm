using System;
using System.Diagnostics;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>A logger writing to the standard debug output.</summary>
public sealed class DebugLogger : ILogger
{
    private readonly string _categoryName;
    private readonly LogLevel _minimumLogLevel;

    /// <summary>Initializes a new instance of the <see cref="DebugLogger"/> class.</summary>
    /// <param name="categoryName">The category this logger is used by.</param>
    /// <param name="minimumLogLevel">The minimum <see cref="LogLevel"/> this logger will handle.</param>
    public DebugLogger(string categoryName, LogLevel minimumLogLevel = LogLevel.Information)
    {
        _minimumLogLevel = minimumLogLevel;
        _categoryName = string.IsNullOrEmpty(categoryName) ? nameof(DebugLogger) : categoryName;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return Debugger.IsAttached && (logLevel != LogLevel.None) && (logLevel >= _minimumLogLevel);
    }

    /// <inheritdoc/>
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

        Guard.IsNotNull(formatter);

        var message = formatter(state, exception);

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        message = $"[{_categoryName}] [{logLevel}] {message}";

        Debug.WriteLine(message);
    }

    /// <inheritdoc/>
    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return Disposable.Empty;
    }
}