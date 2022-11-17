using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Velentr.Collections.CollectionActions;
using Velentr.Collections.Exceptions;

namespace Velentr.Collections
{
    /// <summary>
    ///     A collection of items stored as a history type collection, implementing undo and redo functionality.
    /// </summary>
    /// <typeparam name="T"> Generic type parameter. </typeparam>
    /// <seealso cref="Collection" />
    [DebuggerDisplay("Count = {Count}, Current Position = {CurrentPosition}, Can Undo = {CanUndo}, Can Redo = {CanRedo}")]
    public class HistoryCollection<T> : Collection, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        ///     The current position.
        /// </summary>
        private int _currentPosition;

        /// <summary>
        ///     The internal list.
        /// </summary>
        private readonly SizeLimitedList<T> _list;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="maxHistoryItems"> (Optional) The maximum number of items to store in the history collection. </param>
        public HistoryCollection(int maxHistoryItems = 32)
        {
            this._list = new SizeLimitedList<T>(maxHistoryItems, SizeLimitedFullAction.PopOldestItem);
        }

        /// <summary>
        ///     The current position in the collection.
        /// </summary>
        public int CurrentPosition => this._currentPosition;

        /// <summary>
        ///     The highest index in the history collection.
        /// </summary>
        public int MaxHistoryIndex => (int) this._list.Count - 1;

        /// <summary>
        ///     Whether we can currently perform an Undo call.
        /// </summary>
        public bool CanUndo => this._currentPosition != 0;

        /// <summary>
        ///     Whether we can currently perform an Redo call.
        /// </summary>
        public bool CanRedo => this._currentPosition != this.MaxHistoryIndex;

        /// <summary>
        ///     Gets the current item.
        /// </summary>
        /// <value>
        ///     The current item.
        /// </value>
        public T CurrentItem => this._list[this._currentPosition];

        /// <summary>
        ///     Gets all items in the list.
        /// </summary>
        public List<T> GetAllItems => new(this._list);

        /// <summary>
        ///     Gets the newest item.
        /// </summary>
        /// <value>
        ///     The newest item.
        /// </value>
        public T NewestItem => this._list[this.MaxHistoryIndex];

        /// <summary>
        ///     Gets the oldest item.
        /// </summary>
        /// <value>
        ///     The oldest item.
        /// </value>
        public T OldestItem => this._list[0];

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
        ///     Undoes a number of items back.
        /// </summary>
        /// <param name="numberItems">The number of items to undo.</param>
        /// <returns>The value of the item the number back.</returns>
        public T Undo(int numberItems = 1)
        {
            if (!this.CanUndo)
            {
                throw new ArgumentException("Unable to undo, already at oldest item in history!");
            }

            if (numberItems < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberItems), "At least 1 item must be undone!");
            }

            if (numberItems >= this.Count)
            {
                numberItems = this.MaxHistoryIndex;
            }

            this._currentPosition -= numberItems;

            return this._list[this._currentPosition];
        }

        /// <summary>
        ///     Redoes a number of items forward.
        /// </summary>
        /// <param name="numberItems">The number of items to redo.</param>
        /// <returns>The value of the item the number forward.</returns>
        public T Redo(int numberItems = 1)
        {
            if (!this.CanRedo)
            {
                throw new ArgumentException("Unable to redo, already at newest item in history!");
            }

            if (numberItems < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberItems), "At least 1 item must be redone!");
            }

            if (numberItems + this._currentPosition >= this.Count)
            {
                numberItems = this.MaxHistoryIndex - this._currentPosition;
            }

            this._currentPosition += numberItems;

            return this._list[this._currentPosition];
        }

        /// <summary>
        ///     Adds an item to the history collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void AddItem(T item)
        {
            this._list.AddItem(item);

            // if we're not at the latest item, get rid of everything after our current position...
            if (this._currentPosition < this.MaxHistoryIndex - 1)
            {
                var indexToDelete = this._currentPosition + 1;
                do
                {
                    this._list.RemoveAt(indexToDelete);
                } while (this._currentPosition != this.MaxHistoryIndex);
            }

            this._currentPosition = this.MaxHistoryIndex;
            UpdateCount(this._list.Count);
        }

        /// <summary>
        ///     Resets the collection.
        /// </summary>
        public void Reset()
        {
            Clear();
        }

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        /// <seealso cref="Collection.Clear()" />
        public override void Clear()
        {
            UpdateCount(0);
            this._list.Clear();
            this._currentPosition = 0;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <seealso cref="Collection.Dispose()" />
        public override void Dispose()
        {
            UpdateCount(0);
            this._list.Dispose();
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
