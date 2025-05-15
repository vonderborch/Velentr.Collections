using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Velentr.Collections.Internal;

namespace Velentr.Collections.LockFree;

/// <summary>
///     A lock-free implementation of a linked list data structure.
///     This implementation is thread-safe without using locks, providing better scalability in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class LockFreeList<T> : ICollection<T>, ICollection, IEnumerable<T>, IDisposable
{
    [JsonIgnore] private readonly Node<T> head;

    // Dictionary to track marked (logically deleted) nodes
    [JsonIgnore] private readonly ConcurrentDictionary<Node<T>, bool> markedNodes = new();

    [JsonIgnore] private readonly Node<T> tail;

    [JsonIgnore] private int count;

    [JsonIgnore] private long version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeList{T}" /> class.
    /// </summary>
    public LockFreeList()
    {
        this.head = new Node<T>();
        this.tail = new Node<T>();
        this.head.Next = this.tail;
        this.count = 0;
        this.version = 0;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeList{T}" /> class with an initial value.
    /// </summary>
    /// <param name="item">The value to add to the list.</param>
    public LockFreeList(T item)
    {
        this.head = new Node<T>();
        this.tail = new Node<T>();
        this.head.Next = this.tail;
        this.count = 0;
        this.version = 0;

        Add(item);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeList{T}" /> class with an initial collection of values.
    /// </summary>
    /// <param name="collection">The collection of values to add to the list.</param>
    [JsonConstructor]
    public LockFreeList(IEnumerable<T> collection) : this()
    {
        this.head = new Node<T>();
        this.tail = new Node<T>();
        this.head.Next = this.tail;
        this.count = 0;
        this.version = 0;

        foreach (T item in collection)
        {
            Add(item);
        }
    }

    /// <summary>
    ///     Transforms the list into a standard list.
    ///     This method is safe to call even if the collection is modified during execution.
    /// </summary>
    [JsonPropertyName("collection")]
    public List<T> ToList
    {
        get
        {
            var startingVersion = this.version;

            // Create a new list to hold the snapshot
            List<T> list = new();

            // Take a snapshot of the list structure
            Node<T>? curr = this.head.Next;

            // Walk through the nodes without using the enumerator
            while (curr != this.tail)
            {
                CollectionValidators.ValidateCollectionState(startingVersion, this.version);
                // Only include non-marked (not logically deleted) nodes
                if (!IsMarked(curr))
                {
                    list.Add(curr.Value);
                }

                curr = curr.Next;
            }

            return list;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether access to the list is synchronized (thread safe).
    ///     Always returns true for LockFreeList as it's thread-safe by design.
    /// </summary>
    public bool IsSynchronized => true;

    /// <summary>
    ///     Gets an object that can be used to synchronize access to the list.
    ///     Note: This property exists for ICollection compatibility but lock-free collections
    ///     don't require external synchronization.
    /// </summary>
    [field: JsonIgnore]
    public object SyncRoot { get; } = new();

    /// <summary>
    ///     Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
    /// <param name="index">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">index is less than 0.</exception>
    /// <exception cref="ArgumentException">
    ///     array is multidimensional or the number of elements in the source exceeds available
    ///     space.
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

        var i = index;
        foreach (T item in this)
        {
            array.SetValue(item, i++);
        }
    }

    /// <summary>
    ///     Gets the number of elements contained in the list.
    /// </summary>
    public int Count => this.count;

    /// <summary>
    ///     Gets a value indicating whether the list is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Adds an item to the end of the list.
    /// </summary>
    /// <param name="item">The item to add to the list.</param>
    public void Add(T item)
    {
        Node<T> newNode = new(item);

        while (true)
        {
            // Find the last non-marked node
            Node<T> pred = this.head;
            Node<T>? curr = this.head.Next;

            while (curr != this.tail)
            {
                if (!IsMarked(curr))
                {
                    pred = curr;
                }

                curr = curr.Next;
            }

            // Try to append the new node
            newNode.Next = this.tail;
            if (Interlocked.CompareExchange(ref pred.Next, newNode, this.tail) == this.tail)
            {
                // Success
                Interlocked.Increment(ref this.count);
                Interlocked.Increment(ref this.version);
                return;
            }

            // Failed, retry
        }
    }

    /// <summary>
    ///     Removes all items from the list.
    /// </summary>
    public void Clear()
    {
        // Clean up nodes between head and tail for proper disposal
        Node<T>? current = this.head.Next;
        while (current != this.tail)
        {
            Node<T>? next = current.Next;
            current.Dispose();
            current = next;
        }

        this.head.Next = this.tail;
        this.markedNodes.Clear();
        Interlocked.Exchange(ref this.count, 0);
        Interlocked.Increment(ref this.version);
    }

    /// <summary>
    ///     Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the list.</param>
    /// <returns>true if item is found in the list; otherwise, false.</returns>
    public bool Contains(T item)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        Node<T>? curr = this.head.Next;

        while (curr != this.tail)
        {
            if (!IsMarked(curr) &&
                ((curr.Value == null && item == null) ||
                 (curr.Value != null && comparer.Equals(curr.Value, item))))
            {
                return true;
            }

            curr = curr.Next;
        }

        return false;
    }

    /// <summary>
    ///     Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
    /// <exception cref="ArgumentException">
    ///     The number of elements in the source exceeds the available space in the destination
    ///     array.
    /// </exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index must be non-negative");
        }

        if (array.Length - arrayIndex < this.count)
        {
            throw new ArgumentException("Not enough space in array from index to end of destination array");
        }

        var index = arrayIndex;
        foreach (T item in this)
        {
            array[index++] = item;
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the list.
    /// </summary>
    /// <returns>An enumerator for the list.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the list is modified during enumeration.</exception>
    public IEnumerator<T> GetEnumerator()
    {
        var startingVersion = this.version;
        Node<T>? curr = this.head.Next;

        while (curr != this.tail)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            // Only return non-marked (not logically deleted) nodes
            if (!IsMarked(curr))
            {
                yield return curr.Value;
            }

            curr = curr.Next;
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the list.
    /// </summary>
    /// <returns>An enumerator for the list.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Removes the first occurrence of a specific object from the list.
    /// </summary>
    /// <param name="item">The object to remove from the list.</param>
    /// <returns>true if item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        Node<T> pred, curr;

        while (true)
        {
            // Find the node to remove
            pred = this.head;
            curr = this.head.Next;

            while (curr != this.tail)
            {
                // If node is not marked and matches the value
                if (!IsMarked(curr) &&
                    ((curr.Value == null && item == null) ||
                     (curr.Value != null && comparer.Equals(curr.Value, item))))
                {
                    // Try to mark the node for deletion
                    if (Mark(curr))
                    {
                        // Physical removal by bypassing the node
                        Node<T>? next = curr.Next;
                        Interlocked.CompareExchange(ref pred.Next, next, curr);

                        // Dispose the node
                        curr.Dispose();

                        Interlocked.Decrement(ref this.count);
                        Interlocked.Increment(ref this.version);
                        return true;
                    }

                    // If we failed to mark, retry
                    break;
                }

                if (!IsMarked(curr))
                {
                    pred = curr;
                }

                curr = curr.Next;
            }

            // Item not found
            if (curr == this.tail)
            {
                return false;
            }
        }
    }

    /// <summary>
    ///     Releases all resources used by the list.
    /// </summary>
    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Adds a range of elements to the lock-free list.
    /// </summary>
    /// <param name="range">The collection of elements to add to the list.</param>
    public void AddRange(IEnumerable<T> range)
    {
        foreach (T item in range)
        {
            Add(item);
        }
    }

    /// <summary>
    ///     Gets the first element in the list.
    /// </summary>
    /// <returns>The first element in the list, or default if the list is empty.</returns>
    public T? First()
    {
        Node<T>? curr = this.head.Next;

        while (curr != this.tail)
        {
            if (!IsMarked(curr))
            {
                return curr.Value;
            }

            curr = curr.Next;
        }

        return default;
    }

    /// <summary>
    ///     Checks if a node is marked as logically deleted.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>true if the node is marked; otherwise, false.</returns>
    private bool IsMarked(Node<T> node)
    {
        return this.markedNodes.ContainsKey(node);
    }

    /// <summary>
    ///     Gets the last element in the list.
    /// </summary>
    /// <returns>The last element in the list, or default if the list is empty.</returns>
    public T? Last()
    {
        T? last = default;
        Node<T>? curr = this.head.Next;

        while (curr != this.tail)
        {
            if (!IsMarked(curr))
            {
                last = curr.Value;
            }

            curr = curr.Next;
        }

        return last;
    }

    /// <summary>
    ///     Marks a node as logically deleted.
    /// </summary>
    /// <param name="node">The node to mark.</param>
    /// <returns>true if the node was successfully marked; otherwise, false.</returns>
    private bool Mark(Node<T> node)
    {
        return this.markedNodes.TryAdd(node, true);
    }

    /// <summary>
    ///     Tries to get the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <param name="value">
    ///     When this method returns, contains the element at the specified index, or the default value if the
    ///     index is out of range.
    /// </param>
    /// <returns>true if the element was successfully retrieved; otherwise, false.</returns>
    public bool TryGetAt(int index, out T? value)
    {
        if (index < 0)
        {
            value = default;
            return false;
        }

        var currentIndex = 0;
        Node<T>? curr = this.head.Next;

        while (curr != this.tail)
        {
            if (!IsMarked(curr))
            {
                if (currentIndex == index)
                {
                    value = curr.Value;
                    return true;
                }

                currentIndex++;
            }

            curr = curr.Next;
        }

        value = default;
        return false;
    }
}
