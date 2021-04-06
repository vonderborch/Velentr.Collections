using System.Collections;
using System.Collections.Generic;
using Collections.Net.Exceptions;
using Collections.Net.Helpers;

namespace Collections.Net.Collections
{

    /// <summary>
    ///     A Collection that combines functionality of a dictionary and a list.
    /// </summary>
    /// <typeparam name="K">The type of the keys.</typeparam>
    /// <typeparam name="V">The type of the values.</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{K, V}}" />
    /// <seealso cref="System.Collections.IEnumerable" />
    public class Bank<K, V> : Collection, IEnumerable<KeyValuePair<K, V>>, IEnumerable
    {

        /// <summary>
        ///     The order
        /// </summary>
        private readonly List<K> _order;

        /// <summary>
        ///     The values
        /// </summary>
        private readonly Dictionary<K, V> _values;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Bank{K, V}" /> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public Bank(int capacity = 16)
        {
            _order = new List<K>(capacity);
            _values = new Dictionary<K, V>(capacity);
        }

        /// <summary>
        ///     Gets or sets the <see cref="V" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="V" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public V this[int index]
        {
            get => _values[_order[index]];
            set => UpdateItem(index, value);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
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
        ///     Adds the item and gets metadata.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <param name="forceAdd">if set to <c>true</c> [force add].</param>
        /// <returns>The metadata for the added item.</returns>
        public (bool, int, K) AddItemAndGetMetadata(K key, V value, int index = int.MaxValue, bool forceAdd = false)
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

            Validation.ValidateRange(index, nameof(index), 0);

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
        ///     Adds the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <param name="forceAdd">if set to <c>true</c> [force add].</param>
        /// <returns>Whether we were able to add the item or not.</returns>
        public bool AddItem(K key, V value, int index = int.MaxValue, bool forceAdd = false)
        {
            return AddItemAndGetMetadata(key, value, index, forceAdd).Item1;
        }

        /// <summary>
        ///     Updates an item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void UpdateItem(int index, V value)
        {
            Validation.ValidateRange(index, nameof(index), 0, Count);

            _values[_order[index]] = value;
            IncrementVersion();
        }

        /// <summary>
        /// Whether an item with the specified key exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public bool Exists(K key)
        {
            return GetIndexForKey(key) == -1;
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
        ///     Gets the item and metadata.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public (K, V, int) GetItemAndMetadata(int index)
        {
            Validation.ValidateRange(index, nameof(index), 0, Count);

            var key = _order[index];
            return (key, _values[key], index);
        }

        /// <summary>
        ///     Gets the item and metadata.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public (K, V, int) GetItemAndMetadata(K key)
        {
            V value;
            if (!_values.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException();
            }

            var index = GetIndexForKey(key);
            return (key, _values[key], index);
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the key.</returns>
        public V GetItem(K key)
        {
            if (!_values.TryGetValue(key, out var value))
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
        public V GetItem(int index)
        {
            return GetItemAndMetadata(index).Item2;
        }

        /// <summary>
        ///     Gets the index for key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The item's index.</returns>
        public int GetIndexForKey(K key)
        {
            return _order.FindIndex(k => k.Equals(key));
        }

        /// <summary>
        ///     Gets the key for the index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item's key.</returns>
        public K GetKeyForIndex(int index)
        {
            Validation.ValidateRange(index, nameof(index), 0, Count);
            return _order[index];
        }

        /// <summary>
        ///     Pops the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The metadata for the item.</returns>
        public (K, V, int) PopItem(K key)
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
        public (K, V, int) PopItem(int index)
        {
            Validation.ValidateRange(index, nameof(index), 0, Count);

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
        public void RemoveItem(K key)
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
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        private IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        ///     Internals the get enumerator.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionModifiedException"></exception>
        private IEnumerator<KeyValuePair<K, V>> InternalGetEnumerator()
        {
            var enumeratorVersion = _version;
            for (var i = 0; i < Count; i++)
            {
                if (enumeratorVersion != _version)
                {
                    throw new CollectionModifiedException();
                }

                yield return new KeyValuePair<K, V>(_order[i], _values[_order[i]]);
            }
        }

    }

}
