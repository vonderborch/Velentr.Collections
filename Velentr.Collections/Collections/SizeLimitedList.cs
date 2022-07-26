using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Velentr.Collections.CollectionActions;
using Velentr.Collections.Exceptions;

namespace Velentr.Collections.Collections
{
    /// <summary>
    ///     A list that is limited in maximum size.
    /// </summary>
    /// <typeparam name="T"> Generic type parameter. </typeparam>
    /// <seealso cref="Collection" />
    /// <seealso cref="IEnumerable{T}" />
    /// <seealso cref="IEnumerable" />
    [DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
    public class SizeLimitedList<T> : Collection, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        ///     The list.
        /// </summary>
        private readonly List<T> _list;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="maxSize">        (Optional) The maximum size of the. </param>
        /// <param name="actionWhenFull"> (Optional) The action when full. </param>
        public SizeLimitedList(long maxSize = 32, SizeLimitedFullAction actionWhenFull = SizeLimitedFullAction.PopOldestItem)
        {
            if (maxSize < 2)
            {
                throw new ArgumentException($"{nameof(maxSize)} must be at least 2!", nameof(maxSize));
            }

            this._list = new List<T>();
            this.ActionWhenFull = actionWhenFull;
            this.MaxSize = maxSize;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="capacity">       The capacity. </param>
        /// <param name="maxSize">        (Optional) The maximum size of the. </param>
        /// <param name="actionWhenFull"> (Optional) The action when full. </param>
        public SizeLimitedList(int capacity, long maxSize = 32, SizeLimitedFullAction actionWhenFull = SizeLimitedFullAction.PopOldestItem)
        {
            if (maxSize < 2)
            {
                throw new ArgumentException($"{nameof(maxSize)} must be at least 2!", nameof(maxSize));
            }

            this._list = new List<T>(capacity);
            this.ActionWhenFull = actionWhenFull;
            this.MaxSize = maxSize;
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="collection">     The collection. </param>
        /// <param name="maxSize">        (Optional) The maximum size of the. </param>
        /// <param name="actionWhenFull"> (Optional) The action when full. </param>
        public SizeLimitedList(IEnumerable<T> collection, long maxSize = 32, SizeLimitedFullAction actionWhenFull = SizeLimitedFullAction.PopOldestItem)
        {
            if (maxSize < 2)
            {
                throw new ArgumentException($"{nameof(maxSize)} must be at least 2!", nameof(maxSize));
            }

            this._list = new List<T>(collection);
            this.ActionWhenFull = actionWhenFull;
            this.MaxSize = maxSize;
        }

        /// <summary>
        ///     Gets or sets the action when full.
        /// </summary>
        /// <value>
        ///     The action when full.
        /// </value>
        public SizeLimitedFullAction ActionWhenFull { get; set; }

        /// <summary>
        ///     Gets or sets the maximum size.
        /// </summary>
        /// <value>
        ///     The maximum size of the.
        /// </value>
        public long MaxSize { get; set; }

        /// <summary>
        ///     Indexer to get or set items within this collection using array index syntax.
        /// </summary>
        /// <param name="index"> Zero-based index of the entry to access. </param>
        /// <returns>
        ///     The indexed item.
        /// </returns>
        public T this[int index]
        {
            get => this._list[index];

            set => this._list[index] = value;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
        ///     Adds an item and returns any popped items.
        /// </summary>
        /// <param name="item"> The item. </param>
        /// <returns>
        ///     The item that was popped to make space for the new item.
        /// </returns>
        public T AddItem(T item)
        {
            this._list.Add(item);

            if (this._list.Count >= this.MaxSize)
            {
                switch (this.ActionWhenFull)
                {
                    case SizeLimitedFullAction.PopOldestItem:
                        var newItem = this._list[0];
                        this._list.RemoveAt(0);

                        return newItem;

                    case SizeLimitedFullAction.PopNewestItem:
                        this._list.RemoveAt(this._list.Count - 2);

                        return item;
                }
            }

            IncrementCount();

            return default;
        }

        /// <summary>
        ///     Adds items and returns any popped items.
        /// </summary>
        /// <param name="items"> The items. </param>
        /// <returns>
        ///     The items that were popped to make space for the new item.
        /// </returns>
        public List<T> AddItem(IEnumerable<T> items)
        {
            var removedItems = new List<T>();
            var itemsToRemove = -1;
            var enumerable = items.ToList();
            while (this._list.Count + enumerable.Count >= this.MaxSize && this.ActionWhenFull != SizeLimitedFullAction.Ignore)
            {
                switch (this.ActionWhenFull)
                {
                    case SizeLimitedFullAction.PopOldestItem:
                        removedItems.Add(this._list[0]);
                        this._list.RemoveAt(0);

                        break;

                    case SizeLimitedFullAction.PopNewestItem:
                        removedItems.Add(this._list[this._list.Count - 1]);
                        this._list.RemoveAt(this._list.Count - 1);

                        break;
                }

                DecrementCount();
                if (this._list.Count == 0)
                {
                    itemsToRemove = enumerable.Count - (int) this.MaxSize;

                    break;
                }
            }

            while (itemsToRemove > -1)
            {
                enumerable.RemoveAt(0);
                itemsToRemove--;
            }

            foreach (var item in enumerable)
            {
                this._list.Add(item);
                IncrementCount();
            }

            return removedItems;
        }

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        /// <seealso cref="Velentr.Collections.Collections.Collection.Clear()" />
        public override void Clear()
        {
            this._version = 0;
            var oldCount = this.Count;
            this._list.Clear();
            UpdateCount(-oldCount);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <seealso cref="Velentr.Collections.Collections.Collection.Dispose()" />
        public override void Dispose()
        {
            Clear();
            this._disposed = true;
        }

        /// <summary>
        ///     Determine if 'item' exists.
        /// </summary>
        /// <param name="item"> The item. </param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        public bool Exists(T item)
        {
            return this._list.FindIndex(x => x.Equals(item)) != -1;
        }

        /// <summary>
        ///     Searches for the first index.
        /// </summary>
        /// <param name="item"> The item. </param>
        /// <returns>
        ///     The found index.
        /// </returns>
        public int FindIndex(T item)
        {
            return this._list.FindIndex(x => x.Equals(item));
        }

        /// <summary>
        ///     Gets an item.
        /// </summary>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns>
        ///     The item.
        /// </returns>
        public T GetItem(int index)
        {
            if (index < 0 || index >= this._list.Count)
            {
                return default;
            }

            return this._list[index];
        }

        /// <summary>
        ///     Gets the snapshot.
        /// </summary>
        /// <returns>
        ///     The snapshot.
        /// </returns>
        public List<T> GetSnapshot()
        {
            return new List<T>(this._list);
        }

        /// <summary>
        ///     Removes at described by index.
        /// </summary>
        /// <param name="index"> Zero-based index of the. </param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                return false;
            }

            this._list.RemoveAt(index);
            DecrementCount();

            return true;
        }

        /// <summary>
        ///     Removes the item described by item.
        /// </summary>
        /// <param name="item"> The item. </param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        public bool RemoveItem(T item)
        {
            var output = this._list.Remove(item);

            if (output)
            {
                DecrementCount();
            }

            return output;
        }

        /// <summary>
        ///     Removes the item described by item.
        /// </summary>
        /// <param name="items"> The items. </param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        public List<bool> RemoveItem(IEnumerable<T> items)
        {
            var output = new List<bool>();
            foreach (var item in items)
            {
                output.Add(RemoveItem(item));
            }

            return output;
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        private IEnumerator<T> GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        ///     Internals the get enumerator.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CollectionModifiedException"></exception>
        private IEnumerator<T> InternalGetEnumerator()
        {
            var enumeratorVersion = this._version;
            for (var i = 0; i < this.Count; i++)
            {
                if (enumeratorVersion != this._version)
                {
                    throw new CollectionModifiedException();
                }

                yield return this._list[i];
            }
        }
    }
}
