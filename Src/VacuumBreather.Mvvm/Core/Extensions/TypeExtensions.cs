using System;
using JetBrains.Annotations;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="Type"/> type.</summary>
[PublicAPI]
public static class TypeExtensions
{
    /// <summary>Determines whether an instance of this type can be assigned to a variable of the specified type.</summary>
    /// <param name="type">The current type.</param>
    /// <param name="baseType">The base type.</param>
    /// <returns>
    ///     <para><see langword="true"/> if any of the following conditions is true:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>The current type and <paramref name="baseType"/> represent the same type.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The current type is derived either directly or indirectly from the <paramref name="baseType"/>
    ///                 . The current type is derived directly from the <paramref name="baseType"/> if it inherits from the
    ///                 <paramref name="baseType"/> The current type  is derived indirectly from the
    ///                 <paramref name="baseType"/> if it inherits from a succession of one or more classes that inherit from
    ///                 the <paramref name="baseType"/>.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>The <paramref name="baseType"/> is an interface that the current type implements.</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The current type is a generic type parameter, and the <paramref name="baseType"/> represents
    ///                 one of the constraints of the current type.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 The current type represents a value type, and the <paramref name="baseType"/> represents
    ///                 Nullable&lt; currentType&gt;.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>otherwise, <see langword="false"/>.</para>
    /// </returns>
    public static bool IsDerivedFromOrImplements(this Type type, Type baseType)
    {
        return baseType.IsAssignableFrom(type);
    }
}