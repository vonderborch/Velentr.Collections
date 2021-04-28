using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Velentr.Collections.Exceptions;
using Velentr.Collections.Helpers;

namespace Velentr.Collections.Collections
{
    /// <summary>
    /// A Thread-Safe and Lock-Free dictionary optimized for reads
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    [DebuggerDisplay("Count = {Count}")]
    public class DictionaryCache<TKey, TValue> : Collection, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        /// <summary>
        /// The dictionary
        /// </summary>
        private IImmutableDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryCache{K, V}"/> class.
        /// </summary>
        public DictionaryCache()
        {
            _dictionary = ImmutableDictionary.Create<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryCache{K, V}"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public DictionaryCache(Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary.ToImmutableDictionary();
            UpdateCount(dictionary.Count);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="DictionaryCache{TKey, TValue}"/> to <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Dictionary<TKey, TValue>(DictionaryCache<TKey, TValue> source)
        {
            return source.GetSnapshot();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Dictionary{TKey, TValue}"/> to <see cref="DictionaryCache{TKey, TValue}"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator DictionaryCache<TKey, TValue>(Dictionary<TKey, TValue> source)
        {
            return new DictionaryCache<TKey, TValue>(source);
        }

        /// <summary>
        /// Adds the specified key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool Add(TKey key, TValue value)
        {
            return GetOrAdd(key, value, out _);
        }

        /// <summary>
        /// Appends the specified new items to the Cache.
        /// </summary>
        /// <param name="newItems">The new items.</param>
        public void Append(Dictionary<TKey, TValue> newItems)
        {
            IImmutableDictionary<TKey, TValue> oldDictionary;
            IImmutableDictionary<TKey, TValue> newDictionary;
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
        public void Append(List<KeyValuePair<TKey, TValue>> newItems)
        {
            IImmutableDictionary<TKey, TValue> oldDictionary;
            IImmutableDictionary<TKey, TValue> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                newDictionary = oldDictionary.AddRange(newItems);
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            UpdateCount(newItems.Count);
            IncrementVersion();
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
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        /// Gets or adds the key and value to the Cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        public TValue GetOrAdd(TKey key, TValue value)
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
        public bool GetOrAdd(TKey key, TValue value, out TValue outValue)
        {
            if (TryGetValue(key, out outValue))
            {
                return true;
            }

            IImmutableDictionary<TKey, TValue> oldDictionary;
            IImmutableDictionary<TKey, TValue> newDictionary;
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
        public TValue GetOrAddOrUpdate(TKey key, TValue value)
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
        public bool GetOrAddOrUpdate(TKey key, TValue value, out TValue outValue)
        {
            if (TryGetValue(key, out outValue))
            {
                return true;
            }

            bool setItem;
            IImmutableDictionary<TKey, TValue> oldDictionary;
            IImmutableDictionary<TKey, TValue> newDictionary;
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
        public TValue GetOrUpdate(TKey key, TValue value)
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
        public bool GetOrUpdate(TKey key, TValue value, out TValue outValue)
        {
            if (TryGetValue(key, out outValue))
            {
                return true;
            }

            if (_dictionary.TryGetValue(key, out var output))
            {
                if (!output.Equals(value))
                {
                    IImmutableDictionary<TKey, TValue> oldDictionary;
                    IImmutableDictionary<TKey, TValue> newDictionary;
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
        /// Gets a snapshot of the Cache.
        /// </summary>
        /// <returns>A snapshot of the Cache as a dictionary.</returns>
        public Dictionary<TKey, TValue> GetSnapshot()
        {
            return _dictionary.ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(TKey key)
        {
            IImmutableDictionary<TKey, TValue> oldDictionary;
            IImmutableDictionary<TKey, TValue> newDictionary;
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
        public void RemoveRange(List<TKey> keys)
        {
            IImmutableDictionary<TKey, TValue> oldDictionary;
            IImmutableDictionary<TKey, TValue> newDictionary;
            do
            {
                oldDictionary = _dictionary.ToImmutableDictionary();
                newDictionary = oldDictionary.RemoveRange(keys);
            } while (!AtomicOperations.CAS(ref _dictionary, newDictionary, oldDictionary));

            UpdateCount(-keys.Count);
            IncrementVersion();
        }

        /// <summary>
        /// Tries to get the key.
        /// </summary>
        /// <param name="searchKey">The search key.</param>
        /// <returns>The key.</returns>
        public TKey TryGetKey(TKey searchKey)
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
        public bool TryGetKey(TKey searchKey, out TKey actualKey)
        {
            return _dictionary.TryGetKey(searchKey, out actualKey);
        }

        /// <summary>
        /// Tries to get the value associated with the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public TValue TryGetValue(TKey key)
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
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        ///     Internals the get enumerator.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CollectionModifiedException"></exception>
        private IEnumerator<KeyValuePair<TKey, TValue>> InternalGetEnumerator()
        {
            var enumeratorVersion = _version;
            foreach (var item in _dictionary)
            {
                if (enumeratorVersion != _version)
                {
                    throw new CollectionModifiedException();
                }

                yield return item;
            }
        }
    }
}