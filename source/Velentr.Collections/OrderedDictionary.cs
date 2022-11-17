using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Velentr.Collections.Exceptions;
using Velentr.Core.Helpers.Validation;

namespace Velentr.Collections
{
    /// <summary>
    ///     A Collection that combines functionality of a dictionary and a list.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="KeyValuePair{TKey,TValue}" />
    /// <seealso cref="IEnumerable" />
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
            this._order = new List<TKey>(capacity);
            this._values = new Dictionary<TKey, TValue>(capacity);
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
            get => this._values[this._order[index]];

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
            get => this._values[index];

            set => UpdateItem(index, value);
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
            if (this._values.ContainsKey(key))
            {
                if (!forceAdd)
                {
                    return (false, -1, key);
                }

                this._values[key] = value;
                IncrementVersion();

                index = GetIndexForKey(key);

                return (index == -1, index, key);
            }

            Validations.ValidateRange(index, nameof(index), 0);

            if (index >= this.Count)
            {
                this._order.Add(key);
                this._values.Add(key, value);
                IncrementCount();

                return (true, this._order.Count - 1, key);
            }

            this._order.Insert(index, key);
            this._values.Add(key, value);
            IncrementCount();

            return (true, index, key);
        }

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        public override void Clear()
        {
            this._version = 0;
            var oldCount = this.Count;
            this._order.Clear();
            this._values.Clear();
            UpdateCount(-oldCount);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this._disposed = true;
            this._order.Clear();
            this._values.Clear();
        }

        /// <summary>
        ///     Whether an item with the specified key exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public bool Exists(TKey key)
        {
            return GetIndexForKey(key) != -1;
        }

        /// <summary>
        ///     Whether an item with the specified index exists.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public bool Exists(int index)
        {
            return index >= 0 && index < this.Count;
        }

        /// <summary>
        ///     Gets the index for key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The item's index.</returns>
        public int GetIndexForKey(TKey key)
        {
            return this._order.FindIndex(k => k.Equals(key));
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value associated with the key.</returns>
        public TValue GetItem(TKey key)
        {
            if (this._values.TryGetValue(key, out var value))
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
            Validations.ValidateRange(index, nameof(index), 0, this.Count);

            var key = this._order[index];

            return (key, this._values[key], index);
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
            if (!this._values.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException();
            }

            var index = GetIndexForKey(key);

            return (key, this._values[key], index);
        }

        /// <summary>
        ///     Gets the key for the index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item's key.</returns>
        public TKey GetKeyForIndex(int index)
        {
            Validations.ValidateRange(index, nameof(index), 0, this.Count);

            return this._order[index];
        }

        /// <summary>
        ///     Pops the item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The metadata for the item.</returns>
        public (TKey, TValue, int) PopItem(TKey key)
        {
            var item = GetItemAndMetadata(key);
            this._values.Remove(key);
            this._order.RemoveAt(item.Item3);
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
            Validations.ValidateRange(index, nameof(index), 0, this.Count);

            var key = this._order[index];
            var value = this._values[key];

            this._values.Remove(key);
            this._order.RemoveAt(index);
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
            Validations.ValidateRange(index, nameof(index), 0, this.Count);

            this._values[this._order[index]] = value;
            IncrementVersion();
        }

        /// <summary>
        ///     Updates an item.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void UpdateItem(TKey key, TValue value)
        {
            this._values[key] = value;
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
            var enumeratorVersion = this._version;
            for (var i = 0; i < this.Count; i++)
            {
                if (enumeratorVersion != this._version)
                {
                    throw new CollectionModifiedException();
                }

                yield return new KeyValuePair<TKey, TValue>(this._order[i], this._values[this._order[i]]);
            }
        }
    }
}
