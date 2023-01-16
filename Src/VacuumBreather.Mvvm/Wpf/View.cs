﻿// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.Extensions.Logging;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Hosts attached properties related to view-models.</summary>
    public static class View
    {
        /// <summary>A dependency property for marking an element as framework generated.</summary>
        public static readonly DependencyProperty IsGeneratedProperty = DependencyProperty.RegisterAttached(
            PropertyNameHelper.GetName(nameof(IsGeneratedProperty)),
            typeof(bool),
            typeof(View),
            new PropertyMetadata(default(bool)));

        private static readonly DependencyProperty ContextProperty = DependencyProperty.RegisterAttached(
            PropertyNameHelper.GetName(nameof(ContextProperty)),
            typeof(Guid),
            typeof(View),
            new PropertyMetadata(default(Guid)));

        private static readonly ContentPropertyAttribute DefaultContentProperty = new(nameof(ContentControl.Content));

        /// <summary>A dependency property for attaching a model to the UI.</summary>
        public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached(
            PropertyNameHelper.GetName(nameof(ModelProperty)),
            typeof(object),
            typeof(View),
            new PropertyMetadata(default, OnModelChanged));

        private static ILogger Logger => LogManager.FrameworkLogger;

        /// <summary>Gets a value indicating whether the element was generated by the framework.</summary>
        /// <param name="element">The element to check.</param>
        /// <returns><c>true</c> if the element was generated by the framework; otherwise, <c>false</c>.</returns>
        public static bool GetIsGenerated(DependencyObject element)
        {
            return (bool)element.GetValue(IsGeneratedProperty);
        }

        /// <summary>Gets the model.</summary>
        /// <param name="d">The element the model is attached to.</param>
        /// <returns>The model.</returns>
        public static object GetModel(DependencyObject d)
        {
            return d.GetValue(ModelProperty);
        }

        /// <summary>Sets a value indicating whether the element was generated by the framework.</summary>
        /// <param name="element">The element to mark as generated.</param>
        /// <param name="value">If set to <c>true</c> the element is marked as generated by the framework.</param>
        public static void SetIsGenerated(DependencyObject element, bool value)
        {
            element.SetValue(IsGeneratedProperty, value);
        }

        /// <summary>Sets the model.</summary>
        /// <param name="d">The element to attach the model to.</param>
        /// <param name="value">The model.</param>
        public static void SetModel(DependencyObject d, object value)
        {
            d.SetValue(ModelProperty, value);
        }

        private static UIElement? GetCachedViewFor(object viewModel, DependencyObject? contextLocation)
        {
            Guid context = GetContext(contextLocation);

            if (viewModel is not IViewAware viewAware || context == Guid.Empty)
            {
                return null;
            }

            return viewAware.GetView(context);
        }

        private static Guid GetContext(DependencyObject? element)
        {
            return (Guid)(element?.GetValue(ContextProperty) ?? Guid.Empty);
        }

        private static ViewLocator? GetViewLocator(DependencyObject dependencyObject)
        {
            FrameworkElement? frameworkElement = dependencyObject.FindVisualAncestor<FrameworkElement>();
            IServiceProvider? serviceProvider =
                frameworkElement?.TryFindResource(nameof(IServiceProvider)) as IServiceProvider;

            return serviceProvider?.GetService(typeof(ViewLocator)) as ViewLocator;
        }

        private static void OnModelChanged(DependencyObject targetLocation, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue == args.NewValue)
            {
                return;
            }

            object? viewModel = args.NewValue;

            if (viewModel is null)
            {
                SetContentProperty(targetLocation, null);

                return;
            }

            DependencyObject? contextLocation = targetLocation.FindFirstNonGeneratedAncestor();

            Guid context = GetContext(contextLocation);

            if (GetCachedViewFor(viewModel, contextLocation) is { } cachedView)
            {
                if (Logger.IsEnabled(LogLevel.Debug))
                {
                    // Only do this if the log level is active because FindAncestorDeclaredInUserControlOrWindow()
                    // should not be called unnecessarily.
                    Logger.LogDebug(
                        "Using cached view for {ViewModel} at location {@LocationInView} with context ID {ContextID}",
                        viewModel,
                        contextLocation.FindAncestorDeclaredInUserControlOrWindow(),
                        context);
                }

                SetContentProperty(targetLocation, cachedView);

                return;
            }

            ViewLocator? viewLocator = GetViewLocator(targetLocation);

            if (viewLocator != null)
            {
                UIElement view = viewLocator.LocateForModel(viewModel);

                if (viewModel is IViewAware viewAware &&
                    !ReferenceEquals(GetCachedViewFor(viewModel, contextLocation), view))
                {
                    if (context == Guid.Empty)
                    {
                        context = Guid.NewGuid();
                    }

                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        // Only do this if the log level is active because GetAncestorDeclaredInUserControl()
                        // should not be called unnecessarily.
                        Logger.LogDebug(
                            "Attaching {@View} to {ViewModel} at location {@LocationInView} with context ID {ContextID}",
                            view,
                            viewAware,
                            contextLocation.FindAncestorDeclaredInUserControlOrWindow(),
                            context);
                    }

                    SetContext(contextLocation, context);

                    viewAware.AttachView(view, context);
                }

                if (!SetContentProperty(targetLocation, view))
                {
                    Logger.LogWarning(
                        "{SetContentProperty}() failed for {ViewLocator}.{LocateForModel}(), falling back to {LocateForModelType}()",
                        nameof(SetContentProperty),
                        nameof(ViewLocator),
                        nameof(ViewLocator.LocateForModel),
                        nameof(ViewLocator.LocateForModelType));

                    view = viewLocator.LocateForModelType(viewModel.GetType());

                    SetContentProperty(targetLocation, view);
                }
            }
            else
            {
                SetContentProperty(targetLocation, null);
            }
        }

        private static bool SetContentProperty(object targetLocation, object? view)
        {
            if (view is FrameworkElement { Parent: { } } frameworkElement)
            {
                SetContentPropertyCore(frameworkElement.Parent, null);
            }

            return SetContentPropertyCore(targetLocation, view);
        }

        private static bool SetContentPropertyCore(object targetLocation, object? view)
        {
            try
            {
                Type type = targetLocation.GetType();
                ContentPropertyAttribute contentProperty =
                    type.GetCustomAttributes(typeof(ContentPropertyAttribute), true)
                        .OfType<ContentPropertyAttribute>()
                        .FirstOrDefault() ??
                    DefaultContentProperty;

                PropertyInfo? propertyInfo = type.GetProperty(contentProperty.Name ?? DefaultContentProperty.Name);

                if (propertyInfo == null)
                {
                    return false;
                }

                propertyInfo.SetValue(targetLocation, view, null);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void SetContext(DependencyObject? element, Guid context)
        {
            element?.SetValue(ContextProperty, context);
        }
    }
}