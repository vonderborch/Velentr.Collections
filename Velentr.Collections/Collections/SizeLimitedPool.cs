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
    ///     A size-limited pool
    /// </summary>
    /// <typeparam name="T"> Generic type parameter. </typeparam>
    /// <seealso cref="Collection" />
    /// <seealso cref="IEnumerable{T}" />
    /// <seealso cref="IEnumerable" />
    [DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
    public class SizeLimitedPool<T> : Collection, IEnumerable<T>, IEnumerable
    {
        private const bool IsFree = true;
        private const bool IsUsed = !IsFree;

        /// <summary>
        ///     The internal data structure.
        ///     (T, bool) => Item, IsFree
        /// </summary>
        private readonly List<InternalStructure<T>> _list;

        private int _actionIndex;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="maxSize">        (Optional) The maximum size of the. </param>
        public SizeLimitedPool(int maxSize = 32, SizeLimitedFullAction actionWhenFull = SizeLimitedFullAction.PopOldestItem)
        {
            if (maxSize < 2)
            {
                throw new ArgumentException($"{nameof(maxSize)} must be at least 2!", nameof(maxSize));
            }

            if (actionWhenFull == SizeLimitedFullAction.Ignore)
            {
                throw new ArgumentException($"{nameof(actionWhenFull)} can't be configured as SizeLimitedFullAction.Ignore!", nameof(actionWhenFull));
            }

            this._list = new List<InternalStructure<T>>(maxSize);
            this.ActionWhenFull = actionWhenFull;
            this.MaxSize = maxSize;
            while (this.MaxSize > this._list.Count)
            {
                this._list.Add(new InternalStructure<T>());
            }
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="collection">     The collection. </param>
        /// <param name="maxSize">        (Optional) The maximum size of the. </param>
        public SizeLimitedPool(IEnumerable<T> collection, int maxSize = 32, SizeLimitedFullAction actionWhenFull = SizeLimitedFullAction.PopOldestItem)
        {
            if (maxSize < 2)
            {
                throw new ArgumentException($"{nameof(maxSize)} must be at least 2!", nameof(maxSize));
            }

            if (actionWhenFull == SizeLimitedFullAction.Ignore)
            {
                throw new ArgumentException($"{nameof(actionWhenFull)} can't be configured as SizeLimitedFullAction.Ignore!", nameof(actionWhenFull));
            }

            this.ActionWhenFull = actionWhenFull;

            this._list = new List<InternalStructure<T>>();
            foreach (var item in collection)
            {
                this._list.Add(new InternalStructure<T>(item));
            }

            this.MaxSize = this._list.Count > maxSize ? this._list.Count : maxSize;
            while (this.MaxSize > this._list.Count)
            {
                this._list.Add(new InternalStructure<T>());
            }
        }

        /// <summary>
        ///     Gets the action when full.
        /// </summary>
        /// <value>
        ///     The action when full.
        /// </value>
        public SizeLimitedFullAction ActionWhenFull { get; }

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
            get
            {
                if (index < 0 || index >= this._list.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (this._list[index].IsSlotFree == IsFree)
                {
                    throw new Exception("No item in the slot!");
                }

                return this._list[index].Item;
            }

            set
            {
                if (index < 0 || index >= this._list.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                this._list[index].AssociateItem(value);
            }
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
        public void AddItem(T item)
        {
            // Two scenarios
            // Scenario 1: free slot -> used first unused slot
            // Scenario 2: no free slots -> use the specified Action When Full

            var usedFreeSlot = false;
            for (var i = 0; i < this._list.Count; i++)
            {
                if (this._list[i].IsSlotFree == IsFree)
                {
                    this._list[i].AssociateItem(item);
                    usedFreeSlot = true;
                    IncrementCount();

                    if (this.ActionWhenFull == SizeLimitedFullAction.PopNewestItem)
                    {
                        this._actionIndex = i;
                    }

                    break;
                }
            }

            if (!usedFreeSlot)
            {
                switch (this.ActionWhenFull)
                {
                    case SizeLimitedFullAction.PopNewestItem:
                        this._list[this._actionIndex].AssociateItem(item);

                        break;

                    case SizeLimitedFullAction.PopOldestItem:
                        this._list[this._actionIndex++].AssociateItem(item);
                        if (this._actionIndex >= this._list.Count)
                        {
                            this._actionIndex = 0;
                        }

                        break;
                }
            }
        }

        /// <summary>
        ///     Adds items and returns any popped items.
        /// </summary>
        /// <param name="items"> The items. </param>
        public void AddItem(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        /// <summary>
        ///     Adds an item and returns any popped items.
        /// </summary>
        /// <param name="item"> The item. </param>
        /// <returns>
        ///     The item that was popped to make space for the new item.
        /// </returns>
        public T AddItemWithReturn(T item)
        {
            // Two scenarios
            // Scenario 1: free slot -> used first unused slot
            // Scenario 2: no free slots -> use the specified Action When Full

            var usedFreeSlot = false;
            for (var i = 0; i < this._list.Count; i++)
            {
                if (this._list[i].IsSlotFree == IsFree)
                {
                    this._list[i].AssociateItem(item);
                    usedFreeSlot = true;
                    IncrementCount();

                    if (this.ActionWhenFull == SizeLimitedFullAction.PopNewestItem)
                    {
                        this._actionIndex = i;
                    }

                    break;
                }
            }

            if (!usedFreeSlot)
            {
                T oldItem;
                switch (this.ActionWhenFull)
                {
                    case SizeLimitedFullAction.PopNewestItem:
                        oldItem = this._list[this._actionIndex].Item;
                        this._list[this._actionIndex].AssociateItem(item);

                        return oldItem;

                    case SizeLimitedFullAction.PopOldestItem:
                        oldItem = this._list[this._actionIndex].Item;
                        this._list[this._actionIndex++].AssociateItem(item);
                        if (this._actionIndex >= this._list.Count)
                        {
                            this._actionIndex = 0;
                        }

                        return oldItem;
                }
            }

            return default;
        }

        /// <summary>
        ///     Adds items and returns any popped items.
        /// </summary>
        /// <param name="items"> The items. </param>
        /// <returns>
        ///     The items that were popped to make space for the new item.
        /// </returns>
        public List<T> AddItemWithReturn(IEnumerable<T> items)
        {
            var removedItems = new List<T>();
            foreach (var item in items)
            {
                removedItems.Add(AddItemWithReturn(item));
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
            for (var i = 0; i < this._list.Count; i++)
            {
                this._list[i].ClearSlot();
            }

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
            this._list.Clear();
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

            return this._list[index].Item;
        }

        /// <summary>
        ///     Gets the snapshot.
        /// </summary>
        /// <returns>
        ///     The snapshot.
        /// </returns>
        public List<T> GetSnapshot()
        {
            return new List<T>(this._list.Where(x => x.IsSlotFree == IsUsed).Select(x => x.Item));
        }

        /// <summary>
        ///     Removes and returns the top-of-stack object.
        /// </summary>
        /// <returns>
        ///     The previous top-of-stack object.
        /// </returns>
        public T Pop()
        {
            T oldItem = default;
            if (this._list.Exists(x => x.IsSlotFree == IsUsed))
            {
                switch (this.ActionWhenFull)
                {
                    case SizeLimitedFullAction.PopNewestItem:
                        oldItem = this._list[this._actionIndex].Item;
                        this._list[this._actionIndex--].ClearSlot();
                        DecrementCount();

                        return oldItem;

                    case SizeLimitedFullAction.PopOldestItem:
                        oldItem = this._list[this._actionIndex].Item;
                        this._list[this._actionIndex++].ClearSlot();
                        DecrementCount();
                        if (this._actionIndex >= this._list.Count)
                        {
                            this._actionIndex = 0;
                        }

                        return oldItem;
                }
            }

            return oldItem;
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

            this._list[index].ClearSlot();
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
            var index = this._list.FindIndex(x => x.Item.Equals(item));
            if (index != -1)
            {
                this._list[index].ClearSlot();
                DecrementCount();

                return true;
            }

            return false;
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

                yield return this._list[i].Item;
            }
        }

        /// <summary>
        ///     An internal structure.
        /// </summary>
        /// <typeparam name="T">    Generic type parameter. </typeparam>
        internal class InternalStructure<T>
        {
            /// <summary>
            ///     True if is slot free, false if not.
            /// </summary>
            public bool IsSlotFree;

            /// <summary>
            ///     The item.
            /// </summary>
            public T Item;

            /// <summary>
            ///     Default constructor.
            /// </summary>
            public InternalStructure()
            {
                this.IsSlotFree = true;
                this.Item = default;
            }

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="item"> The item. </param>
            public InternalStructure(T item)
            {
                this.IsSlotFree = false;
                this.Item = item;
            }

            /// <summary>
            ///     Associate item.
            /// </summary>
            /// <param name="item"> The item. </param>
            public void AssociateItem(T item)
            {
                this.IsSlotFree = false;
                this.Item = item;
            }

            /// <summary>
            ///     Clears the slot.
            /// </summary>
            public void ClearSlot()
            {
                this.IsSlotFree = true;
            }
        }
    }
}
