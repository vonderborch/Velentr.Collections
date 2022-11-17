using System;
using System.Diagnostics;
using System.Threading;

using Velentr.Collections.CollectionActions;
using Velentr.Collections.Events;
using Velentr.Core.Helpers.General;

namespace Velentr.Collections.LockFree
{
    /// <summary>
    ///     A Pool of objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    [DebuggerDisplay("FreeCapacity = {FreeCapacity}, MaxCapacity = {MaxCapacity}")]
    public class LockFreePool<T> : Collection
    {
        /// <summary>
        ///     The constructor parameters
        /// </summary>
        private readonly object[] _constructorParameters;

        /// <summary>
        ///     The pool
        /// </summary>
        private readonly LockFreeQueue<T> _pool;

        /// <summary>
        ///     The maximum size
        /// </summary>
        private long _maxSize;

        /// <summary>
        ///     The created event
        /// </summary>
        public CollectionEvent<PoolEventArgs<T>> CreatedEvent;

        /// <summary>
        ///     The returned event
        /// </summary>
        public CollectionEvent<PoolEventArgs<T>> ReturnedEvent;

        /// <summary>
        ///     The reused event
        /// </summary>
        public CollectionEvent<PoolEventArgs<T>> ReusedEvent;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="constructorParameters">
        ///     (Optional)
        ///     The constructor parameters.
        /// </param>
        /// <param name="actionWhenPoolFull">
        ///     (Optional)
        ///     The action when pool full.
        /// </param>
        /// <param name="capacity">              (Optional) The capacity. </param>
        /// <param name="maxCapacity">
        ///     (Optional)
        ///     The maximum capacity.
        /// </param>
        /// <param name="pruningAction">         (Optional) The pruning action. </param>
        public LockFreePool(object[] constructorParameters = null, PoolFullAction actionWhenPoolFull = PoolFullAction.IncreaseSize, int capacity = 0, long maxCapacity = 32, PoolPruningAction pruningAction = PoolPruningAction.Ignore)
        {
            this._pool = new LockFreeQueue<T>();
            this.ActionWhenPoolFull = actionWhenPoolFull;
            this._constructorParameters = constructorParameters ?? new object[] { };

            if (capacity > 0)
            {
                for (var i = 0; i < capacity; i++)
                {
                    this._pool.Enqueue((T) Activator.CreateInstance(typeof(T), this._constructorParameters));
                }
            }

            this._maxSize = maxCapacity;
            this.ActionWhenPruningPool = pruningAction;
        }

        /// <summary>
        ///     Gets or sets the action when pool full.
        /// </summary>
        /// <value>
        ///     The action when pool full.
        /// </value>
        public PoolFullAction ActionWhenPoolFull { get; set; }

        /// <summary>
        ///     Gets or sets the action when pruning pool.
        /// </summary>
        /// <value>
        ///     The action when pruning pool.
        /// </value>
        public PoolPruningAction ActionWhenPruningPool { get; set; }

        /// <summary>
        ///     Gets the free capacity.
        /// </summary>
        /// <value>
        ///     The free capacity.
        /// </value>
        public long FreeCapacity => this.Count;

        /// <summary>
        ///     Gets the maximum capacity.
        /// </summary>
        /// <value>
        ///     The maximum capacity.
        /// </value>
        public long MaxCapacity => this._maxSize;

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        public override void Clear()
        {
            var oldCount = this.Count;
            this._version = 0;
            this._pool.Clear();
            this._maxSize = 0;
            UpdateCount(-oldCount);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this._disposed = true;
            this._pool.Dispose();
            this.CreatedEvent?.Clear();
            this.ReusedEvent?.Clear();
            this.ReturnedEvent?.Clear();
        }

        /// <summary>
        ///     Gets an instance from the pool.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">The pool is full!</exception>
        public T Get()
        {
            if (!this._pool.Dequeue(out var result))
            {
                switch (this.ActionWhenPoolFull)
                {
                    case PoolFullAction.IncreaseSize:
                        result = (T) Activator.CreateInstance(typeof(T), this._constructorParameters);
                        this.CreatedEvent?.TriggerEvent(this, new PoolEventArgs<T>(result));
                        IncrementVersion();
                        IncrementMaxCapacity();

                        break;

                    case PoolFullAction.ReturnNull:
                        return default;

                    case PoolFullAction.ThrowException:
                        throw new Exception("The pool is full!");
                }
            }
            else
            {
                this.ReusedEvent?.TriggerEvent(this, new PoolEventArgs<T>(result));
                IncrementVersion();
            }

            return result;
        }

        /// <summary>
        ///     Returns the item to the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Return(T item)
        {
            // if we've reached our max capacity and an object is returned, we should dispose of it
            if (this.FreeCapacity >= this.MaxCapacity && this.ActionWhenPruningPool == PoolPruningAction.PruneToMaxCapacity)
            {
                DisposingHelpers.DisposeIfPossible(item);
            }

            // otherwise, return it to the pool
            else
            {
                this._pool.Enqueue(item);
                this.ReturnedEvent?.TriggerEvent(this, new PoolEventArgs<T>(item));
                IncrementCount();
            }
        }

        /// <summary>
        ///     Decrements the count.
        /// </summary>
        private void IncrementMaxCapacity()
        {
            Interlocked.Increment(ref this._maxSize);
        }
    }
}
