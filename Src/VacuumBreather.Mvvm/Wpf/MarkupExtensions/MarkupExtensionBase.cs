using System;
using System.Windows;
using System.Windows.Markup;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VacuumBreather.Mvvm.Wpf.ValueConverters;

namespace VacuumBreather.Mvvm.Wpf.MarkupExtensions;

/// <summary>Base class for markup extensions with access to logging and a <see cref="IServiceProvider"/>.</summary>
/// <seealso cref="System.Windows.Markup.MarkupExtension"/>
[PublicAPI]
public abstract class MarkupExtensionBase : MarkupExtension
{
    private static readonly Lazy<ILogger> LazyLogger = new(GetLogger);
    private static readonly Lazy<IServiceProvider> LazyServiceProvider = new(GetServiceProvider);

    /// <summary>Gets the <see cref="ILogger"/> for this converter.</summary>
    protected static ILogger Logger => LazyLogger.Value;

    /// <summary>Gets the <see cref="IServiceProvider"/> for this converter.</summary>
    protected static IServiceProvider ServiceProvider => LazyServiceProvider.Value;

    private static ILogger GetLogger()
    {
        var logger = (ILogger?)ServiceProvider.GetService(typeof(ILogger<MathExpressionConverter>));

        return logger ?? NullLogger.Instance;
    }

    private static IServiceProvider GetServiceProvider()
    {
        var serviceProvider = (IServiceProvider?)Application.Current?.TryFindResource(nameof(IServiceProvider));

        return serviceProvider ?? NullProvider.Instance;
    }

    private sealed class NullProvider : IServiceProvider
    {
        internal static IServiceProvider Instance { get; } = new NullProvider();

        public object? GetService(Type serviceType)
        {
            return null;
        }
    }
}