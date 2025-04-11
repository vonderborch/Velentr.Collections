using System.Collections.Generic;

namespace Velentr.Collections
{
    /// <summary>
    /// Extension methods for <see cref="BiDirectionalDictionary{TKey, TValue}"/>.
    /// </summary>
    public static class BiDirectionalDictionaryExtensions
    {
        /// <summary>
        /// Creates a BiDirectionalDictionary from an IEnumerable of KeyValuePairs.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <returns>A new BiDirectionalDictionary containing the elements from the source collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if source is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the source contains duplicate keys or values.</exception>
        public static BiDirectionalDictionary<TKey, TValue> ToBiDirectionalDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source)
            where TKey : notnull
            where TValue : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // Convert to array once to get the count for capacity initialization
            var sourceArray = source.ToArray();
            var result = new BiDirectionalDictionary<TKey, TValue>(sourceArray.Length);
            result.AddRange(sourceArray);
            return result;
        }

        /// <summary>
        /// Creates a BiDirectionalDictionary from an IEnumerable.
        /// </summary>
        /// <typeparam name="TSource">The type of elements in the source.</typeparam>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="valueSelector">A function to extract a value from each element.</param>
        /// <returns>A new BiDirectionalDictionary containing the elements from the source collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if source, keySelector, or valueSelector is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the source contains duplicate keys or values after transformation.</exception>
        public static BiDirectionalDictionary<TKey, TValue> ToBiDirectionalDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector)
            where TKey : notnull
            where TValue : notnull
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            // Convert to array once to get the count and to avoid multiple enumeration
            var sourceArray = source.ToArray();
            var result = new BiDirectionalDictionary<TKey, TValue>(sourceArray.Length);
            
            // Transform into key-value pairs first
            var pairs = sourceArray.Select(item => new KeyValuePair<TKey, TValue>(
                keySelector(item), 
                valueSelector(item))).ToArray();
                
            // Add all at once for better performance
            result.AddRange(pairs);
            return result;
        }

        /// <summary>
        /// Performs the specified action on each element of the BiDirectionalDictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
        /// <param name="dictionary">The BiDirectionalDictionary instance.</param>
        /// <param name="action">The action to perform on each element.</param>
        /// <exception cref="ArgumentNullException">Thrown if dictionary or action is null.</exception>
        public static void ForEach<TKey, TValue>(
            this BiDirectionalDictionary<TKey, TValue> dictionary,
            Action<TKey, TValue> action)
            where TKey : notnull
            where TValue : notnull
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var kvp in dictionary)
            {
                action(kvp.Key, kvp.Value);
            }
        }
    }
}
