using System;
using Microsoft.Extensions.Hosting;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Provides extension methods for the <see cref="IHostBuilder" /> type.</summary>
internal static class HostBuilderExtensions
{
    /// <summary>
    ///     Configures the a host builder with a custom configuration function.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="configure">The configuration function.</param>
    /// <returns>The configured <see cref="IHostBuilder" />.</returns>
    public static IHostBuilder ConfigureHostBuilder(this IHostBuilder hostBuilder,
                                                    Func<IHostBuilder, IHostBuilder> configure)
    {
        return configure(hostBuilder);
    }
}