using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Velentr.Collections.Internal;

namespace Velentr.Collections.LockFree;

/// <summary>
/// A lock-free implementation of a linked list data structure.
/// This implementation is thread-safe without using locks, providing better scalability in multi-threaded scenarios.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class LockFreeList<T> : ICollection<T>, ICollection, IEnumerable<T>, IDisposable
{
    [JsonIgnore]
    private readonly Node<T> head;

    [JsonIgnore]
    private readonly Node<T> tail;

    [JsonIgnore]
    private int count;

    [JsonIgnore]
    private ulong version;

    [JsonIgnore]
    private readonly object syncRoot = new object();

    // Dictionary to track marked (logically deleted) nodes
    [JsonIgnore]
    private readonly ConcurrentDictionary<Node<T>, bool> markedNodes = new ConcurrentDictionary<Node<T>, bool>();

    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreeList{T}"/> class.
    /// </summary>
    public LockFreeList()
    {
        head = new Node<T>();
        tail = new Node<T>();
        head.Next = tail;
        count = 0;
        version = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreeList{T}"/> class with an initial value.
    /// </summary>
    /// <param name="value">The value to add to the list.</param>
    public LockFreeList(T value) : this()
    {
        Add(value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreeList{T}"/> class with an initial collection of values.
    /// </summary>
    /// <param name="collection">The collection of values to add to the list.</param>
    [JsonConstructor]
    public LockFreeList(IEnumerable<T> collection) : this()
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Gets the number of elements contained in the list.
    /// </summary>
    public int Count => count;

    /// <summary>
    /// Gets a value indicating whether the list is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets a value indicating whether access to the list is synchronized (thread safe).
    /// Always returns true for LockFreeList as it's thread-safe by design.
    /// </summary>
    public bool IsSynchronized => true;

    /// <summary>
    /// Gets an object that can be used to synchronize access to the list.
    /// Note: This property exists for ICollection compatibility but lock-free collections
    /// don't require external synchronization.
    /// </summary>
    public object SyncRoot => syncRoot;

    /// <summary>
    /// Transforms the list into a standard list.
    /// This method is safe to call even if the collection is modified during execution.
    /// </summary>
    [JsonPropertyName("collection")]
    public List<T> ToList
    {
        get
        {
            // Create a new list to hold the snapshot
            var list = new List<T>();
            
            // Take a snapshot of the list structure
            var curr = this.head.Next;
            
            // Walk through the nodes without using the enumerator
            while (curr != this.tail)
            {
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
    /// Adds an item to the end of the list.
    /// </summary>
    /// <param name="item">The item to add to the list.</param>
    public void Add(T item)
    {
        var newNode = new Node<T>(item);
        
        while (true)
        {
            // Find the last non-marked node
            var pred = head;
            var curr = head.Next;
            
            while (curr != tail)
            {
                if (!IsMarked(curr))
                {
                    pred = curr;
                }
                curr = curr.Next;
            }
            
            // Try to append the new node
            newNode.Next = tail;
            if (Interlocked.CompareExchange(ref pred.Next, newNode, tail) == tail)
            {
                // Success
                Interlocked.Increment(ref count);
                Interlocked.Increment(ref version);
                return;
            }
            
            // Failed, retry
        }
    }

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    public void Clear()
    {
        // Clean up nodes between head and tail for proper disposal
        var current = head.Next;
        while (current != tail)
        {
            var next = current.Next;
            current.Dispose();
            current = next;
        }

        head.Next = tail;
        markedNodes.Clear();
        Interlocked.Exchange(ref count, 0);
        Interlocked.Increment(ref version);
    }

    /// <summary>
    /// Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the list.</param>
    /// <returns>true if item is found in the list; otherwise, false.</returns>
    public bool Contains(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        var curr = head.Next;
        
        while (curr != tail)
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
    /// Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
    /// <exception cref="ArgumentException">The number of elements in the source exceeds the available space in the destination array.</exception>
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
        
        if (array.Length - arrayIndex < count)
        {
            throw new ArgumentException("Not enough space in array from index to end of destination array");
        }
        
        int index = arrayIndex;
        foreach (var item in this)
        {
            array[index++] = item;
        }
    }

    /// <summary>
    /// Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements.</param>
    /// <param name="index">The zero-based index in array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">index is less than 0.</exception>
    /// <exception cref="ArgumentException">array is multidimensional or the number of elements in the source exceeds available space.</exception>
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
        
        int i = index;
        foreach (T item in this)
        {
            array.SetValue(item, i++);
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the list.
    /// </summary>
    /// <returns>An enumerator for the list.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the list is modified during enumeration.</exception>
    public IEnumerator<T> GetEnumerator()
    {
        var enumVersion = this.version;
        var curr = this.head.Next;
        
        while (curr != this.tail)
        {
            if (enumVersion != this.version)
            {
                throw new InvalidOperationException("Collection was modified during enumeration.");
            }
            
            // Only return non-marked (not logically deleted) nodes
            if (!IsMarked(curr))
            {
                yield return curr.Value;
            }
            
            curr = curr.Next;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the list.
    /// </summary>
    /// <returns>An enumerator for the list.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the list.
    /// </summary>
    /// <param name="item">The object to remove from the list.</param>
    /// <returns>true if item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        Node<T> pred, curr;
        
        while (true)
        {
            // Find the node to remove
            pred = head;
            curr = head.Next;
            
            while (curr != tail)
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
                        var next = curr.Next;
                        Interlocked.CompareExchange(ref pred.Next, next, curr);
                        
                        // Dispose the node
                        curr.Dispose();
                        
                        Interlocked.Decrement(ref count);
                        Interlocked.Increment(ref version);
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
            if (curr == tail)
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// Gets the first element in the list.
    /// </summary>
    /// <returns>The first element in the list, or default if the list is empty.</returns>
    public T? First()
    {
        var curr = head.Next;
        
        while (curr != tail)
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
    /// Gets the last element in the list.
    /// </summary>
    /// <returns>The last element in the list, or default if the list is empty.</returns>
    public T? Last()
    {
        var last = default(T);
        var curr = head.Next;
        
        while (curr != tail)
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
    /// Tries to get the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <param name="value">When this method returns, contains the element at the specified index, or the default value if the index is out of range.</param>
    /// <returns>true if the element was successfully retrieved; otherwise, false.</returns>
    public bool TryGetAt(int index, out T? value)
    {
        if (index < 0)
        {
            value = default;
            return false;
        }
        
        int currentIndex = 0;
        var curr = head.Next;
        
        while (curr != tail)
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
    
    /// <summary>
    /// Marks a node as logically deleted.
    /// </summary>
    /// <param name="node">The node to mark.</param>
    /// <returns>true if the node was successfully marked; otherwise, false.</returns>
    private bool Mark(Node<T> node)
    {
        return markedNodes.TryAdd(node, true);
    }
    
    /// <summary>
    /// Checks if a node is marked as logically deleted.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>true if the node is marked; otherwise, false.</returns>
    private bool IsMarked(Node<T> node)
    {
        return markedNodes.ContainsKey(node);
    }
    
    /// <summary>
    /// Releases all resources used by the list.
    /// </summary>
    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }
}
