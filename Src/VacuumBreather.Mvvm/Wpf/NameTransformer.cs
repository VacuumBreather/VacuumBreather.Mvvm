// "// Copyright (c) 2022 VacuumBreather. All rights reserved.
// // Licensed under the MIT License. See LICENSE in the project root for license information."

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>
    ///     Class for managing the list of rules for transforming view-model type names into view type
    ///     names.
    /// </summary>
    public class NameTransformer : BindableCollection<NameTransformer.Rule>
    {
        private const RegexOptions Options = RegexOptions.Compiled;

        /// <summary>
        ///     Flag to indicate if transformations from all matched rules are returned. Otherwise,
        ///     transformations from only the first matched rule are returned.
        /// </summary>
        public bool UseEagerRuleSelection { get; set; } = true;

        /// <summary>Adds a transform using a single replacement value and a global filter pattern.</summary>
        /// <param name="replacePattern">Regular expression pattern for replacing text.</param>
        /// <param name="replaceValue">The replacement value.</param>
        /// <param name="globalFilterPattern">Regular expression pattern for global filtering.</param>
        /// <example>
        ///     <code>
        ///         NameTransformer.AddRule("Model$", string.Empty);
        ///     </code>
        ///     This transformation rule looks for the substring “Model” terminating the ViewModel name and
        ///     strips out that substring (i.e. replace with string.Empty or “null string”).<br /> The “$” in
        ///     the first argument indicates that the pattern must match at the end of the source string. If
        ///     “Model” exists anywhere else, the pattern is not matched. Because this call did not include the
        ///     optional “globalFilterPattern” argument, this rule applies to all ViewModel names.<br /> This
        ///     rule yields the following results:
        ///     <list type="bullet">
        ///         <item>MainViewModel => MainView</item>
        ///         <item>ModelAirplaneViewModel => ModelAirplaneView</item>
        ///         <item>CustomerViewModelBase => CustomerViewModelBase</item>
        ///     </list>
        ///     For examples of the use of the global filter pattern check the defaults used in
        ///     <see cref="ViewLocator" />.
        /// </example>
        public void AddRule(string replacePattern, string replaceValue, string? globalFilterPattern = null)
        {
            AddRule(
                replacePattern,
                new[] { replaceValue },
                globalFilterPattern);
        }

        /// <summary>Adds a transform using a list of replacement values and a global filter pattern.</summary>
        /// <param name="replacePattern">Regular expression pattern for replacing text.</param>
        /// <param name="replaceValueList">The list of replacement values.</param>
        /// <param name="globalFilterPattern">Regular expression pattern for global filtering.</param>
        /// <example>
        ///     <code>
        ///         NameTransformer.AddRule("Model$", new string[] { string.Empty });
        ///     </code>
        ///     This transformation rule looks for the substring “Model” terminating the ViewModel name and
        ///     strips out that substring (i.e. replace with string.Empty or “null string”).<br /> The “$” in
        ///     the first argument indicates that the pattern must match at the end of the source string. If
        ///     “Model” exists anywhere else, the pattern is not matched. Because this call did not include the
        ///     optional “globalFilterPattern” argument, this rule applies to all ViewModel names.<br /> This
        ///     rule yields the following results:
        ///     <list type="bullet">
        ///         <item>MainViewModel => MainView</item>
        ///         <item>ModelAirplaneViewModel => ModelAirplaneView</item>
        ///         <item>CustomerViewModelBase => CustomerViewModelBase</item>
        ///     </list>
        ///     For examples of the use of the global filter pattern check the defaults used in
        ///     <see cref="ViewLocator" />.
        /// </example>
        public void AddRule(string replacePattern,
            IEnumerable<string> replaceValueList,
            string? globalFilterPattern = null)
        {
            Add(
                new Rule
                {
                    ReplacePattern = replacePattern,

                    // ReSharper disable once PossibleMultipleEnumeration
                    ReplacementValues = replaceValueList,
                    GlobalFilterPattern = globalFilterPattern
                });
        }

        /// <summary>Gets the list of transformations for a given name based on the currently rule set.</summary>
        /// <param name="source">The name to transform into the resolved name list.</param>
        /// <returns>The transformed names.</returns>
        public IEnumerable<string> Transform(string source)
        {
            List<string> nameList = new();
            IEnumerable<Rule> rules = this.Reverse();

            foreach (Rule rule in rules)
            {
                if (!string.IsNullOrEmpty(rule.GlobalFilterPattern) && !rule.GlobalFilterPatternRegex.IsMatch(source))
                {
                    continue;
                }

                if (!rule.ReplacePatternRegex.IsMatch(source))
                {
                    continue;
                }

                nameList.AddRange(
                    rule.ReplacementValues.Select(repString => rule.ReplacePatternRegex.Replace(source, repString)));

                if (!UseEagerRuleSelection)
                {
                    break;
                }
            }

            return nameList;
        }

        /// <summary>A rule that describes a name transform.</summary>
        public class Rule
        {
            /// <summary>Regular expression pattern for global filtering.</summary>
            public string? GlobalFilterPattern;

            private Regex? globalFilterPatternRegex;

            /// <summary>The list of replacement values</summary>
            public IEnumerable<string> ReplacementValues = new string[] { };

            /// <summary>Regular expression pattern for replacing text.</summary>
            public string? ReplacePattern;

            private Regex? replacePatternRegex;

            /// <summary>Regular expression for global filtering.</summary>
            public Regex GlobalFilterPatternRegex => this.globalFilterPatternRegex ??= new Regex(
                this.GlobalFilterPattern ?? string.Empty,
                Options);

            /// <summary>Regular expression for replacing text.</summary>
            public Regex ReplacePatternRegex =>
                this.replacePatternRegex ??= new Regex(this.ReplacePattern ?? string.Empty, Options);
        }
    }
}