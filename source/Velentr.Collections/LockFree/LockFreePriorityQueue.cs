using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using Velentr.Collections.Internal;

namespace Velentr.Collections.LockFree;

/// <summary>
///     A lock-free implementation of a priority queue data structure where lower priority values are dequeued first.
///     This implementation is thread-safe without using locks, providing better scalability in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class LockFreePriorityQueue<T> : ICollection, IEnumerable<T>
{
    [JsonIgnore] private readonly int priorityLevels;

    [JsonIgnore] private readonly LockFreeQueue<T>[] queues;

    [JsonIgnore] private int count;

    [JsonIgnore] private long nonEmptyQueuesBitmap;

    [JsonIgnore] private long version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreePriorityQueue{T}" /> class with a default number of priority
    ///     levels (32).
    /// </summary>
    public LockFreePriorityQueue() : this(32)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreePriorityQueue{T}" /> class with the specified number of
    ///     priority levels.
    /// </summary>
    /// <param name="priorityLevels">The number of priority levels (1-64).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when priorityLevels is less than 1 or greater than 64.</exception>
    public LockFreePriorityQueue(int priorityLevels)
    {
        if (priorityLevels < 1 || priorityLevels > 64)
        {
            throw new ArgumentOutOfRangeException(nameof(priorityLevels), "Priority levels must be between 1 and 64.");
        }

        this.priorityLevels = priorityLevels;
        this.queues = new LockFreeQueue<T>[priorityLevels];
        for (var i = 0; i < priorityLevels; i++)
        {
            this.queues[i] = new LockFreeQueue<T>();
        }

        this.nonEmptyQueuesBitmap = 0;
        this.version = 0;
        this.count = 0;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreePriorityQueue{T}" /> class with an initial item and priority.
    /// </summary>
    /// <param name="item">The item to add to the queue.</param>
    /// <param name="priority">The priority of the item (lower values are dequeued first).</param>
    public LockFreePriorityQueue(T item, int priority) : this(32)
    {
        Enqueue(item, priority);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreePriorityQueue{T}" /> class with an initial collection of items
    ///     and priorities.
    /// </summary>
    /// <param name="items">The items to add to the queue.</param>
    /// <param name="priority">The priority for all items (lower values are dequeued first).</param>
    [JsonConstructor]
    public LockFreePriorityQueue(IEnumerable<T> items, int priority) : this(32)
    {
        foreach (T item in items)
        {
            Enqueue(item, priority);
        }
    }

    /// <summary>
    ///     Transforms the queue into a list in priority order (lowest priority first).
    /// </summary>
    [JsonPropertyName("collection")]
    public List<T> ToList
    {
        get
        {
            var startingVersion = this.version;
            List<T> list = new(this.count);
            foreach (T item in this)
            {
                CollectionValidators.ValidateCollectionState(startingVersion, this.version);
                list.Add(item);
            }

            return list;
        }
    }

    /// <summary>
    ///     Gets the number of elements contained in the queue.
    /// </summary>
    public int Count => this.count;

    /// <summary>
    ///     Gets a value indicating whether access to the queue is synchronized (thread safe).
    ///     Always returns true for LockFreePriorityQueue as it's thread-safe by design.
    /// </summary>
    public bool IsSynchronized => true;

    /// <summary>
    ///     Gets an object that can be used to synchronize access to the queue.
    ///     Note: This property exists for ICollection compatibility but lock-free collections
    ///     don't require external synchronization.
    /// </summary>
    [field: JsonIgnore]
    public object SyncRoot { get; } = new();

    /// <summary>
    ///     Copies the elements of the queue to an array, starting at a particular index.
    ///     Elements are copied in priority order (lowest priority first).
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
    /// <param name="index">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is less than zero.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown if array is multidimensional or if the number of elements in the source
    ///     exceeds available space.
    /// </exception>
    public void CopyTo(Array array, int index)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative");
        }

        if (array.Rank > 1)
        {
            throw new ArgumentException("Multidimensional arrays are not supported", nameof(array));
        }

        if (array.Length - index < this.count)
        {
            throw new ArgumentException("Not enough space in array from index to end of destination array");
        }

        var currentIndex = index;
        foreach (T item in this)
        {
            array.SetValue(item, currentIndex++);
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the queue in priority order (lowest priority first).
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        var startingVersion = this.version;

        for (var priority = 0; priority < this.priorityLevels; priority++)
        {
            foreach (T item in this.queues[priority])
            {
                CollectionValidators.ValidateCollectionState(startingVersion, this.version);

                yield return item;
            }
        }
    }

    /// <summary>
    ///     Removes all elements from the queue.
    /// </summary>
    public void Clear()
    {
        for (var i = 0; i < this.priorityLevels; i++)
        {
            this.queues[i].Clear();
        }

        Interlocked.Exchange(ref this.nonEmptyQueuesBitmap, 0);
        Interlocked.Exchange(ref this.count, 0);
        Interlocked.Increment(ref this.version);
    }

    /// <summary>
    ///     Determines whether the queue contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the queue.</param>
    /// <returns>true if item is found in the queue; otherwise, false.</returns>
    public bool Contains(T item)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (var priority = 0; priority < this.priorityLevels; priority++)
        {
            LockFreeQueue<T> queue = this.queues[priority];
            foreach (T queueItem in queue)
            {
                if (item == null)
                {
                    if (queueItem == null)
                    {
                        return true;
                    }
                }
                else if (queueItem != null && comparer.Equals(queueItem, item))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Removes and returns the element with the lowest priority from the queue.
    /// </summary>
    /// <returns>The element with the lowest priority, or default value if the queue is empty.</returns>
    public T? Dequeue()
    {
        Dequeue(out T? result);
        return result;
    }

    /// <summary>
    ///     Tries to remove and return the element with the lowest priority from the queue.
    /// </summary>
    /// <param name="value">
    ///     When this method returns, contains the element with the lowest priority, or the default value if
    ///     the queue is empty.
    /// </param>
    /// <returns>true if an element was removed; otherwise, false.</returns>
    public bool Dequeue(out T? value)
    {
        var spinCount = 0;
        const int MaxSpins = 10; // Limit spinning to prevent CPU exhaustion

        while (spinCount < MaxSpins)
        {
            var bitmap = Interlocked.Read(ref this.nonEmptyQueuesBitmap);
            if (bitmap == 0)
            {
                value = default;
                return false;
            }

            var priority = FindLowestPriority(bitmap);

            // Check if the bit is still set before attempting to dequeue
            var mask = 1L << priority;
            if ((bitmap & mask) == 0)
            {
                // The bit was cleared by another thread, retry with updated bitmap
                spinCount++;
                Thread.Yield();
                continue;
            }

            if (this.queues[priority].Dequeue(out value))
            {
                // First update counters to maintain consistency
                Interlocked.Decrement(ref this.count);
                Interlocked.Increment(ref this.version);

                // Then update bitmap if necessary - if the queue is empty, clear its bit
                if (this.queues[priority].Count == 0)
                {
                    var clearMask = ~mask;

                    // Use atomic operation to clear the bit
                    Interlocked.And(ref this.nonEmptyQueuesBitmap, clearMask);
                }

                return true;
            }

            {
                // We hit a race condition - the bitmap says there are items in this queue,
                // but we couldn't dequeue any. This happens if another thread dequeued
                // the last item but hasn't updated the bitmap yet.

                // Try to help by clearing this priority's bit in the bitmap
                var clearMask = ~mask;
                Interlocked.And(ref this.nonEmptyQueuesBitmap, clearMask);

                // Increment spin count to avoid infinite loops
                spinCount++;
                Thread.Yield(); // Give other threads a chance to run
            }
        }

        // After maximum spins, we still couldn't dequeue successfully
        // This is a fallback case that should rarely happen
        value = default;
        return false;
    }

    /// <summary>
    ///     Adds an element with the specified priority to the queue.
    /// </summary>
    /// <param name="item">The element to add to the queue.</param>
    /// <param name="priority">The priority of the element (lower values are dequeued first).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when priority is out of range.</exception>
    public void Enqueue(T item, int priority)
    {
        if (priority < 0 || priority >= this.priorityLevels)
        {
            throw new ArgumentOutOfRangeException(nameof(priority),
                $"Priority must be between 0 and {this.priorityLevels - 1}.");
        }

        // First enqueue the item to ensure it's available before setting the bit
        this.queues[priority].Enqueue(item);

        // Set the bit corresponding to this priority level
        // Using 1L ensures we get a long (64-bit) shift even on 32-bit systems
        var mask = 1L << priority;
        Interlocked.Or(ref this.nonEmptyQueuesBitmap, mask);

        // Increment counters after the item is fully available
        Interlocked.Increment(ref this.count);
        Interlocked.Increment(ref this.version);
    }

    /// <summary>
    ///     Finds the lowest set bit in a bitmap, which corresponds to the lowest priority non-empty queue.
    /// </summary>
    /// <param name="bitmap">The bitmap of non-empty queues.</param>
    /// <returns>The index of the lowest set bit.</returns>
    private int FindLowestPriority(long bitmap)
    {
        // Find the position of the least significant bit that is set
        return BitOperations.TrailingZeroCount((ulong)bitmap);
    }

    /// <summary>
    ///     Gets the element with the lowest priority without removing it.
    /// </summary>
    /// <returns>The element with the lowest priority, or default value if the queue is empty.</returns>
    public T? Peek()
    {
        Peek(out T? result);
        return result;
    }

    /// <summary>
    ///     Tries to get the element with the lowest priority without removing it.
    /// </summary>
    /// <param name="value">
    ///     When this method returns, contains the element with the lowest priority, or the default value if
    ///     the queue is empty.
    /// </param>
    /// <returns>true if the queue is not empty; otherwise, false.</returns>
    public bool Peek(out T? value)
    {
        var bitmap = Interlocked.Read(ref this.nonEmptyQueuesBitmap);
        if (bitmap == 0)
        {
            value = default;
            return false;
        }

        var priority = FindLowestPriority(bitmap);
        value = this.queues[priority].Peek();
        return true;
    }
}
