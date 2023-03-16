using System;
using System.Globalization;

namespace VacuumBreather.Mvvm.Core;

/// <summary>Provides extension methods for the <see cref="string"/> type.</summary>
internal static class StringExtensions
{
    /// <summary>Returns a new string in which all occurrences of a specified string in the current instance are removed.</summary>
    /// <param name="str">The string to be manipulated.</param>
    /// <param name="pattern">The string to be removed.</param>
    /// <returns>
    ///     A string that is equivalent to the current string except that all instances of <paramref name="pattern"/> are
    ///     removed. If <paramref name="pattern"/> is not found in the current instance, the method returns the current
    ///     instance unchanged.
    /// </returns>
    internal static string Remove(this string str, string pattern)
    {
        return str.Replace(pattern, string.Empty, StringComparison.Ordinal);
    }

    /// <summary>Splits a string into substrings based on specified delimiting character.</summary>
    /// <param name="str">The string to be manipulated.</param>
    /// <param name="separator">A character that delimit the substrings in this string, or <see langword="null"/>.</param>
    /// <returns>An array whose elements contain the substrings in this string that are delimited by the separator character.</returns>
    internal static string[] Split(this string str, string separator)
    {
        return str.Split(new[] { separator }, StringSplitOptions.None);
    }

    /// <summary>Returns a copy of this string with the first character converted to uppercase.</summary>
    /// <param name="str">The string to be manipulated.</param>
    /// <returns>The equivalent of the current string with the first character converted to uppercase.</returns>
    internal static string UppercaseFirst(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        return char.ToUpper(str[index: 0], CultureInfo.InvariantCulture) + str.Substring(startIndex: 1);
    }
}