using System;
using System.Windows.Markup;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Wpf.MarkupExtensions;

/// <summary>A <see cref="MarkupExtension"/> used to retrieve a service from the <see cref="IServiceProvider"/>.</summary>
/// <seealso cref="MarkupExtensionBase"/>
/// <seealso cref="System.Windows.Markup.MarkupExtension"/>
[PublicAPI]
[MarkupExtensionReturnType(typeof(object))]
public class GetServiceExtension : MarkupExtensionBase
{
    /// <summary>Initializes a new instance of the <see cref="GetServiceExtension"/> class.</summary>
    /// <param name="serviceType">Type of the service which should be retrieved from the <see cref="IServiceProvider"/>.</param>
    public GetServiceExtension(Type serviceType)
    {
        ServiceType = serviceType;
    }

    /// <summary>Gets or sets the type of the service which should be retrieved from the <see cref="IServiceProvider"/>.</summary>
    /// <value>The type of the service which should be retrieved.</value>
    [ConstructorArgument(argumentName: "serviceType")]
    public Type ServiceType { get; set; }

    /// <summary>Retrieve a service of type <see cref="ServiceType"/> from the <see cref="IServiceProvider"/>.</summary>
    /// <param name="serviceProvider">Unused by this markup extension.</param>
    /// <returns>The service of type <see cref="ServiceType"/>. -or- <see langword="null"/> if there is no such service.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        return ServiceProvider.GetService(ServiceType);
    }
}