namespace Velentr.Collections.Concurrent;

/// <summary>
///     A thread-safe implementation of PriorityQueue that synchronizes access to the underlying queue.
///     This class provides concurrent operations on a priority queue by using locks to ensure thread safety.
/// </summary>
/// <typeparam name="TElement">The type of elements contained in the queue.</typeparam>
/// <typeparam name="TPriority">The type of priority associated with the elements.</typeparam>
public class ConcurrentPriorityQueue<TElement, TPriority> : PriorityQueue<TElement, TPriority>
{
    private readonly object _syncRoot = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}" /> class.
    /// </summary>
    public ConcurrentPriorityQueue()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}" /> class with the
    ///     specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial number of elements the queue can contain.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="initialCapacity" /> is less than 0.</exception>
    public ConcurrentPriorityQueue(int initialCapacity) : base(initialCapacity)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}" /> class with the
    ///     specified comparer.
    /// </summary>
    /// <param name="comparer">
    ///     The comparer used to determine the priority of elements. If null, the default comparer for
    ///     <typeparamref name="TPriority" /> is used.
    /// </param>
    public ConcurrentPriorityQueue(IComparer<TPriority> comparer) : base(comparer)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}" /> class with the
    ///     specified initial capacity and comparer.
    /// </summary>
    /// <param name="initialCapacity">The initial number of elements the queue can contain.</param>
    /// <param name="comparer">
    ///     The comparer used to determine the priority of elements. If null, the default comparer for
    ///     <typeparamref name="TPriority" /> is used.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="initialCapacity" /> is less than 0.</exception>
    public ConcurrentPriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
        : base(initialCapacity, comparer)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}" /> class with the
    ///     specified elements and priorities.
    /// </summary>
    /// <param name="items">The collection of element-priority pairs to add to the queue.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
    public ConcurrentPriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items)
        : base(items)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentPriorityQueue{TElement, TPriority}" /> class with the
    ///     specified elements, priorities, and comparer.
    /// </summary>
    /// <param name="items">The collection of element-priority pairs to add to the queue.</param>
    /// <param name="comparer">
    ///     The comparer used to determine the priority of elements. If null, the default comparer for
    ///     <typeparamref name="TPriority" /> is used.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
    public ConcurrentPriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items,
        IComparer<TPriority> comparer) : base(items, comparer)
    {
    }

    /// <summary>
    ///     Gets the number of elements contained in the queue in a thread-safe manner.
    /// </summary>
    public new int Count
    {
        get
        {
            lock (this._syncRoot)
            {
                return base.Count;
            }
        }
    }

    /// <summary>
    ///     Removes all elements from the queue in a thread-safe manner.
    /// </summary>
    public new void Clear()
    {
        lock (this._syncRoot)
        {
            base.Clear();
        }
    }

    /// <summary>
    ///     Removes and returns the element with the highest priority from the queue in a thread-safe manner.
    /// </summary>
    /// <returns>The element with the highest priority.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public new TElement Dequeue()
    {
        lock (this._syncRoot)
        {
            return base.Dequeue();
        }
    }

    /// <summary>
    ///     Adds an element with an associated priority to the queue in a thread-safe manner.
    /// </summary>
    /// <param name="element">The element to add to the queue.</param>
    /// <param name="priority">The priority associated with the element.</param>
    public new void Enqueue(TElement element, TPriority priority)
    {
        lock (this._syncRoot)
        {
            base.Enqueue(element, priority);
        }
    }

    /// <summary>
    ///     Adds a collection of element-priority pairs to the queue in a thread-safe manner.
    /// </summary>
    /// <param name="items">The collection of element-priority pairs to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items" /> is null.</exception>
    public new void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> items)
    {
        lock (this._syncRoot)
        {
            base.EnqueueRange(items);
        }
    }

    /// <summary>
    ///     Adds a collection of elements, all with the same priority, to the queue in a thread-safe manner.
    /// </summary>
    /// <param name="elements">The collection of elements to add.</param>
    /// <param name="priority">The priority to associate with all the elements.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="elements" /> is null.</exception>
    public new void EnqueueRange(IEnumerable<TElement> elements, TPriority priority)
    {
        lock (this._syncRoot)
        {
            base.EnqueueRange(elements, priority);
        }
    }

    /// <summary>
    ///     Returns the element with the highest priority without removing it from the queue in a thread-safe manner.
    /// </summary>
    /// <returns>The element with the highest priority.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
    public new TElement Peek()
    {
        lock (this._syncRoot)
        {
            return base.Peek();
        }
    }

    /// <summary>
    ///     Attempts to remove and return the element with the highest priority from the queue in a thread-safe manner.
    /// </summary>
    /// <param name="element">
    ///     When this method returns, contains the removed element, if the operation was successful;
    ///     otherwise, the default value for the type.
    /// </param>
    /// <param name="priority">
    ///     When this method returns, contains the priority of the removed element, if the operation was
    ///     successful; otherwise, the default value for the type.
    /// </param>
    /// <returns>true if an element was removed and returned successfully; otherwise, false.</returns>
    public new bool TryDequeue(out TElement element, out TPriority priority)
    {
        lock (this._syncRoot)
        {
            return base.TryDequeue(out element, out priority);
        }
    }

    /// <summary>
    ///     Attempts to return the element with the highest priority without removing it from the queue in a thread-safe
    ///     manner.
    /// </summary>
    /// <param name="element">
    ///     When this method returns, contains the element with the highest priority, if the operation was
    ///     successful; otherwise, the default value for the type.
    /// </param>
    /// <param name="priority">
    ///     When this method returns, contains the priority of the element, if the operation was successful;
    ///     otherwise, the default value for the type.
    /// </param>
    /// <returns>true if an element was returned successfully; otherwise, false.</returns>
    public new bool TryPeek(out TElement element, out TPriority priority)
    {
        lock (this._syncRoot)
        {
            return base.TryPeek(out element, out priority);
        }
    }
}
