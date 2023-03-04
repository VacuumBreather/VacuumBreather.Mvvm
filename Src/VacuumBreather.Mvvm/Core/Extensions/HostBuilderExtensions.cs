using Microsoft.Extensions.Hosting;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Provides extension methods for the <see cref="IHostBuilder" /> type.
///     types.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    ///     Registers a module and allows it to add its services to the dependency injection container.
    /// </summary>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <param name="hostBuilder">The <see cref="IHostBuilder" /> responsible for building the dependency injection container.</param>
    /// <returns>The same instance of the <see cref="IHostBuilder" /> for chaining.</returns>
    public static IHostBuilder RegisterModule<TModule>(this IHostBuilder hostBuilder)
        where TModule : IModule, new()
    {
        var module = new TModule();

        return hostBuilder.ConfigureServices((context, services) => module.ConfigureServices(context, services));
    }

    /// <summary>
    ///     Registers a module and allows it to add its services to the dependency injection container.
    ///     This overload allows the module direct access to the underlying container builder.
    /// </summary>
    /// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
    /// <typeparam name="TModule">The type of the module.</typeparam>
    /// <param name="hostBuilder">The <see cref="IHostBuilder" /> responsible for building the dependency injection container.</param>
    /// <returns>The same instance of the <see cref="IHostBuilder" /> for chaining.</returns>
    public static IHostBuilder RegisterModule<TContainerBuilder, TModule>(this IHostBuilder hostBuilder)
        where TModule : IModule<TContainerBuilder>, new()
    {
        var module = new TModule();

        return hostBuilder.ConfigureServices((context, services) => module.ConfigureServices(context, services))
                          .ConfigureContainer<TContainerBuilder>(containerBuilder =>
                                                                     module.ConfigureContainer(containerBuilder));
    }
}