using System.Collections;
using System.Diagnostics;
using Velentr.Collections.CollectionFullActions;
using Velentr.Collections.Events;

namespace Velentr.Collections.Concurrent;

/// <summary>
/// A thread-safe pool collection that manages a fixed number of reusable objects.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class ConcurrentPool<T> : ICollection<T>, IDisposable
{
    /// <summary>
    /// The underlying pool that handles all operations with thread safety enabled
    /// </summary>
    private readonly Pool<T> _internalPool;

    /// <summary>
    /// Synchronization object
    /// </summary>
    private readonly object _lock = new object();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentPool{T}"/> class.
    /// </summary>
    /// <param name="maxSize">The maximum size of the pool.</param>
    /// <param name="actionWhenFull">The action to take when the pool is full.</param>
    public ConcurrentPool(int maxSize = 32, PoolFullAction actionWhenFull = PoolFullAction.PopOldestItem)
    {
        _internalPool = new Pool<T>(maxSize, actionWhenFull);
        
        // Connect to the internal pool's events
        _internalPool.ClaimedSlotEvent = new CollectionEvent<ClaimedSlotPoolEventArgs<T>>();
        _internalPool.ReleasedSlotEvent = new CollectionEvent<ReleasedSlotPoolEventArgs<T>>();
        _internalPool.SlotClaimFailureEvent = new CollectionEvent<SlotClaimFailureEventArgs<T>>();
    }

    /// <summary>
    /// Gets the remaining capacity of the pool.
    /// </summary>
    public int RemainingCapacity
    {
        get
        {
            lock (_lock)
            {
                return _internalPool.RemainingCapacity;
            }
        }
    }

    /// <summary>
    /// Gets or sets the action to take when the pool is full.
    /// </summary>
    public PoolFullAction ActionWhenFull
    {
        get
        {
            lock (_lock)
            {
                return _internalPool.ActionWhenFull;
            }
        }
        set
        {
            lock (_lock)
            {
                _internalPool.ActionWhenFull = value;
            }
        }
    }

    /// <summary>
    /// Gets the maximum size of the pool.
    /// </summary>
    public int MaxSize
    {
        get
        {
            lock (_lock)
            {
                return _internalPool.MaxSize;
            }
        }
    }

    /// <summary>
    /// Gets the number of items currently in the pool.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _internalPool.Count;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the pool is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get
        {
            lock (_lock)
            {
                return _internalPool.IsReadOnly;
            }
        }
    }

    /// <summary>
    /// Event that is triggered when a slot is claimed in the pool.
    /// </summary>
    public CollectionEvent<ClaimedSlotPoolEventArgs<T>> ClaimedSlotEvent
    {
        get => this._internalPool.ClaimedSlotEvent;
        set => this._internalPool.ClaimedSlotEvent = value;
    }

    /// <summary>
    /// Event that is triggered when a slot is released in the pool.
    /// </summary>
    public CollectionEvent<ReleasedSlotPoolEventArgs<T>> ReleasedSlotEvent
    {
        get => _internalPool.ReleasedSlotEvent;
        set => this._internalPool.ReleasedSlotEvent = value;
    }

    /// <summary>
    /// Event that is triggered when a slot claim operation fails.
    /// </summary>
    public CollectionEvent<SlotClaimFailureEventArgs<T>> SlotClaimFailureEvent
    {
        get => _internalPool.SlotClaimFailureEvent;
        set => this._internalPool.SlotClaimFailureEvent = value;
    }

    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item at the specified index.</returns>
    public T this[int index]
    {
        get
        {
            lock (_lock)
            {
                return _internalPool[index];
            }
        }
        set
        {
            lock (_lock)
            {
                _internalPool[index] = value;
            }
        }
    }

    /// <summary>
    /// Adds an item to the pool.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        lock (_lock)
        {
            _internalPool.Add(item);
        }
    }

    /// <summary>
    /// Adds an item to the pool and returns any item that was removed due to the pool being full.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was removed, or the default value of <typeparamref name="T"/> if no item was removed.</returns>
    public T? AddAndReturn(T item)
    {
        lock (_lock)
        {
            return _internalPool.AddAndReturn(item);
        }
    }

    /// <summary>
    /// Adds a range of items to the pool.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        lock (_lock)
        {
            _internalPool.AddRange(items);
        }
    }

    /// <summary>
    /// Adds a range of items to the pool and returns any items that were removed.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>A list of items that were removed from the pool.</returns>
    public List<T?> AddRangeReturn(IEnumerable<T> items)
    {
        lock (_lock)
        {
            return _internalPool.AddRangeReturn(items);
        }
    }

    /// <summary>
    /// Clears all items from the pool.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _internalPool.Clear();
        }
    }

    /// <summary>
    /// Clears all items from the pool and emits events for each item cleared.
    /// </summary>
    public void ClearAndEmitEvents()
    {
        lock (_lock)
        {
            _internalPool.ClearAndEmitEvents();
        }
    }

    /// <summary>
    /// Determines whether the pool contains a specific item.
    /// </summary>
    /// <param name="item">The item to locate in the pool.</param>
    /// <returns>True if the item is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        lock (_lock)
        {
            return _internalPool.Contains(item);
        }
    }

    /// <summary>
    /// Copies the elements of the pool to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_lock)
        {
            _internalPool.CopyTo(array, arrayIndex);
        }
    }

    /// <summary>
    /// Gets all items currently in the pool.
    /// </summary>
    /// <returns>A list of items in the pool.</returns>
    public List<T> GetItems()
    {
        lock (_lock)
        {
            return _internalPool.GetItems();
        }
    }

    /// <summary>
    /// Removes the first occurrence of a specific item from the pool.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        lock (_lock)
        {
            return _internalPool.Remove(item);
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the pool.
    /// </summary>
    /// <returns>An enumerator for the pool.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        lock (_lock)
        {
            return _internalPool.GetItems().GetEnumerator();
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the pool.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Releases all resources used by the pool.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _internalPool.Dispose();
        }
    }
}
