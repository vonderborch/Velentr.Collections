using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Velentr.Collections.Exceptions;
using Velentr.Core.Helpers.Validation;

namespace Velentr.Collections.Collections
{
    /// <summary>
    ///     A Collection that combines functionality of a dictionary and a list.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="KeyValuePair{TKey,TValue}" />
    /// <seealso cref="System.Collections.IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class OrderedDictionary<TKey, TValue> : Collection, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        /// <summary>
        ///     The order
        /// </summary>
        private readonly List<TKey> _order;

        /// <summary>
        ///     The values
        /// </summary>
        private readonly Dictionary<TKey, TValue> _values;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderedDictionary{K, V}" /> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public OrderedDictionary(int capacity = 16)
        {
            _order = new List<TKey>(capacity);
            _values = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        ///     Gets or sets the <see cref="TValue" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="TValue" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        public TValue this[int index]
        {
            get => _values[_order[index]];
            set => UpdateItem(index, value);
        }

        /// <summary>
        ///     Gets or sets the <see cref="TValue" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="TValue" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public TValue this[TKey index]
        {
            get => _values[index];
            set => UpdateItem(index, value);
        }

        /// <summary>
        ///     Adds the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <param name="forceAdd">if set to <c>true</c> [force add].</param>
        /// <returns>Whether we were able to add the item or not.</returns>
        public bool AddItem(TKey key, TValue value, int index = int.MaxValue, bool forceAdd = false)
        {
            return AddItemAndGetMetadata(key, value, index, forceAdd).Item1;
        }

        /// <summary>
        ///     Adds the item and gets metadata.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <param name="forceAdd">if set to <c>true</c> [force add].</param>
        /// <returns>The metadata for the added item.</returns>
        public (bool, int, TKey) AddItemAndGetMetadata(TKey key, TValue value, int index = int.MaxValue, bool forceAdd = false)
        {
            if (_values.ContainsKey(key))
            {
                if (!forceAdd)
                {
                    return (false, -1, key);
                }

                _values[key] = value;
                IncrementVersion();

                index = GetIndexForKey(key);
                return (index == -1, index, key);
            }

            Validations.ValidateRange(index, nameof(index), 0);

            if (index >= Count)
            {
                _order.Add(key);
                _values.Add(key, value);
                IncrementCount();

                return (true, _order.Count - 1, key);
            }

            _order.Insert(index, key);
            _values.Add(key, value);
            IncrementCount();
            return (true, index, key);
        }

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        public override void Clear()
        {
            _version = 0;
            var oldCount = Count;
            _order.Clear();
            _values.Clear();
            UpdateCount(-oldCount);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _disposed = true;
            _order.Clear();
            _values.Clear();
        }

        /// <summary>
        /// Whether an item with the specified key exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public bool Exists(TKey key)
        {
            return GetIndexForKey(key) != -1;
        }

        /// <summary>
        /// Whether an item with the specified index exists.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public bool Exists(int index)
        {
            return index >= 0 && index < Count;
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
        ///     Gets the index for key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The item's index.</returns>
        public int GetIndexForKey(TKey key)
        {
            return _order.FindIndex(k => k.Equals(key));
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the key.</returns>
        public TValue GetItem(TKey key)
        {
            if (_values.TryGetValue(key, out var value))
            {
                return value;
            }

            throw new InvalidKeyException();
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        public TValue GetItem(int index)
        {
            return GetItemAndMetadata(index).Item2;
        }

        /// <summary>
        ///     Gets the item and metadata.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public (TKey, TValue, int) GetItemAndMetadata(int index)
        {
            Validations.ValidateRange(index, nameof(index), 0, Count);

            var key = _order[index];
            return (key, _values[key], index);
        }

        /// <summary>
        ///     Gets the item and metadata.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public (TKey, TValue, int) GetItemAndMetadata(TKey key)
        {
            TValue value;
            if (!_values.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException();
            }

            var index = GetIndexForKey(key);
            return (key, _values[key], index);
        }

        /// <summary>
        ///     Gets the key for the index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item's key.</returns>
        public TKey GetKeyForIndex(int index)
        {
            Validations.ValidateRange(index, nameof(index), 0, Count);
            return _order[index];
        }

        /// <summary>
        ///     Pops the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The metadata for the item.</returns>
        public (TKey, TValue, int) PopItem(TKey key)
        {
            var item = GetItemAndMetadata(key);
            _values.Remove(key);
            _order.RemoveAt(item.Item3);
            DecrementCount();

            return item;
        }

        /// <summary>
        ///     Pops the item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The metadata for the item.</returns>
        public (TKey, TValue, int) PopItem(int index)
        {
            Validations.ValidateRange(index, nameof(index), 0, Count);

            var key = _order[index];
            var value = _values[key];

            _values.Remove(key);
            _order.RemoveAt(index);
            DecrementCount();

            return (key, value, index);
        }

        /// <summary>
        ///     Removes the item.
        /// </summary>
        /// <param name="key">The key.</param>
        public void RemoveItem(TKey key)
        {
            PopItem(key);
        }

        /// <summary>
        ///     Removes the item.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveItem(int index)
        {
            PopItem(index);
        }

        /// <summary>
        ///     Updates an item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void UpdateItem(int index, TValue value)
        {
            Validations.ValidateRange(index, nameof(index), 0, Count);

            _values[_order[index]] = value;
            IncrementVersion();
        }

        /// <summary>
        ///     Updates an item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void UpdateItem(TKey key, TValue value)
        {
            _values[key] = value;
            IncrementVersion();
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
            for (var i = 0; i < Count; i++)
            {
                if (enumeratorVersion != _version)
                {
                    throw new CollectionModifiedException();
                }

                yield return new KeyValuePair<TKey, TValue>(_order[i], _values[_order[i]]);
            }
        }
    }
}