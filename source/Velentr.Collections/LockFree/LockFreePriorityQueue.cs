using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Velentr.Collections.LockFree;

/// <summary>
/// A lock-free implementation of a priority queue data structure where lower priority values are dequeued first.
/// This implementation is thread-safe without using locks, providing better scalability in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class LockFreePriorityQueue<T> : ICollection, IEnumerable<T>
{
    [JsonIgnore]
    private readonly LockFreeQueue<T>[] queues;
    
    [JsonIgnore]
    private readonly int priorityLevels;
    
    [JsonIgnore]
    private int count;
    
    [JsonIgnore]
    private ulong version;
    
    [JsonIgnore]
    private readonly object syncRoot = new object();
    
    [JsonIgnore]
    private long nonEmptyQueuesBitmap;

    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreePriorityQueue{T}"/> class with a default number of priority levels (32).
    /// </summary>
    public LockFreePriorityQueue() : this(32) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreePriorityQueue{T}"/> class with the specified number of priority levels.
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
        queues = new LockFreeQueue<T>[priorityLevels];
        for (int i = 0; i < priorityLevels; i++)
        {
            queues[i] = new LockFreeQueue<T>();
        }
        
        nonEmptyQueuesBitmap = 0;
        version = 0;
        count = 0;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreePriorityQueue{T}"/> class with an initial item and priority.
    /// </summary>
    /// <param name="item">The item to add to the queue.</param>
    /// <param name="priority">The priority of the item (lower values are dequeued first).</param>
    public LockFreePriorityQueue(T item, int priority) : this(32)
    {
        Enqueue(item, priority);
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreePriorityQueue{T}"/> class with an initial collection of items and priorities.
    /// </summary>
    /// <param name="items">The items to add to the queue.</param>
    /// <param name="priority">The priority for all items (lower values are dequeued first).</param>
    [JsonConstructor]
    public LockFreePriorityQueue(IEnumerable<T> items, int priority) : this(32)
    {
        foreach (var item in items)
        {
            Enqueue(item, priority);
        }
    }
    
    /// <summary>
    /// Transforms the queue into a list in priority order (lowest priority first).
    /// </summary>
    [JsonPropertyName("collection")]
    public List<T> ToList
    {
        get
        {
            var enumVersion = this.version;
            List<T> list = new List<T>(this.count);
            foreach (var item in this)
            {
                if (enumVersion != this.version)
                {
                    throw new InvalidOperationException("Collection was modified during enumeration.");
                }
                list.Add(item);
            }
            return list;
        }
    }

    /// <summary>
    /// Adds an element with the specified priority to the queue.
    /// </summary>
    /// <param name="item">The element to add to the queue.</param>
    /// <param name="priority">The priority of the element (lower values are dequeued first).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when priority is out of range.</exception>
    public void Enqueue(T item, int priority)
    {
        if (priority < 0 || priority >= priorityLevels)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), $"Priority must be between 0 and {priorityLevels - 1}.");
        }
        
        queues[priority].Enqueue(item);
        
        // Set the bit corresponding to this priority level
        Interlocked.Or(ref nonEmptyQueuesBitmap, 1L << priority);
        
        Interlocked.Increment(ref count);
        Interlocked.Increment(ref version);
    }
    
    /// <summary>
    /// Removes and returns the element with the lowest priority from the queue.
    /// </summary>
    /// <returns>The element with the lowest priority, or default value if the queue is empty.</returns>
    public T? Dequeue()
    {
        Dequeue(out var result);
        return result;
    }
    
    /// <summary>
    /// Tries to remove and return the element with the lowest priority from the queue.
    /// </summary>
    /// <param name="value">When this method returns, contains the element with the lowest priority, or the default value if the queue is empty.</param>
    /// <returns>true if an element was removed; otherwise, false.</returns>
    public bool Dequeue(out T? value)
    {
        while (true)
        {
            var bitmap = Interlocked.Read(ref nonEmptyQueuesBitmap);
            if (bitmap == 0)
            {
                value = default;
                return false;
            }
            
            // Find the lowest priority non-empty queue
            int priority = FindLowestPriority(bitmap);
            
            if (queues[priority].Dequeue(out value))
            {
                // If the queue is now empty, clear its bit in the bitmap
                if (queues[priority].Count == 0)
                {
                    // Clear the bit atomically using AND with complement
                    Interlocked.And(ref nonEmptyQueuesBitmap, ~(1L << priority));
                }
                
                Interlocked.Decrement(ref count);
                Interlocked.Increment(ref version);
                
                return true;
            }
            
            // If we couldn't dequeue (race condition), try again
        }
    }
    
    /// <summary>
    /// Gets the element with the lowest priority without removing it.
    /// </summary>
    /// <returns>The element with the lowest priority, or default value if the queue is empty.</returns>
    public T? Peek()
    {
        Peek(out var result);
        return result;
    }
    
    /// <summary>
    /// Tries to get the element with the lowest priority without removing it.
    /// </summary>
    /// <param name="value">When this method returns, contains the element with the lowest priority, or the default value if the queue is empty.</param>
    /// <returns>true if the queue is not empty; otherwise, false.</returns>
    public bool Peek(out T? value)
    {
        var bitmap = Interlocked.Read(ref nonEmptyQueuesBitmap);
        if (bitmap == 0)
        {
            value = default;
            return false;
        }
        
        int priority = FindLowestPriority(bitmap);
        value = queues[priority].Peek();
        return true;
    }
    
    /// <summary>
    /// Removes all elements from the queue.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < priorityLevels; i++)
        {
            queues[i].Clear();
        }
        
        Interlocked.Exchange(ref nonEmptyQueuesBitmap, 0);
        Interlocked.Exchange(ref count, 0);
        Interlocked.Increment(ref version);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the queue in priority order (lowest priority first).
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        var enumVersion = this.version;
        
        for (int priority = 0; priority < priorityLevels; priority++)
        {
            foreach (var item in queues[priority])
            {
                if (enumVersion != this.version)
                {
                    throw new InvalidOperationException("Collection was modified during enumeration.");
                }
                
                yield return item;
            }
        }
    }
    
    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Copies the elements of the queue to an array, starting at a particular index.
    /// Elements are copied in priority order (lowest priority first).
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
    /// <param name="index">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown if array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if index is less than zero.</exception>
    /// <exception cref="ArgumentException">Thrown if array is multidimensional or if the number of elements in the source exceeds available space.</exception>
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
        
        int currentIndex = index;
        foreach (T item in this)
        {
            array.SetValue(item, currentIndex++);
        }
    }
    
    /// <summary>
    /// Determines whether the queue contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the queue.</param>
    /// <returns>true if item is found in the queue; otherwise, false.</returns>
    public bool Contains(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (int priority = 0; priority < priorityLevels; priority++)
        {
            var queue = queues[priority];
            foreach (var queueItem in queue)
            {
                if (item == null)
                {
                    if (queueItem == null)
                        return true;
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
    /// Gets the number of elements contained in the queue.
    /// </summary>
    public int Count => count;
    
    /// <summary>
    /// Gets a value indicating whether access to the queue is synchronized (thread safe).
    /// Always returns true for LockFreePriorityQueue as it's thread-safe by design.
    /// </summary>
    public bool IsSynchronized => true;
    
    /// <summary>
    /// Gets an object that can be used to synchronize access to the queue.
    /// Note: This property exists for ICollection compatibility but lock-free collections
    /// don't require external synchronization.
    /// </summary>
    public object SyncRoot => syncRoot;
    
    /// <summary>
    /// Finds the lowest set bit in a bitmap, which corresponds to the lowest priority non-empty queue.
    /// </summary>
    /// <param name="bitmap">The bitmap of non-empty queues.</param>
    /// <returns>The index of the lowest set bit.</returns>
    private int FindLowestPriority(long bitmap)
    {
        // Find the position of the least significant bit that is set
        return BitOperations.TrailingZeroCount((ulong)bitmap);
    }
}
