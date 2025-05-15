using Velentr.Collections.CollectionFullActions;
using Velentr.Collections.Events;
using Velentr.Core.Eventing;

namespace Velentr.Collections;

/// <summary>
///     Represents a pool of items that can be added, retrieved, and managed with capacity control.
/// </summary>
/// <typeparam name="T">The type of items stored in the pool.</typeparam>
public interface IPool<T> : ICollection<T>, IDisposable
{
    /// <summary>
    ///     Gets the maximum size of the pool.
    /// </summary>
    int MaxSize { get; }

    /// <summary>
    ///     Gets the remaining capacity of the pool.
    /// </summary>
    int RemainingCapacity { get; }

    /// <summary>
    ///     Gets or sets the action to take when the pool is full.
    /// </summary>
    PoolFullAction ActionWhenFull { get; set; }

    /// <summary>
    ///     Event that is triggered when a slot is claimed in the pool.
    /// </summary>
    Event<ClaimedSlotPoolEventArgs<T>> ClaimedSlotEvent { get; set; }

    /// <summary>
    ///     Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    /// <exception cref="Exception">Thrown when accessing an empty slot or overwriting an occupied slot.</exception>
    T this[int index] { get; set; }

    /// <summary>
    ///     Event that is triggered when a slot is released in the pool.
    /// </summary>
    Event<ReleasedSlotPoolEventArgs<T>> ReleasedSlotEvent { get; set; }

    /// <summary>
    ///     Event that is triggered when a slot claim operation fails.
    /// </summary>
    Event<SlotClaimFailureEventArgs<T>> SlotClaimFailureEvent { get; set; }

    /// <summary>
    ///     Adds an item to the pool and returns any item that was removed due to the pool being full.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was removed, or the default value of <typeparamref name="T" /> if no item was removed.</returns>
    T? AddAndReturn(T item);

    /// <summary>
    ///     Adds a range of items to the pool.
    /// </summary>
    /// <param name="items">The items to add.</param>
    void AddRange(IEnumerable<T> items);

    /// <summary>
    ///     Adds a range of items to the pool and returns any items that were removed.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>A list of items that were removed from the pool.</returns>
    List<T?> AddRangeAndReturnRemoved(IEnumerable<T> items);

    /// <summary>
    ///     Clears all items from the pool and emits events for each item cleared.
    /// </summary>
    void ClearAndEmitEvents();

    /// <summary>
    ///     Returns an item to the pool, making it available for reuse.
    /// </summary>
    /// <param name="item">The item to return to the pool.</param>
    /// <returns>True if the item was successfully returned; false otherwise.</returns>
    /// <think>
    public bool Return(T item)
    {
        return Remove(item);
    }

    /// <summary>
    ///     Gets an array of all items in the pool.
    /// </summary>
    /// <returns>An array of items in the pool.</returns>
    public T[] ToArray();

    /// <summary>
    ///     Gets a list of all items in the pool.
    /// </summary>
    /// <returns>A list of items in the pool.</returns>
    public List<T> ToList();
}
