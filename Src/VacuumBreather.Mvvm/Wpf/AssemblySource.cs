// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using VacuumBreather.Mvvm.Lifecycle;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>A source of assemblies that contain view and view-model types relevant to the framework.</summary>
    public class AssemblySource
    {
        private readonly IBindableCollection<Assembly> typeAssemblies = new BindableCollection<Assembly>();

        private readonly IDictionary<string, Type> typeNameCache = new Dictionary<string, Type>();

        /// <summary>Initializes a new instance of the <see cref="AssemblySource" /> class.</summary>
        public AssemblySource()
        {
            this.typeAssemblies.CollectionChanged += OnTypeAssembliesCollectionChanged;
        }

        /// <summary>
        ///     Gets all relevant view-model types exported by the configured assemblies, excluding any
        ///     view-model implementing <see cref="IWindowManager" />.
        /// </summary>
        public IEnumerable<Type> ViewModelTypes => this.typeNameCache.Values.Where(
            type => type.IsDerivedFromOrImplements(typeof(INotifyPropertyChanged)) &&
                    !type.IsDerivedFromOrImplements(typeof(IDialogConductor)) &&
                    !type.IsDerivedFromOrImplements(typeof(IWindowManager)));

        /// <summary>
        ///     Gets all relevant view types exported by the configured assemblies, excluding
        ///     <see cref="ShellView" />.
        /// </summary>
        public IEnumerable<Type> ViewTypes => this.typeNameCache.Values.Where(
            type => type.IsDerivedFromOrImplements(typeof(UserControl)) &&
                    !type.IsDerivedFromOrImplements(typeof(ShellView)));

        /// <summary>Adds an assembly to the <see cref="AssemblySource" />.</summary>
        /// <param name="assembly">The assembly to add.</param>
        public void Add(Assembly assembly)
        {
            if (!this.typeAssemblies.Contains(assembly))
            {
                this.typeAssemblies.Add(assembly);
            }
        }

        /// <summary>Adds a range of assemblies to the <see cref="AssemblySource" />.</summary>
        /// <param name="assemblies">The range of assemblies to add.</param>
        public void AddRange(IEnumerable<Assembly> assemblies)
        {
            this.typeAssemblies.AddRange(assemblies.Where(assembly => !this.typeAssemblies.Contains(assembly)));
        }

        /// <summary>Removes all assemblies from the <see cref="AssemblySource" />.</summary>
        public void Clear()
        {
            this.typeAssemblies.Clear();
        }

        /// <summary>Finds a type which matches the specified name.</summary>
        /// <param name="name">The name to search for.</param>
        public Type? FindTypeByName(string name)
        {
            return FindTypeByNames(
                new[] { name });
        }

        /// <summary>Finds a type which matches one of the elements in the sequence of names.</summary>
        /// <param name="names">A sequence of names to search for.</param>
        public Type? FindTypeByNames(IEnumerable<string> names)
        {
            return names.Select(n => this.typeNameCache.GetValueOrDefault(n)).NotNull().FirstOrDefault();
        }

        private void OnTypeAssembliesCollectionChanged(object? s, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        eventArgs.NewItems?.OfType<Assembly>()
                            .SelectMany(ExtractTypes)
                            .Where(type => type.FullName != null)
                            .ForEach(type => this.typeNameCache.Add(type.FullName!, type));

                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Reset:
                    {
                        this.typeNameCache.Clear();
                        this.typeAssemblies.SelectMany(ExtractTypes)
                            .Where(type => type.FullName != null)
                            .ForEach(type => this.typeNameCache.Add(type.FullName!, type));

                        break;
                    }
            }
        }

        private static IEnumerable<Type> ExtractTypes(Assembly assembly)
        {
            // Extract all potential view and view-model types.
            return assembly.GetExportedTypes()
                .Where(type =>
                    type is { IsGenericType: false, IsInterface: false, IsAbstract: false, IsNested: false } &&
                    (type.IsDerivedFromOrImplements(typeof(UIElement)) ||
                     type.IsDerivedFromOrImplements(typeof(Screen))));
        }
    }
}