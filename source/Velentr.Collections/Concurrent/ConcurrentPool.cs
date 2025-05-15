using System.Collections;
using System.Diagnostics;
using Velentr.Collections.CollectionFullActions;
using Velentr.Collections.Events;

namespace Velentr.Collections.Concurrent;

/// <summary>
///     A thread-safe pool collection that manages a fixed number of reusable objects.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class ConcurrentPool<T> : IPool<T>
{
    /// <summary>
    ///     The underlying pool that handles all operations with thread safety enabled
    /// </summary>
    private readonly Pool<T> internalPool;

    /// <summary>
    ///     Synchronization object
    /// </summary>
    private readonly Lock @lock = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentPool{T}" /> class.
    /// </summary>
    /// <param name="maxSize">The maximum size of the pool.</param>
    /// <param name="actionWhenFull">The action to take when the pool is full.</param>
    public ConcurrentPool(int maxSize = 32, PoolFullAction actionWhenFull = PoolFullAction.PopOldestItem)
    {
        this.internalPool = new Pool<T>(maxSize, actionWhenFull);

        // Connect to the internal pool's events
        this.internalPool.ClaimedSlotEvent = new CollectionEvent<ClaimedSlotPoolEventArgs<T>>();
        this.internalPool.ReleasedSlotEvent = new CollectionEvent<ReleasedSlotPoolEventArgs<T>>();
        this.internalPool.SlotClaimFailureEvent = new CollectionEvent<SlotClaimFailureEventArgs<T>>();
    }

    /// <summary>
    ///     Gets the number of items currently in the pool.
    /// </summary>
    public int Count
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.Count;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the pool is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.IsReadOnly;
            }
        }
    }

    /// <summary>
    ///     Gets the maximum size of the pool.
    /// </summary>
    public int MaxSize
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.MaxSize;
            }
        }
    }

    /// <summary>
    ///     Gets the remaining capacity of the pool.
    /// </summary>
    public int RemainingCapacity
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.RemainingCapacity;
            }
        }
    }

    /// <summary>
    ///     Gets or sets the action to take when the pool is full.
    /// </summary>
    public PoolFullAction ActionWhenFull
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.ActionWhenFull;
            }
        }
        set
        {
            lock (this.@lock)
            {
                this.internalPool.ActionWhenFull = value;
            }
        }
    }

    /// <summary>
    ///     Adds an item to the pool.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        lock (this.@lock)
        {
            this.internalPool.Add(item);
        }
    }

    /// <summary>
    ///     Adds an item to the pool and returns any item that was removed due to the pool being full.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was removed, or the default value of <typeparamref name="T" /> if no item was removed.</returns>
    public T? AddAndReturn(T item)
    {
        lock (this.@lock)
        {
            return this.internalPool.AddAndReturn(item);
        }
    }

    /// <summary>
    ///     Adds a range of items to the pool.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        lock (this.@lock)
        {
            this.internalPool.AddRange(items);
        }
    }

    /// <summary>
    ///     Adds a range of items to the pool and returns any items that were removed.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>A list of items that were removed from the pool.</returns>
    public List<T?> AddRangeAndReturnRemoved(IEnumerable<T> items)
    {
        lock (this.@lock)
        {
            return this.internalPool.AddRangeAndReturnRemoved(items);
        }
    }

    /// <summary>
    ///     Event that is triggered when a slot is claimed in the pool.
    /// </summary>
    public CollectionEvent<ClaimedSlotPoolEventArgs<T>> ClaimedSlotEvent
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.ClaimedSlotEvent;
            }
        }
        set
        {
            lock (this.@lock)
            {
                this.internalPool.ClaimedSlotEvent = value;
            }
        }
    }

    /// <summary>
    ///     Clears all items from the pool.
    /// </summary>
    public void Clear()
    {
        lock (this.@lock)
        {
            this.internalPool.Clear();
        }
    }

    /// <summary>
    ///     Clears all items from the pool and emits events for each item cleared.
    /// </summary>
    public void ClearAndEmitEvents()
    {
        lock (this.@lock)
        {
            this.internalPool.ClearAndEmitEvents();
        }
    }

    /// <summary>
    ///     Determines whether the pool contains a specific item.
    /// </summary>
    /// <param name="item">The item to locate in the pool.</param>
    /// <returns>True if the item is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        lock (this.@lock)
        {
            return this.internalPool.Contains(item);
        }
    }

    /// <summary>
    ///     Copies the elements of the pool to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (this.@lock)
        {
            this.internalPool.CopyTo(array, arrayIndex);
        }
    }

    /// <summary>
    ///     Releases all resources used by the pool.
    /// </summary>
    public void Dispose()
    {
        lock (this.@lock)
        {
            this.internalPool.Dispose();
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the pool.
    /// </summary>
    /// <returns>An enumerator for the pool.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        lock (this.@lock)
        {
            return this.internalPool.GetEnumerator();
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the pool.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item at the specified index.</returns>
    public T this[int index]
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool[index];
            }
        }
        set
        {
            lock (this.@lock)
            {
                this.internalPool[index] = value;
            }
        }
    }

    /// <summary>
    ///     Event that is triggered when a slot is released in the pool.
    /// </summary>
    public CollectionEvent<ReleasedSlotPoolEventArgs<T>> ReleasedSlotEvent
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.ReleasedSlotEvent;
            }
        }
        set
        {
            lock (this.@lock)
            {
                this.internalPool.ReleasedSlotEvent = value;
            }
        }
    }

    /// <summary>
    ///     Removes the first occurrence of a specific item from the pool.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        lock (this.@lock)
        {
            return this.internalPool.Remove(item);
        }
    }

    /// <summary>
    ///     Event that is triggered when a slot claim operation fails.
    /// </summary>
    public CollectionEvent<SlotClaimFailureEventArgs<T>> SlotClaimFailureEvent
    {
        get
        {
            lock (this.@lock)
            {
                return this.internalPool.SlotClaimFailureEvent;
            }
        }
        set
        {
            lock (this.@lock)
            {
                this.internalPool.SlotClaimFailureEvent = value;
            }
        }
    }

    /// <summary>
    ///     Gets an array of all items in the pool.
    /// </summary>
    /// <returns>An array of items in the pool.</returns>
    public T[] ToArray()
    {
        lock (this.@lock)
        {
            return this.internalPool.ToArray();
        }
    }

    /// <summary>
    ///     Gets a list of all items in the pool.
    /// </summary>
    /// <returns>A list of items in the pool.</returns>
    public List<T> ToList()
    {
        lock (this.@lock)
        {
            return this.internalPool.ToList();
        }
    }
}
