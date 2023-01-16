﻿// Copyright (c) 2022 VacuumBreather. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VacuumBreather.Mvvm.Wpf
{
    /// <summary>Provides extension methods for the <see cref="DependencyObject" /> type.</summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        ///     Goes up the visual tree, looking for an ancestor declared inside a
        ///     <see cref="UserControl" /> or <see cref="Window" /> (i.e. with a <see cref="UserControl" /> or
        ///     <see cref="Window" />
        ///     ancestor in its logical tree).
        /// </summary>
        /// <param name="dependencyObject">The starting point.</param>
        /// <returns>The visual ancestor, if any, which was declared inside a <see cref="UserControl" />.</returns>
        public static DependencyObject? FindAncestorDeclaredInUserControlOrWindow(
            this DependencyObject? dependencyObject)
        {
            while (dependencyObject != null && FindLogicalAncestor<UserControl>(dependencyObject) is null &&
                   FindLogicalAncestor<Window>(dependencyObject) is null)
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            return dependencyObject;
        }

        /// <summary>
        ///     Goes up the visual tree, looking for the first element which is not marked as framework
        ///     generated.
        /// </summary>
        /// <param name="dependencyObject">The starting point.</param>
        /// <returns>The first element not generated by the framework (this can be the element itself).</returns>
        public static DependencyObject? FindFirstNonGeneratedAncestor(this DependencyObject? dependencyObject)
        {
            while (dependencyObject != null && View.GetIsGenerated(dependencyObject))
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            return dependencyObject;
        }

        /// <summary>Goes up the logical tree, looking for an ancestor of the specified <see cref="Type" />.</summary>
        /// <typeparam name="T">The <see cref="Type" /> to look for.</typeparam>
        /// <param name="dependencyObject">The starting point.</param>
        /// <returns>The logical ancestor, if any, of the specified starting point and <see cref="Type" />.</returns>
        public static T? FindLogicalAncestor<T>(this DependencyObject? dependencyObject)
            where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

            do
            {
                dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
            } while (dependencyObject != null && !(dependencyObject is T));

            return dependencyObject as T;
        }

        /// <summary>Goes up the visual tree, looking for an ancestor of the specified <see cref="Type" />.</summary>
        /// <typeparam name="T">The <see cref="Type" /> to look for.</typeparam>
        /// <param name="dependencyObject">The starting point.</param>
        /// <returns>The visual ancestor, if any, of the specified starting point and <see cref="Type" />.</returns>
        public static T? FindVisualAncestor<T>(this DependencyObject? dependencyObject)
            where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                return null;
            }

            do
            {
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            } while (dependencyObject != null && !(dependencyObject is T));

            return dependencyObject as T;
        }
    }
}