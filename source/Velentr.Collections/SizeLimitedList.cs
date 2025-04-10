using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Velentr.Collections.CollectionActions;

namespace Velentr.Collections;

/// <summary>
/// A list with a maximum size limit. When the limit is reached, it performs a specified action to handle the overflow.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class SizeLimitedList<T> : IList<T>
{
    [JsonIgnore]
    private readonly List<T> baseList;

    [JsonIgnore] 
    private int maxSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeLimitedList{T}"/> class with a specified maximum size and action when full.
    /// </summary>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize is less than 1.</exception>
    public SizeLimitedList(int maxSize, SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        if (maxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than or equal to 1.");
        }
        
        this.baseList = new List<T>(maxSize);
        this.maxSize = maxSize;
        this.ActionWhenFull = actionWhenFull;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeLimitedList{T}"/> class with a specified starting capacity, maximum size, and action when full.
    /// </summary>
    /// <param name="startingCapacity">The initial capacity of the list.</param>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize or startingCapacity is less than 1.</exception>
    public SizeLimitedList(int startingCapacity, int maxSize, SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        if (maxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than or equal to 1.");
        }

        if (startingCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(startingCapacity), "Starting capacity must be greater than or equal to 1.");
        }
        
        this.baseList = new List<T>(startingCapacity);
        this.maxSize = maxSize;
        this.ActionWhenFull = actionWhenFull;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeLimitedList{T}"/> class with an existing list, maximum size, and action when full.
    /// </summary>
    /// <param name="baseList">The existing list to initialize with.</param>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize is less than 0 or baseList exceeds maxSize.</exception>
    /// <exception cref="ArgumentNullException">Thrown when baseList is null.</exception>
    [JsonConstructor]
    public SizeLimitedList(IList<T> baseList, int maxSize, SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        if (maxSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "New max size must be greater than or equal to 0.");
        }
        if (baseList == null)
        {
            throw new ArgumentNullException(nameof(baseList), "Base list cannot be null.");
        }
        if (baseList.Count > maxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(baseList), "Base list cannot be larger than the max size.");
        }
        
        this.baseList = new List<T>(maxSize);
        for (int i = 0; i < baseList.Count; i++)
        {
            this.baseList.Add(baseList[i]);
        }
        this.ActionWhenFull = actionWhenFull;
        this.maxSize = maxSize;
    }

    /// <summary>
    /// Gets or sets the action to perform when the list exceeds its maximum size.
    /// </summary>
    [JsonPropertyName("actionWhenFull")]
    public SizeLimitedCollectionFullAction ActionWhenFull { get; set; }

    /// <summary>
    /// Gets the maximum size of the list.
    /// </summary>
    [JsonPropertyName("maxSize")] 
    public int MaxSize => this.maxSize;

    [JsonPropertyName("baseList")]
    public ImmutableList<T> UnderlyingList => this.baseList.ToImmutableList();

    /// <summary>
    /// Changes the maximum size of the list and removes excess elements if necessary.
    /// </summary>
    /// <param name="newMaxSize">The new maximum size of the list.</param>
    /// <returns>A list of elements that were removed to fit the new size.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when newMaxSize is less than 1.</exception>
    public List<T> ChangeMaxSize(int newMaxSize)
    {
        if (newMaxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newMaxSize), "New max size must be greater than or equal to 1.");
        }
        if (newMaxSize >= this.baseList.Count)
        {
            this.maxSize = newMaxSize;
            return new();
        }
        
        this.maxSize = newMaxSize;
        List<T> poppedItems = new();
        int remainingItemsToPop = this.baseList.Count - newMaxSize;
        int currentI = 0;
        int incrementor = 0;
        if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopNewestItem)
        {
            currentI = this.baseList.Count - 1;
            incrementor = -1;
        }

        while (remainingItemsToPop > 0)
        {
            poppedItems.Add(this.baseList[currentI]);
            this.baseList.RemoveAt(currentI);
            
            currentI += incrementor;
            remainingItemsToPop--;
        }

        return poppedItems;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the list.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return this.baseList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds an item to the list. If the list is full, performs the specified action to handle the overflow.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the list is full and no valid action is defined.</exception>
    public void Add(T item)
    {
        AddAndReturn(item);
    }

    /// <summary>
    /// Adds an item to the list. If the list is full, performs the specified action to handle the overflow, and returns
    /// any popped items.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was popped from the list, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the list is full and no valid action is defined.</exception>
    public T? AddAndReturn(T item)
    {
        T? poppedItem = default;
        if (this.baseList.Count >= maxSize)
        {
            if (ActionWhenFull == SizeLimitedCollectionFullAction.PopOldestItem)
            {
                poppedItem = this.baseList[0];
                this.baseList.RemoveAt(0);
            }
            else if (ActionWhenFull == SizeLimitedCollectionFullAction.PopNewestItem)
            {
                poppedItem = this.baseList[this.baseList.Count - 1];
                this.baseList.RemoveAt(this.baseList.Count - 1);
            }
            else
            {
                throw new InvalidOperationException("The collection is full and no valid action is defined.");
            }
        }
        this.baseList.Add(item);
        return poppedItem;
    }

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    public void Clear()
    {
        this.baseList.Clear();
    }

    /// <summary>
    /// Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The item to locate in the list.</param>
    /// <returns>True if the item is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        return this.baseList.Contains(item);
    }

    /// <summary>
    /// Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        this.baseList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        return this.baseList.Remove(item);
    }

    /// <summary>
    /// Gets the number of elements contained in the list.
    /// </summary>
    public int Count => this.baseList.Count;

    /// <summary>
    /// Gets a value indicating whether the list is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Determines the index of a specific item in the list.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>The index of the item if found; otherwise, -1.</returns>
    public int IndexOf(T item)
    {
        return this.baseList.IndexOf(item);
    }

    /// <summary>
    /// Inserts an item to the list at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the item should be inserted.</param>
    /// <param name="item">The item to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is greater than the max size.</exception>
    public void Insert(int index, T item)
    {
        if (index > this.maxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be greater than the max size.");
        }
        this.baseList.Insert(index, item);
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        this.baseList.RemoveAt(index);
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public T this[int index]
    {
        get => this.baseList[index];
        set
        {
            if (index < 0 || index >= this.baseList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            this.baseList[index] = value;
        }
    }
}
