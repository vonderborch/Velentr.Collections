using System;
using System.Diagnostics;
using System.Threading;
using Collections.Net.CollectionActions;
using Collections.Net.Events;

namespace Collections.Net.Collections.LockFree
{

    /// <summary>
    /// A Pool of objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    [DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
    public class LockFreePool<T> : Collection
    {

        /// <summary>
        /// The constructor parameters
        /// </summary>
        private readonly object[] _constructorParameters;

        /// <summary>
        /// The maximum size
        /// </summary>
        private long _maxSize;

        /// <summary>
        /// The pool
        /// </summary>
        private readonly LockFreeQueue<T> _pool;

        /// <summary>
        /// The created event
        /// </summary>
        public CollectionEvent<PoolEventArgs<T>> CreatedEvent;

        /// <summary>
        /// The returned event
        /// </summary>
        public CollectionEvent<PoolEventArgs<T>> ReturnedEvent;

        /// <summary>
        /// The reused event
        /// </summary>
        public CollectionEvent<PoolEventArgs<T>> ReusedEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockFreePool{T}"/> class.
        /// </summary>
        /// <param name="constructorParameters">The constructor parameters.</param>
        /// <param name="actionWhenPoolFull">The action when pool full.</param>
        /// <param name="capacity">The initial capacity.</param>
        public LockFreePool(object[] constructorParameters = null, PoolFullAction actionWhenPoolFull = PoolFullAction.IncreaseSize, int capacity = 0)
        {
            _pool = new LockFreeQueue<T>();
            ActionWhenPoolFull = actionWhenPoolFull;
            _constructorParameters = constructorParameters ?? new object[] { };

            if (capacity > 0)
            {
                for (var i = 0; i < capacity; i++)
                {
                    _pool.Enqueue((T) Activator.CreateInstance(typeof(T), _constructorParameters));
                }
            }
        }

        /// <summary>
        /// Gets or sets the action when pool full.
        /// </summary>
        /// <value>
        /// The action when pool full.
        /// </value>
        public PoolFullAction ActionWhenPoolFull { get; set; }

        /// <summary>
        /// Gets the free capacity.
        /// </summary>
        /// <value>
        /// The free capacity.
        /// </value>
        public long FreeCapacity => Count;

        /// <summary>
        /// Gets the maximum capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity.
        /// </value>
        public long MaxCapacity => _maxSize;

        /// <summary>
        /// Gets an instance from the pool.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">The pool is full!</exception>
        public T Get()
        {
            if (!_pool.Dequeue(out var result))
            {
                switch (ActionWhenPoolFull)
                {
                    case PoolFullAction.IncreaseSize:
                        result = (T) Activator.CreateInstance(typeof(T), _constructorParameters);
                        CreatedEvent?.TriggerEvent(this, new PoolEventArgs<T>(result));
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
                ReusedEvent?.TriggerEvent(this, new PoolEventArgs<T>(result));
                IncrementVersion();
            }

            return result;
        }

        /// <summary>
        /// Returns the item to the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Return(T item)
        {
            _pool.Enqueue(item);
            ReturnedEvent?.TriggerEvent(this, new PoolEventArgs<T>(item));
            IncrementCount();
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public override void Clear()
        {
            var oldCount = Count;
            _version = 0;
            _pool.Clear();
            _maxSize = 0;
            UpdateCount(-oldCount);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _disposed = true;
            _pool.Dispose();
            CreatedEvent?.Clear();
            ReusedEvent?.Clear();
            ReturnedEvent?.Clear();
        }

        /// <summary>
        ///     Decrements the count.
        /// </summary>
        private void IncrementMaxCapacity()
        {
            Interlocked.Increment(ref _maxSize);
        }

    }

}
