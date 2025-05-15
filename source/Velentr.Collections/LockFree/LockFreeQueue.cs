using System.Collections;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Velentr.Collections.Internal;

namespace Velentr.Collections.LockFree;

/// <summary>
///     A lock-free implementation of a queue data structure.
///     This implementation is thread-safe without using locks, providing better scalability in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class LockFreeQueue<T> : ICollection, IEnumerable<T>
{
    [JsonIgnore] private int count;

    [JsonIgnore] private Node<T> head;

    [JsonIgnore] private Node<T> tail;

    [JsonIgnore] private long version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeQueue{T}" /> class.
    /// </summary>
    public LockFreeQueue()
    {
        this.version = 0;
        this.head = new Node<T>();
        this.tail = this.head;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeQueue{T}" /> class with an initial value.
    /// </summary>
    /// <param name="value">The value to add to the queue.</param>
    public LockFreeQueue(T value)
    {
        this.version = 0;
        this.head = new Node<T>();
        this.tail = this.head;
        Enqueue(value);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeQueue{T}" /> class with an initial collection of values.
    /// </summary>
    /// <param name="collection">The collection of values to add to the queue.</param>
    [JsonConstructor]
    public LockFreeQueue(IEnumerable<T> collection)
    {
        this.version = 0;
        this.head = new Node<T>();
        this.tail = this.head;
        foreach (T item in collection)
        {
            Enqueue(item);
        }
    }

    /// <summary>
    ///     Transforms the queue into a list.
    /// </summary>
    [JsonPropertyName("collection")]
    public List<T> ToList
    {
        get
        {
            var startingVersion = this.version;
            List<T> list = new();
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
    ///     Always returns true for LockFreeQueue as it's thread-safe by design.
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

        var i = index;
        foreach (T item in this)
        {
            array.SetValue(item, i++);
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
    ///     Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the queue is modified during enumeration.</exception>
    public IEnumerator<T> GetEnumerator()
    {
        var startingVersion = this.version;
        Node<T>? current = this.head.Next;

        while (current != null)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);
            yield return current.Value;
            current = current.Next;
        }
    }

    /// <summary>
    ///     Removes all elements from the queue.
    /// </summary>
    public void Clear()
    {
        this.head = new Node<T>();
        this.tail = this.head;
        Interlocked.Exchange(ref this.count, 0);
        Interlocked.Exchange(ref this.version, 0);
    }

    /// <summary>
    ///     Removes and returns the element at the front of the queue.
    /// </summary>
    /// <returns>The element at the front of the queue, or default value if the queue is empty.</returns>
    public T? Dequeue()
    {
        Dequeue(out T? result);
        return result;
    }

    /// <summary>
    ///     Tries to remove and return the element at the front of the queue.
    /// </summary>
    /// <param name="value">
    ///     When this method returns, contains the element at the front of the queue, or the default value if
    ///     the queue is empty.
    /// </param>
    /// <returns>true if an element was removed; otherwise, false.</returns>
    public bool Dequeue(out T? value)
    {
        T? result = default;
        var updated = false;
        Node<T> previousHead;
        Node<T> previousTail;
        Node<T>? previousNextHead;

        do
        {
            previousHead = this.head;
            previousTail = this.tail;
            previousNextHead = previousHead.Next;

            if (previousHead == this.head)
            {
                if (previousHead == previousTail)
                {
                    if (previousNextHead == null)
                    {
                        value = default;
                        return false;
                    }

                    Interlocked.CompareExchange(ref this.tail, previousNextHead, previousTail);
                }
                else
                {
                    result = previousNextHead.Value;
                    updated = Interlocked.CompareExchange(ref this.head, previousNextHead, previousHead) ==
                              previousHead;
                }
            }
        } while (!updated);

        Interlocked.Decrement(ref this.count);
        Interlocked.Increment(ref this.version);
        value = result;
        return true;
    }

    /// <summary>
    ///     Adds an element to the end of the queue.
    /// </summary>
    /// <param name="value">The element to add to the queue.</param>
    public void Enqueue(T value)
    {
        Node<T>? previousTail;
        Node<T>? previousNext;
        Node<T> newNode = new(value);

        var updated = false;
        do
        {
            previousTail = this.tail;
            previousNext = previousTail.Next;

            if (this.tail == previousTail)
            {
                if (previousNext == null)
                {
                    // Attempt to link the new node to the end of the queue
                    updated = Interlocked.CompareExchange(ref previousTail.Next, newNode, null) == null;
                }
                else
                {
                    // Tail is not at the end of the queue, move it forward
                    Interlocked.CompareExchange(ref this.tail, previousNext, previousTail);
                }
            }
        } while (!updated);

        Interlocked.Increment(ref this.version);
        Interlocked.Increment(ref this.count);
    }

    /// <summary>
    ///     Gets the element at the front of the queue without removing it.
    /// </summary>
    /// <returns>The element at the front of the queue, or default value if the queue is empty.</returns>
    public T? Peek()
    {
        if (this.head.Next == null)
        {
            return default;
        }

        return this.head.Next.Value;
    }
}
