using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VacuumBreather.Mvvm.Core;

/// <summary>
///     Interface for a module extending the main application.
/// </summary>
[PublicAPI]
public interface IModule
{
    /// <summary>
    ///     Adds services of this module to the container.
    /// </summary>
    /// <param name="context">
    ///     Context containing the common services on the <see cref="IHost" />. Some properties may be null
    ///     until set by the <see cref="IHost" />.
    /// </param>
    /// <param name="services">The collection of service descriptors.</param>
    void ConfigureServices(HostBuilderContext context, IServiceCollection services);
}

/// <summary>
///     Interface for a module extending the main application, configuring a specific container builder.
/// </summary>
/// <typeparam name="TContainerBuilder">The type of the container builder.</typeparam>
[PublicAPI]
public interface IModule<in TContainerBuilder> : IModule
{
    /// <summary>
    ///     Enables this module to configure the dependency injection container builder.
    /// </summary>
    /// <param name="builder">The dependency injection container builder to configure.</param>
    void ConfigureContainer(TContainerBuilder builder);
}