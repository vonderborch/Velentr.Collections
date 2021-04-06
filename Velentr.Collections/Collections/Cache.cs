using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Velentr.Collections.Helpers;

namespace Velentr.Collections.Collections
{

    /// <summary>
    /// A Thread-Safe and Lock-Free dictionary optimized for reads
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    [DebuggerDisplay("Count = {Count}")]
    public class Cache<K, V> : Collection
    {

        /// <summary>
        /// The dictionary
        /// </summary>
        private IImmutableDictionary<K, V> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{K, V}"/> class.
        /// </summary>
        public Cache()
        {
            _dictionary = ImmutableDictionary.Create<K, V>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cache{K, V}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public Cache(Dictionary<K, V> dictionary)
        {
            _dictionary = dictionary.ToImmutableDictionary();
            UpdateCount(dictionary.Count);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public override void Clear()
        {
            UpdateCount(-_dictionary.Count);
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            _dictionary.Clear();
            _version = 0;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _disposed = true;
            Clear();
            _dictionary = null;
        }

        /// <summary>
        /// Adds the specified key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool Add(K key, V value)
        {
            return GetOrAdd(key, value, out _);
        }

        /// <summary>
        /// Appends the specified new items to the Cache.
        /// </summary>
        /// <param name="newItems">The new items.</param>
        public void Append(Dictionary<K, V> newItems)
        {
            IImmutableDictionary<K, V> oldDictionary;
            IImmutableDictionary<K, V> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                newDictionary = oldDictionary.AddRange(newItems);
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            UpdateCount(newItems.Count);
            IncrementVersion();
        }

        /// <summary>
        /// Appends the specified new items to the Cache.
        /// </summary>
        /// <param name="newItems">The new items.</param>
        public void Append(List<KeyValuePair<K, V>> newItems)
        {
            IImmutableDictionary<K, V> oldDictionary;
            IImmutableDictionary<K, V> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                newDictionary = oldDictionary.AddRange(newItems);
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            UpdateCount(newItems.Count);
            IncrementVersion();
        }

        /// <summary>
        /// Gets or adds the key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        public V GetOrAdd(K key, V value)
        {
            GetOrAdd(key, value, out var output);
            return output;
        }

        /// <summary>
        /// Gets or adds the key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="outValue">The out value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool GetOrAdd(K key, V value, out V outValue)
        {
            if (TryGetValue(key, out outValue))
            {
                return true;
            }

            IImmutableDictionary<K, V> oldDictionary;
            IImmutableDictionary<K, V> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                if (oldDictionary.TryGetValue(key, out var output))
                {
                    outValue = output;
                    return output.Equals(outValue);
                }

                newDictionary = oldDictionary.Add(key, value);
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            IncrementCount();
            outValue = value;
            return true;
        }

        /// <summary>
        /// Gets, adds, or updates the key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        public V GetOrAddOrUpdate(K key, V value)
        {
            GetOrAddOrUpdate(key, value, out var output);
            return output;
        }

        /// <summary>
        /// Gets, adds, or updates the key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="outValue">The out value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool GetOrAddOrUpdate(K key, V value, out V outValue)
        {
            if (TryGetValue(key, out outValue))
            {
                return true;
            }

            bool setItem;
            IImmutableDictionary<K, V> oldDictionary;
            IImmutableDictionary<K, V> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                if (oldDictionary.TryGetValue(key, out var output))
                {
                    if (output.Equals(value))
                    {
                        outValue = value;
                        return true;
                    }

                    newDictionary = oldDictionary.SetItem(key, value);
                    setItem = true;
                }
                else
                {
                    newDictionary = oldDictionary.Add(key, value);
                    setItem = false;
                }
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            if (!setItem)
            {
                IncrementCount();
            }
            else
            {
                IncrementVersion();
            }

            outValue = value;
            return true;
        }

        /// <summary>
        /// Gets or updates the key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        public V GetOrUpdate(K key, V value)
        {
            GetOrUpdate(key, value, out var output);
            return output;
        }

        /// <summary>
        /// Gets or updates the key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="outValue">The out value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool GetOrUpdate(K key, V value, out V outValue)
        {
            if (TryGetValue(key, out outValue))
            {
                return true;
            }

            if (_dictionary.TryGetValue(key, out var output))
            {
                if (!output.Equals(value))
                {
                    IImmutableDictionary<K, V> oldDictionary;
                    IImmutableDictionary<K, V> newDictionary;
                    do
                    {
                        oldDictionary = _dictionary.ToImmutableDictionary();
                        if (oldDictionary.TryGetValue(key, out output))
                        {
                            if (output.Equals(value))
                            {
                                outValue = value;
                                return true;
                            }

                            newDictionary = oldDictionary.SetItem(key, value);
                        }
                        else
                        {
                            outValue = default;
                            return false;
                        }
                    } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));
                }

                IncrementVersion();
                outValue = value;
                return true;
            }

            outValue = default;
            return false;
        }

        /// <summary>
        /// Tries to get the key.
        /// </summary>
        /// <param name="searchKey">The search key.</param>
        /// <returns>The key.</returns>
        public K TryGetKey(K searchKey)
        {
            TryGetKey(searchKey, out var value);
            return value;
        }

        /// <summary>
        /// Tries to get the key.
        /// </summary>
        /// <param name="searchKey">The search key.</param>
        /// <param name="actualKey">The actual key.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryGetKey(K searchKey, out K actualKey)
        {
            return _dictionary.TryGetKey(searchKey, out actualKey);
        }

        /// <summary>
        /// Tries to get the value associated with the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public V TryGetValue(K key)
        {
            TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Tries to get the value associated with the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool TryGetValue(K key, out V value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets a snapshot of the Cache.
        /// </summary>
        /// <returns>A snapshot of the Cache as a dictionary.</returns>
        public Dictionary<K, V> GetSnapshot()
        {
            return _dictionary.ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Cache{TKey, TValue}"/> to <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Dictionary<K, V>(Cache<K, V> source)
        {
            return source.GetSnapshot();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Dictionary{TKey, TValue}"/> to <see cref="Cache{TKey, TValue}"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Cache<K, V>(Dictionary<K, V> source)
        {
            return new Cache<K, V>(source);
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(K key)
        {
            IImmutableDictionary<K, V> oldDictionary;
            IImmutableDictionary<K, V> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                newDictionary = oldDictionary.Remove(key);
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            DecrementCount();
        }

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public void RemoveRange(List<K> keys)
        {
            IImmutableDictionary<K, V> oldDictionary;
            IImmutableDictionary<K, V> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                newDictionary = oldDictionary.RemoveRange(keys);
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            UpdateCount(-keys.Count);
            IncrementVersion();
        }

    }

}
