// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Responsible for mapping view-model types to their corresponding view types.</summary>
    public class ViewLocator
    {
        private const string DefaultViewSuffix = "View";

        private readonly List<string> viewSuffixList = new();

        private string defaultSubNsViewModels = "ViewModels";
        private string defaultSubNsViews = "Views";

        private bool includeViewSuffixInVmNames;
        private string nameFormat = string.Empty;
        private bool useNameSuffixesInMappings;
        private string viewModelSuffix = "ViewModel";

        /// <summary>Initializes a new instance of the <see cref="ViewLocator" /> class.</summary>
        /// <param name="assemblySource">
        ///     The source of assemblies that contain view and view-model types
        ///     relevant to this instance.
        /// </param>
        /// <param name="serviceProvider">The service provider used to resolve views.</param>
        public ViewLocator(AssemblySource assemblySource, IServiceProvider serviceProvider)
        {
            AssemblySource = assemblySource;
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        ///     Gets the source of assemblies that contain view and view-model types relevant to this
        ///     instance.
        /// </summary>
        /// <value>The source of assemblies that contain view and view-model types relevant to this instance.</value>
        public AssemblySource AssemblySource { get; }

        private static ILogger Logger => LogManager.FrameworkLogger;

        private NameTransformer NameTransformer { get; } = new();

        private IServiceProvider ServiceProvider { get; }

        /// <summary>Adds a default type mapping using the standard namespace mapping convention.</summary>
        /// <param name="viewSuffix">(Optional) Suffix for type name. Should be "View" or synonym of "View".</param>
        public void AddDefaultTypeMapping(string viewSuffix = DefaultViewSuffix)
        {
            if (!this.useNameSuffixesInMappings)
            {
                return;
            }

            // Check for <Namespace>.<BaseName><ViewSuffix> construct.
            AddNamespaceMapping(string.Empty, string.Empty, viewSuffix);

            // Check for <Namespace>.ViewModels.<NameSpace>.<BaseName><ViewSuffix> construct.
            AddSubNamespaceMapping(this.defaultSubNsViewModels, this.defaultSubNsViews, viewSuffix);
        }

        /// <summary>Adds a standard type mapping based on simple namespace mapping.</summary>
        /// <param name="nsSource">Namespace of source type.</param>
        /// <param name="nsTargets">Namespaces of target type.</param>
        /// <param name="viewSuffix">(Optional) Suffix for type name. Should be "View" or synonym of "View".</param>
        public void AddNamespaceMapping(string nsSource,
            IEnumerable<string> nsTargets,
            string viewSuffix = DefaultViewSuffix)
        {
            // We need to terminate with "." in order to concatenate with type name later.
            string nsEncoded = RegExHelper.NamespaceToRegEx(nsSource + ".");

            // Start the pattern search from beginning of string ("^")
            // unless the original string was blank (i.e. special case to indicate "append target to source").
            if (!string.IsNullOrEmpty(nsSource))
            {
                nsEncoded = "^" + nsEncoded;
            }

            // Capture the namespace as "nsOrig" in case we need to use it in the output in the future.
            string nsReplace = RegExHelper.GetCaptureGroup("nsOrig", nsEncoded);

            // ReSharper disable once PossibleMultipleEnumeration
            string[] nsTargetsRegEx = nsTargets.Select(t => t + ".").ToArray();

            AddTypeMapping(nsReplace, null, nsTargetsRegEx, viewSuffix);
        }

        /// <summary>Adds a standard type mapping based on simple namespace mapping.</summary>
        /// <param name="nsSource">Namespace of source type.</param>
        /// <param name="nsTarget">Namespace of target type.</param>
        /// <param name="viewSuffix">(Optional) Suffix for type name. Should  be "View" or synonym of "View".</param>
        public void AddNamespaceMapping(string nsSource, string nsTarget, string viewSuffix = DefaultViewSuffix)
        {
            AddNamespaceMapping(
                nsSource,
                new[] { nsTarget },
                viewSuffix);
        }

        /// <summary>Adds a standard type mapping by substituting one sub-namespace for another.</summary>
        /// <param name="nsSource">Sub-namespace of source type.</param>
        /// <param name="nsTargets">Sub-namespaces of target type.</param>
        /// <param name="viewSuffix">(Optional) Suffix for type name. Should  be "View" or synonym of "View".</param>
        public void AddSubNamespaceMapping(string? nsSource,
            IEnumerable<string?> nsTargets,
            string viewSuffix = DefaultViewSuffix)
        {
            // We need to terminate with "." in order to concatenate with type name later.
            string nsEncoded = RegExHelper.NamespaceToRegEx(nsSource + ".");

            string rxBeforeTgt = string.Empty;
            string rxAfterSrc = string.Empty;
            string rxAfterTgt = string.Empty;
            string rxBeforeSrc = string.Empty;

            if (!string.IsNullOrEmpty(nsSource))
            {
                if (!nsSource.StartsWith("*"))
                {
                    rxBeforeSrc = RegExHelper.GetNamespaceCaptureGroup("nsBefore");
                    rxBeforeTgt = @"${nsBefore}";
                }

                if (!nsSource.EndsWith("*"))
                {
                    rxAfterSrc = RegExHelper.GetNamespaceCaptureGroup("nsAfter");
                    rxAfterTgt = "${nsAfter}";
                }
            }

            string rxMid = RegExHelper.GetCaptureGroup("subNs", nsEncoded);
            string rxReplace = string.Concat(rxBeforeSrc, rxMid, rxAfterSrc);

            // ReSharper disable once PossibleMultipleEnumeration
            string[] nsTargetsRegEx = nsTargets.Select(t => string.Concat(rxBeforeTgt, t, ".", rxAfterTgt)).ToArray();

            AddTypeMapping(rxReplace, null, nsTargetsRegEx, viewSuffix);
        }

        /// <summary>Adds a standard type mapping by substituting one sub-namespace for another.</summary>
        /// <param name="nsSource">Sub-namespace of source type.</param>
        /// <param name="nsTarget">Sub-namespace of target type.</param>
        /// <param name="viewSuffix">(Optional) Suffix for type name. Should  be "View" or synonym of "View".</param>
        public void AddSubNamespaceMapping(string? nsSource, string? nsTarget, string viewSuffix = DefaultViewSuffix)
        {
            AddSubNamespaceMapping(
                nsSource,
                new[] { nsTarget },
                viewSuffix);
        }

        /// <summary>Adds a standard type mapping based on namespace RegEx replace and filter patterns.</summary>
        /// <param name="nsSourceReplaceRegEx">RegEx replace pattern for source namespace.</param>
        /// <param name="nsSourceFilterRegEx">RegEx filter pattern for source namespace.</param>
        /// <param name="nsTargetsRegEx">Array of RegEx replace values for target namespaces.</param>
        /// <param name="viewSuffix">(Optional) Suffix for type name. Should  be "View" or synonym of "View".</param>
        public void AddTypeMapping(string nsSourceReplaceRegEx,
            string? nsSourceFilterRegEx,
            IEnumerable<string> nsTargetsRegEx,
            string viewSuffix = DefaultViewSuffix)
        {
            RegisterViewSuffix(viewSuffix);

            string repSuffix = this.useNameSuffixesInMappings ? viewSuffix : string.Empty;

            const string baseGroup = "${basename}";

            string rxBase = RegExHelper.GetNameCaptureGroup("basename");
            string suffix = string.Empty;

            if (this.useNameSuffixesInMappings)
            {
                suffix = this.viewModelSuffix;

                if (!this.viewModelSuffix.Contains(viewSuffix) && this.includeViewSuffixInVmNames)
                {
                    suffix = viewSuffix + suffix;
                }
            }

            string? rxSourceFilter = string.IsNullOrEmpty(nsSourceFilterRegEx)
                ? null
                : string.Concat(
                    nsSourceFilterRegEx,
                    string.Format(this.nameFormat, RegExHelper.NameRegEx, suffix),
                    "$");

            string rxSuffix = RegExHelper.GetCaptureGroup("suffix", suffix);

            NameTransformer.AddRule(
                string.Concat(nsSourceReplaceRegEx, string.Format(this.nameFormat, rxBase, rxSuffix), "$"),

                // ReSharper disable once PossibleMultipleEnumeration
                nsTargetsRegEx.Select(t => t + string.Format(this.nameFormat, baseGroup, repSuffix)).ToArray(),
                rxSourceFilter);
        }

        /// <summary>Adds a standard type mapping based on namespace RegEx replace and filter patterns.</summary>
        /// <param name="nsSourceReplaceRegEx">RegEx replace pattern for source namespace.</param>
        /// <param name="nsSourceFilterRegEx">RegEx filter pattern for source namespace.</param>
        /// <param name="nsTargetRegEx">RegEx replace value for target namespace.</param>
        /// <param name="viewSuffix">(Optional) Suffix for type name. Should  be "View" or synonym of "View".</param>
        public void AddTypeMapping(string nsSourceReplaceRegEx,
            string? nsSourceFilterRegEx,
            string nsTargetRegEx,
            string viewSuffix = DefaultViewSuffix)
        {
            AddTypeMapping(
                nsSourceReplaceRegEx,
                nsSourceFilterRegEx,
                new[] { nsTargetRegEx },
                viewSuffix);
        }

        /// <summary>
        ///     Specifies how type mappings are created, including default type mappings. Calling this
        ///     method will clear all existing name transformation rules and create new default type mappings
        ///     according to the configuration.
        /// </summary>
        /// <param name="config">
        ///     An instance of TypeMappingConfiguration that provides the settings for
        ///     configuration
        /// </param>
        public void ConfigureTypeMappings(TypeMappingConfiguration config)
        {
            if (string.IsNullOrEmpty(config.DefaultSubNamespaceForViews))
            {
                throw new ArgumentException($"{config.DefaultSubNamespaceForViews} cannot be blank.");
            }

            if (string.IsNullOrEmpty(config.DefaultSubNamespaceForViewModels))
            {
                throw new ArgumentException($"{config.DefaultSubNamespaceForViewModels} cannot be blank.");
            }

            if (string.IsNullOrEmpty(config.NameFormat))
            {
                throw new ArgumentException($"{config.NameFormat} cannot be blank.");
            }

            NameTransformer.Clear();
            this.viewSuffixList.Clear();

            this.defaultSubNsViews = config.DefaultSubNamespaceForViews;
            this.defaultSubNsViewModels = config.DefaultSubNamespaceForViewModels;
            this.nameFormat = config.NameFormat;
            this.useNameSuffixesInMappings = config.UseNameSuffixesInMappings;
            this.viewModelSuffix = config.ViewModelSuffix;
            this.viewSuffixList.AddRange(config.ViewSuffixList);
            this.includeViewSuffixInVmNames = config.IncludeViewSuffixInViewModelNames;

            SetAllDefaults();
        }

        /// <summary>Retrieves the view from the IoC container or tries to create it if not found.</summary>
        /// <param name="viewType">The type of view to create.</param>
        /// <remarks>Pass the type of view as a parameter and receive an instance of the view.</remarks>
        public UIElement GetOrCreateViewType(Type viewType)
        {
            TextBlock CreatePlaceholderView()
            {
                return new TextBlock { Text = $"Cannot create {viewType.FullName}." };
            }

            if (viewType.IsInterface || viewType.IsAbstract || !viewType.IsDerivedFromOrImplements(typeof(UIElement)))
            {
                return CreatePlaceholderView();
            }

            if (ServiceProvider.GetService(viewType) is UIElement view)
            {
                return view;
            }

            try
            {
                view = (UIElement)Activator.CreateInstance(viewType)!;
            }
            catch (Exception)
            {
                return CreatePlaceholderView();
            }

            return view;
        }

        /// <summary>Locates the view for the specified model instance.</summary>
        /// <param name="model">The model.</param>
        /// <returns>The view.</returns>
        /// <remarks>
        ///     Pass the model instance, display location (or null) and the context (or null) as
        ///     parameters and receive a view instance.
        /// </remarks>
        public UIElement LocateForModel(object model)
        {
            return LocateForModelType(model.GetType());
        }

        /// <summary>Locates the view for the specified model type.</summary>
        /// <param name="modelType">The type of the model.</param>
        /// <returns>The view.</returns>
        /// <remarks>
        ///     Pass the model type, display location (or null) and the context instance (or null) as
        ///     parameters and receive a view instance.
        /// </remarks>
        public UIElement LocateForModelType(Type modelType)
        {
            Type? viewType = LocateTypeForModelType(modelType);

            return viewType == null
                ? new TextBlock { Text = $"Cannot find view for {modelType}." }
                : GetOrCreateViewType(viewType);
        }

        /// <summary>Locates the view type based on the specified model type.</summary>
        /// <param name="modelType">The model type.</param>
        /// <returns>The located view type or <c>null</c> if no such type could be found.</returns>
        public Type? LocateTypeForModelType(Type modelType)
        {
            string modelTypeName = modelType.FullName ?? string.Empty;

            int index = modelTypeName.IndexOf('`') < 0 ? modelTypeName.Length : modelTypeName.IndexOf('`');

            modelTypeName = modelTypeName[..index];

            List<string> viewTypes = TransformName(modelTypeName).ToList();
            Type? viewType = AssemblySource.FindTypeByNames(viewTypes);

            if (viewType is { })
            {
                return viewType;
            }

            if (viewTypes.Any())
            {
                Logger.LogDebug(
                    "No view found for {ModelType} - no match among {ViewTypes}",
                    modelType.Name,
                    string.Join(", ", viewTypes.ToArray()));
            }
            else
            {
                Logger.LogDebug("No view found for {ModelType}", modelType.Name);
            }

            return viewType;
        }

        /// <summary>
        ///     This method registers a View suffix or synonym so that View Context resolution works
        ///     properly. It is automatically called internally when calling AddNamespaceMapping(),
        ///     AddDefaultTypeMapping(), or AddTypeMapping(). It should not need to be called explicitly unless
        ///     a rule that handles synonyms is added directly through the NameTransformer.
        /// </summary>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View".</param>
        public void RegisterViewSuffix(string viewSuffix)
        {
            if (this.viewSuffixList.All(s => s != viewSuffix))
            {
                this.viewSuffixList.Add(viewSuffix);
            }
        }

        /// <summary>Transforms a ViewModel type name into all of its possible View type names.</summary>
        /// <param name="typeName">The name of the ViewModel type being resolved to its companion View.</param>
        /// <returns>Enumeration of transformed names.</returns>
        public IEnumerable<string> TransformName(string typeName)
        {
            return NameTransformer.Transform(typeName);
        }

        private void SetAllDefaults()
        {
            if (this.useNameSuffixesInMappings)
            {
                // Add support for all view suffixes.
                this.viewSuffixList.ForEach(AddDefaultTypeMapping);
            }
            else
            {
                AddSubNamespaceMapping(this.defaultSubNsViewModels, this.defaultSubNsViews);
            }
        }
    }
}