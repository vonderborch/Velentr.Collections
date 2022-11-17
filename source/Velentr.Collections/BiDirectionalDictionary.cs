using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Velentr.Collections.Exceptions;

namespace Velentr.Collections
{
    /// <summary>
    ///     A dictionary implementation where keys can be accessed using the value and vice-versa.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <seealso cref="Collection" />
    /// <seealso cref="IEnumerable{T}" />
    /// <seealso cref="IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class BiDirectionalDictionary<TKey, TValue> : Collection, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        /// <summary>
        ///     A lock for when we add new items to the dictionaries.
        /// </summary>
        private readonly object _addLock = new();

        /// <summary>
        ///     The internal dictionary representing the keys to values mapping.
        /// </summary>
        private readonly Dictionary<TKey, TValue> _keyToValue;

        /// <summary>
        ///     The internal dictionary representing the values to keys mapping.
        /// </summary>
        private readonly Dictionary<TValue, TKey> _valueToKey;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public BiDirectionalDictionary()
        {
            this._keyToValue = new Dictionary<TKey, TValue>();
            this._valueToKey = new Dictionary<TValue, TKey>();
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="dictionary">The base dictionary to convert.</param>
        public BiDirectionalDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this._keyToValue = new Dictionary<TKey, TValue>(dictionary);
            this._valueToKey = new Dictionary<TValue, TKey>(dictionary.ToDictionary(x => x.Value, x => x.Key));
        }

        /// <summary>
        ///     Gets or sets the <see cref="TKey" /> at the specified value.
        /// </summary>
        /// <value>
        ///     The <see cref="TValue" />.
        /// </value>
        /// <param name="v">The value.</param>
        /// <returns>The key.</returns>
        public TKey this[TValue v]
        {
            get => this._valueToKey[v];

            set
            {
                var oldKey = this._valueToKey[v];
                this._valueToKey[v] = value;
                this._keyToValue.Remove(oldKey);
                this._keyToValue.Add(value, v);
            }
        }

        /// <summary>
        ///     Gets or sets the <see cref="TValue" /> at the specified key.
        /// </summary>
        /// <value>
        ///     The <see cref="TValue" />.
        /// </value>
        /// <param name="k">The key.</param>
        /// <returns>The value.</returns>
        public TValue this[TKey k]
        {
            get => this._keyToValue[k];

            set
            {
                var oldValue = this._keyToValue[k];
                this._keyToValue[k] = value;
                this._valueToKey.Remove(oldValue);
                this._valueToKey.Add(value, k);
            }
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
        ///     Adds an item to the bi-directional dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddItem(TKey key, TValue value)
        {
            lock (this._addLock)
            {
                if (this._keyToValue.ContainsKey(key))
                {
                    throw new ArgumentException(nameof(key), $"An item with the key [{key}] already exists!");
                }

                if (this._valueToKey.ContainsKey(value))
                {
                    throw new ArgumentException(nameof(value), $"An item with the value [{value}] already exists!");
                }

                this._keyToValue.Add(key, value);
                this._valueToKey.Add(value, key);
                IncrementCount();
            }
        }

        /// <summary>
        ///     Removes an item.
        /// </summary>
        /// <param name="key">The key.</param>
        public void RemoveItem(TKey key)
        {
            var value = this._keyToValue[key];
            this._keyToValue.Remove(key);
            this._valueToKey.Remove(value);
            DecrementCount();
        }

        /// <summary>
        ///     Removes an item.
        /// </summary>
        /// <param name="value">The value.</param>
        public void RemoveItem(TValue value)
        {
            var key = this._valueToKey[value];
            this._keyToValue.Remove(key);
            this._valueToKey.Remove(value);
            DecrementCount();
        }

        /// <summary>
        ///     Whether an item with the key already exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Whether a matching item exists.</returns>
        public bool ContainsKey(TKey key)
        {
            return this._keyToValue.ContainsKey(key);
        }

        /// <summary>
        ///     Whether an item with the value already exists.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Whether a matching item exists.</returns>
        public bool ContainsValue(TValue value)
        {
            return this._valueToKey.ContainsKey(value);
        }

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        public override void Clear()
        {
            UpdateCount(0);
            this._keyToValue.Clear();
            this._valueToKey.Clear();
        }

        /// <summary>
        ///     Disposes the collection.
        /// </summary>
        public override void Dispose()
        {
            UpdateCount(0);
            this._keyToValue.Clear();
            this._valueToKey.Clear();
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
            var enumeratorVersion = this._version;
            foreach (var pair in this._keyToValue)
            {
                if (enumeratorVersion != this._version)
                {
                    throw new CollectionModifiedException();
                }

                yield return pair;
            }
        }
    }
}
