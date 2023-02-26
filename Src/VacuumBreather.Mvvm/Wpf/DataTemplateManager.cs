using System;
using System.Collections;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Creates data templates for view-models.</summary>
internal static class DataTemplateManager
{
    private static ILogger? _logger;

    /// <summary>
    ///     Gets or sets the logger for the <see cref="DataTemplateManager" />.
    /// </summary>
    /// <value>
    ///     The logger for the <see cref="DataTemplateManager" />.
    /// </value>
    internal static ILogger Logger
    {
        get => _logger ?? NullLogger.Instance;
        set => _logger = value;
    }

    /// <summary>
    ///     Creates data templates for view-models.
    /// </summary>
    /// <param name="resourceDictionary">
    ///     The <see cref="IDictionary" /> to register the data templates in.
    /// </param>
    /// <param name="onRegister">A callback to execute whenever a data template was registered.</param>
    public static void RegisterDataTemplates(IDictionary resourceDictionary, Action<DataTemplate> onRegister)
    {
        DataTemplate defaultDataTemplate = CreateDefaultDataTemplate();
        RegisterDataTemplate(defaultDataTemplate, resourceDictionary, onRegister);
    }

    private static DataTemplate CreateDefaultDataTemplate()
    {
        Logger.LogDebug("Creating default data template for {ViewModel}", nameof(BindableObject));

        string xamlTemplate = "<DataTemplate\n" +
                              "  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n" +
                              "  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n" +
                              $"  xmlns:vm=\"clr-namespace:{typeof(BindableObject).Namespace};assembly={typeof(BindableObject).Assembly.GetName().Name}\"\n" +
                              $"  xmlns:mvvm=\"clr-namespace:{typeof(View).Namespace};assembly={typeof(View).Assembly.GetName().Name}\"\n" +
                              $"  DataType=\"{{x:Type vm:{nameof(BindableObject)}}}\">\n" +
                              "    <ContentPresenter mvvm:View.IsGenerated=\"True\" mvvm:View.Model=\"{Binding Mode=OneTime}\" />\n" +
                              "</DataTemplate>";

        return (DataTemplate)XamlReader.Parse(xamlTemplate);
    }

    private static void RegisterDataTemplate(DataTemplate dataTemplate,
                                             IDictionary dictionary,
                                             Action<DataTemplate> onRegister)
    {
        onRegister(dataTemplate);
        dataTemplate.Seal();

        dictionary[dataTemplate.DataTemplateKey!] = dataTemplate;
    }
}