﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VacuumBreather.Mvvm.Core;

namespace VacuumBreather.Mvvm.Wpf;

/// <summary>Hosts attached properties related to view-models.</summary>
[PublicAPI]
public static class View
{
    /// <summary>
    ///     A dependency property for declaring the name of the content dependency property, to which the view resolved
    ///     from the view-model will be assigned.
    /// </summary>
    public static readonly DependencyProperty ContentPropertyNameProperty = DependencyProperty.RegisterAttached(
        PropertyNameHelper.GetName(nameof(ContentPropertyNameProperty)),
        typeof(string),
        typeof(View),
        new PropertyMetadata(default(string), OnContentPropertyNameChanged));

    /// <summary>A dependency property for marking an element as framework generated.</summary>
    public static readonly DependencyProperty IsGeneratedProperty = DependencyProperty.RegisterAttached(
        PropertyNameHelper.GetName(nameof(IsGeneratedProperty)),
        typeof(bool),
        typeof(View),
        new PropertyMetadata(default(bool)));

    /// <summary>A dependency property for attaching a model to the UI.</summary>
    public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached(
        PropertyNameHelper.GetName(nameof(ModelProperty)),
        typeof(object),
        typeof(View),
        new PropertyMetadata(default, OnModelChanged));

    private const string DefaultContentProperty = nameof(ContentControl.Content);

    private static readonly DependencyProperty ContextProperty = DependencyProperty.RegisterAttached(
        PropertyNameHelper.GetName(nameof(ContextProperty)),
        typeof(Guid),
        typeof(View),
        new PropertyMetadata(default(Guid)));

    private static ILogger? _logger;
    private static bool? _isLoggingDebug;
    private static ViewLocator? _viewLocator;

    /// <summary>Sets the name of the content property, to which the view will be assigned.</summary>
    /// <param name="d">The element the model is attached to.</param>
    /// <returns>The name of the content property.</returns>
    public static string? GetContentPropertyName(DependencyObject d)
    {
        return (string?)d.GetValue(ContentPropertyNameProperty);
    }

    /// <summary>Gets a value indicating whether the element was generated by the framework.</summary>
    /// <param name="element">The element to check.</param>
    /// <returns><see langword="true" /> if the element was generated by the framework; otherwise, <see langword="false" />.</returns>
    public static bool GetIsGenerated(DependencyObject element)
    {
        return (bool)element.GetValue(IsGeneratedProperty);
    }

    /// <summary>Gets the model.</summary>
    /// <param name="d">The element the model is attached to.</param>
    /// <returns>The model.</returns>
    public static object? GetModel(DependencyObject d)
    {
        return d.GetValue(ModelProperty);
    }

    /// <summary>Sets the name of the content property, to which the view will be assigned.</summary>
    /// <param name="d">The element to set the content property name on.</param>
    /// <param name="propertyName">The name of the content property.</param>
    public static void SetContentPropertyName(DependencyObject d, string? propertyName)
    {
        d.SetValue(ContentPropertyNameProperty, propertyName);
    }

    /// <summary>Sets a value indicating whether the element was generated by the framework.</summary>
    /// <param name="element">The element to mark as generated.</param>
    /// <param name="value">
    ///     If set to <see langword="true" /> the element is marked as generated by the framework; otherwise,
    ///     <see langword="false" />.
    /// </param>
    public static void SetIsGenerated(DependencyObject element, bool value)
    {
        element.SetValue(IsGeneratedProperty, value);
    }

    /// <summary>Sets the model.</summary>
    /// <param name="d">The element to attach the model to.</param>
    /// <param name="value">The model.</param>
    public static void SetModel(DependencyObject d, object? value)
    {
        d.SetValue(ModelProperty, value);
    }

    private static string GetAncestorTypeName(DependencyObject? ancestor)
    {
        var ancestorTypeName = ancestor?.GetType().Name;

        ancestorTypeName += ancestor is FrameworkElement element && !string.IsNullOrEmpty(element.Name)
            ? $":'{element.Name}'"
            : string.Empty;

        return ancestorTypeName;
    }

    private static UIElement? GetCachedViewFor(object viewModel, DependencyObject? contextLocation)
    {
        Guid context = GetContext(contextLocation);

        if (viewModel is not IViewAware viewAware || (context == Guid.Empty))
        {
            return null;
        }

        return viewAware.GetView(context) as UIElement;
    }

    private static Guid GetContext(DependencyObject? element)
    {
        return (Guid)(element?.GetValue(ContextProperty) ?? Guid.Empty);
    }

    private static IServiceProvider GetServiceProvider(DependencyObject dependencyObject)
    {
        var frameworkElement = dependencyObject.FindVisualAncestor<FrameworkElement>()!;

        return (IServiceProvider)frameworkElement.TryFindResource(nameof(IServiceProvider));
    }

