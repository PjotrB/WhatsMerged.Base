using System.Collections.Generic;
using System.ComponentModel;

namespace WhatsMerged.Base.Helpers
{
    /// <summary>
    /// Some simple but useful extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Wrapper for "string.IsNullOrWhiteSpace(s)".
        /// </summary>
        /// <param name="s">The string to check</param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// Wrapper for "!string.IsNullOrWhiteSpace(s)".
        /// </summary>
        /// <param name="s">The string to check</param>
        /// <returns></returns>
        public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// True if items is either null or empty; false otherwise.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="items">The source to check for content</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IList<T> items)
        {
            return items == null || items.Count == 0;
        }

        /// <summary>
        /// True if items is not null and also not empty; false otherwise.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="items">The source to check for content</param>
        /// <returns></returns>
        public static bool HasItems<T>(this IList<T> items)
        {
            return !items.IsNullOrEmpty();
        }

        /// <summary>
        /// Creates a BindingList of T and adds everything from items to it.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="items">The source from which to get items of type T</param>
        /// <returns></returns>
        public static BindingList<T> ToBindingList<T>(this IEnumerable<T> items)
        {
            var list = new BindingList<T>();
            list.AddRange(items);
            return list;
        }

        /// <summary>
        /// Adds everything from itemsToAdd to the BindingList of T.
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="list">The BindingList of T object</param>
        /// <param name="itemsToAdd">The source from which to get items of type T</param>
        public static void AddRange<T>(this BindingList<T> list, IEnumerable<T> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
                list.Add(item);
        }

        /// <summary>
        /// Remove everything in itemsToRemove from the BindingList of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="itemsToRemove"></param>
        public static void RemoveRange<T>(this BindingList<T> list, IEnumerable<T> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
                list.Remove(item);
        }
    }
}