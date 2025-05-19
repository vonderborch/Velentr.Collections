using System.Collections;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Velentr.Collections.Internal;

namespace Velentr.Collections.LockFree;

/// <summary>
///     A lock-free implementation of a stack data structure.
///     This implementation is thread-safe without using locks, providing better scalability in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">The type of elements in the stack</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class LockFreeStack<T> : ICollection, IEnumerable<T>
{
    [JsonIgnore] private readonly Node<T> head;
    [JsonIgnore] private int count;

    [JsonIgnore] private long version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeStack{T}" /> class.
    /// </summary>
    public LockFreeStack()
    {
        this.version = 0;
        this.count = 0;
        this.head = new Node<T>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeStack{T}" /> class with an initial value.
    /// </summary>
    /// <param name="value">The value to add to the stack.</param>
    public LockFreeStack(T value)
    {
        this.version = 0;
        this.count = 0;
        this.head = new Node<T>();
        Push(value);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LockFreeStack{T}" /> class with an initial value.
    /// </summary>
    /// <param name="collection">The values to add to the stack.</param>
    [JsonConstructor]
    public LockFreeStack(IEnumerable<T> collection)
    {
        this.version = 0;
        this.count = 0;
        this.head = new Node<T>();
        foreach (T item in collection)
        {
            Push(item);
        }
    }

    /// <summary>
    ///     Transforms the stack into a list.
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
    ///     Gets the number of elements contained in the stack.
    /// </summary>
    public int Count => this.count;

    /// <summary>
    ///     Gets a value indicating whether access to the stack is synchronized (thread safe).
    ///     Always returns true for LockFreeStack as it's thread-safe by design.
    /// </summary>
    public bool IsSynchronized => true;

    /// <summary>
    ///     Gets an object that can be used to synchronize access to the stack.
    ///     Note: This property exists for ICollection compatibility but lock-free collections
    ///     don't require external synchronization.
    /// </summary>
    [field: JsonIgnore]
    public object SyncRoot { get; } = new();

    /// <summary>
    ///     Copies the elements of the stack to an array, starting at a particular index.
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
    ///     Returns an enumerator that iterates through the stack.
    /// </summary>
    /// <returns>An enumerator for the stack.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the stack.
    /// </summary>
    /// <returns>An enumerator for the stack.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the stack is modified during enumeration.</exception>
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
    ///     Clears all elements from the stack.
    /// </summary>
    public void Clear()
    {
        this.head.Next = null;
        Interlocked.Increment(ref this.version);
        Interlocked.Exchange(ref this.count, 0);
    }

    /// <summary>
    ///     Determines whether the stack contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the stack.</param>
    /// <returns>true if item is found in the stack; otherwise, false.</returns>
    public bool Contains(T item)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        Node<T> current = this.head.Next;

        while (current != null)
        {
            if (item == null)
            {
                if (current.Value == null)
                {
                    return true;
                }
            }
            else if (current.Value != null && comparer.Equals(current.Value, item))
            {
                return true;
            }

            current = current.Next;
        }

        return false;
    }

    /// <summary>
    ///     Gets the element at the top of the stack without removing it.
    /// </summary>
    /// <returns>The element at the top of the stack, or default value if the stack is empty.</returns>
    public T? Peek()
    {
        return this.head.Next == null ? default : this.head.Next.Value;
    }

    /// <summary>
    ///     Removes and returns the element at the top of the stack.
    /// </summary>
    /// <returns>The element at the top of the stack, or default value if the stack is empty.</returns>
    public T? Pop()
    {
        Pop(out T? value);
        return value;
    }

    /// <summary>
    ///     Tries to remove and return the element at the top of the stack.
    /// </summary>
    /// <param name="value">
    ///     When this method returns, contains the element at the top of the stack, or the default value if the
    ///     stack is empty.
    /// </param>
    /// <returns>true if an element was removed; otherwise, false.</returns>
    public bool Pop(out T value)
    {
        Node<T>? node;
        do
        {
            node = this.head.Next;
            if (node == null)
            {
                value = default;
                return false;
            }
        } while (!Helpers.CompareAndSwap(ref this.head.Next, node.Next, this.head.Next));

        value = node.Value;
        Interlocked.Increment(ref this.version);
        Helpers.Decrement(ref this.count, 0);
        return true;
    }

    /// <summary>
    ///     Inserts an element at the top of the stack.
    /// </summary>
    /// <param name="value">The element to push onto the stack.</param>
    public void Push(T value)
    {
        Node<T> newNode = new(value);
        do
        {
            newNode.Next = this.head.Next;
        } while (!Helpers.CompareAndSwap(ref this.head.Next, newNode, this.head.Next));

        Interlocked.Increment(ref this.version);
        Interlocked.Increment(ref this.count);
    }
}