    private static void OnContentPropertyNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue)
        {
            return;
        }

        SetViewOnContentProperty(d);
    }

    private static void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue)
        {
            return;
        }

        SetViewOnContentProperty(targetLocation);
    }

    private static bool SetContentPropertyValue(object targetLocation, object? view, string? overrideContentProperty)
    {
        if (view is FrameworkElement { Parent: { } } frameworkElement)
        {
            SetContentPropertyValueCore(frameworkElement.Parent, null, overrideContentProperty);
        }

        return SetContentPropertyValueCore(targetLocation, view, overrideContentProperty);
    }

    private static bool SetContentPropertyValueCore(object targetLocation,
                                                    object? view,
                                                    string? overrideContentProperty)
    {
        try
        {
            Type type = targetLocation.GetType();

            string contentProperty = overrideContentProperty ??
                                     type.GetCustomAttributes(typeof(ContentPropertyAttribute), true)
                                         .OfType<ContentPropertyAttribute>()
                                         .FirstOrDefault()
                                         ?.Name ??
                                     DefaultContentProperty;

            PropertyInfo? propertyInfo = type.GetProperty(contentProperty);

            if (propertyInfo == null)
            {
                return false;
            }

            propertyInfo.SetValue(targetLocation, view, null);

            return true;
        }
        catch (Exception exception) when (exception is TypeLoadException or
                                                       ArgumentException or
                                                       InvalidOperationException or
                                                       AmbiguousMatchException or
                                                       TargetException or
                                                       TargetParameterCountException or
                                                       MethodAccessException or
                                                       TargetInvocationException)
        {
            return false;
        }
    }

    private static void SetContext(DependencyObject? element, Guid context)
    {
        element?.SetValue(ContextProperty, context);
    }

    private static void SetViewOnContentProperty(DependencyObject targetLocation)
    {
        object? viewModel = GetModel(targetLocation);

        var overrideContentProperty = GetContentPropertyName(targetLocation);

        if (viewModel is null)
        {
            SetContentPropertyValue(targetLocation, null, overrideContentProperty);

            return;
        }

        _logger ??= GetServiceProvider(targetLocation).GetService<ILoggerFactory>()?.CreateLogger(typeof(View)) ??
                    NullLogger.Instance;

        var declaredAncestor = targetLocation.FindAncestorDeclaredInUserControlOrWindowOrAdorner() as FrameworkElement;

        if (declaredAncestor?.DataContext == viewModel)
        {
            _logger.LogDebug(
                $"Inherited {nameof(FrameworkElement.DataContext)} detected at {{Location}}: {{ViewModel}}. Setting view to null.",
                GetAncestorTypeName(declaredAncestor),
                viewModel.GetType().Name);

            SetContentPropertyValue(targetLocation, null, overrideContentProperty);

            return;
        }

        var contextLocation = targetLocation.FindFirstNonGeneratedAncestor();

        Guid context = GetContext(contextLocation);

        if (GetCachedViewFor(viewModel, contextLocation) is { } cachedView)
        {
            _logger.LogTrace(
                "Using cached view for {ViewModel} at location {LocationInView} with context ID {ContextID}",
                viewModel.GetType().Name,
                GetAncestorTypeName(declaredAncestor),
                context);

            SetContentPropertyValue(targetLocation, cachedView, overrideContentProperty);

            return;
        }

        _viewLocator ??= GetServiceProvider(targetLocation).GetRequiredService<ViewLocator>();

        void SetDataContext(object dataContext, FrameworkElement view)
        {
            view.DataContext = dataContext;
        }

        var view = _viewLocator.LocateViewForViewModel(viewModel, SetDataContext);

        if (view is null)
        {
            SetContentPropertyValue(targetLocation, null, overrideContentProperty);

            return;
        }

        if (viewModel is IViewAware viewAware && !ReferenceEquals(GetCachedViewFor(viewModel, contextLocation), view))
        {
            if (context == Guid.Empty)
            {
                context = Guid.NewGuid();
            }

            _logger.LogDebug("Attaching {View} to {ViewModel} at location {LocationInView} with context ID {ContextID}",
                             view.GetType().Name,
                             viewAware.GetType().Name,
                             GetAncestorTypeName(declaredAncestor),
                             context);

            SetContext(contextLocation, context);

            viewAware.AttachView(view, context);
        }

        if (!SetContentPropertyValue(targetLocation, view, overrideContentProperty))
        {
            const string Message =
                $"{nameof(SetContentPropertyValue)}() failed for {nameof(ViewLocator)}.{nameof(ViewLocator.LocateViewForViewModel)}(). Used content property override: {{OverrideContentProperty}}";

            _logger.LogWarning(Message, overrideContentProperty);
        }
    }
}