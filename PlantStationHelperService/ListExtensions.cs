using System;
using System.Collections.Generic;
using System.Linq; // Notwendig für die LINQ-Methode Where und String.Join

namespace PlantStationHelperService
{
    public static class ListExtensions
    {
        /// <summary>
        /// Converts a list of elements into a semicolon-and-space ("; ") delimited string.
        /// It safely handles null lists and null elements within the list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list of elements to convert. Can be <see langword="null"/>.</param>
        /// <returns>A string containing the concatenated string representations of the non-null list elements, 
        /// delimited by "; ". Returns <see cref="string.Empty"/> if the list is null, empty, or contains only null elements.</returns>
        public static string ToDelimitedString<T>(this List<T>? list)
        {
            if (list == null || !list.Any())
            {
                return string.Empty;
            }

            return String.Join("; ", list.Where(item => item != null).Select(item => item!.ToString()));
        }
    }
}