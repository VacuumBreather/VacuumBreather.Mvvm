// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>
    ///     Specifies settings for configuring type mappings performed by the
    ///     <see cref="ViewLocator" />.
    /// </summary>
    public class TypeMappingConfiguration
    {
        /// <summary>
        ///     The default sub-namespace for ViewModels. Used for creating default sub-namespace
        ///     mappings. Defaults to "ViewModels".
        /// </summary>
        public string DefaultSubNamespaceForViewModels { get; set; } = "ViewModels";

        /// <summary>
        ///     The default sub-namespace for Views. Used for creating default sub-namespace mappings.
        ///     Defaults to "Views".
        /// </summary>
        public string DefaultSubNamespaceForViews { get; set; } = "Views";

        /// <summary>
        ///     Flag to indicate if ViewModel names should include View suffixes (i.e.
        ///     CustomerPageViewModel vs. CustomerViewModel)
        /// </summary>
        public bool IncludeViewSuffixInViewModelNames { get; set; } = true;

        /// <summary>The format string used to compose the name of a type from base name and name suffix</summary>
        public string NameFormat { get; set; } = @"{0}{1}";

        /// <summary>
        ///     Gets or sets a value indicating whether or not the name of the Type should be transformed when adding a
        ///     type mapping. Defaults to true.
        /// </summary>
        public bool UseNameSuffixesInMappings { get; set; } = true;

        /// <summary>
        ///     The name suffix for ViewModels. Applies only when UseNameSuffixesInMappings = true. The
        ///     default is "ViewModel".
        /// </summary>
        public string ViewModelSuffix { get; set; } = "ViewModel";

        /// <summary>
        ///     List of View suffixes for which default type mappings should be created. Applies only when
        ///     UseNameSuffixesInMappings = true. Default values are "View", "Page"
        /// </summary>
        public List<string> ViewSuffixList { get; } = new(
            new[] { "View", "Page" });
    }
}