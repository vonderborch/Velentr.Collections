using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Velentr.Collections.CollectionFullActions;

namespace Velentr.Collections;

/// <summary>
///     A list with a maximum size limit. When the limit is reached, it performs a specified action to handle the overflow.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class SizeLimitedList<T> : IList<T>
{
    [JsonIgnore] private readonly ReaderWriterLockSlim _lock = new();
    [JsonIgnore] private readonly object _syncLock = new();
    [JsonIgnore] private readonly List<T> internalList;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SizeLimitedList{T}" /> class with a specified maximum size and action
    ///     when full.
    /// </summary>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    /// <param name="isThreadSafe">If true, all operations will be synchronized for thread safety.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize is less than 1.</exception>
    public SizeLimitedList(int maxSize,
        SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem,
        bool isThreadSafe = false)
    {
        if (maxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than or equal to 1.");
        }

        this.internalList = new List<T>(maxSize);
        this.MaxSize = maxSize;
        this.ActionWhenFull = actionWhenFull;
        this.IsThreadSafe = isThreadSafe;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SizeLimitedList{T}" /> class with a specified starting capacity,
    ///     maximum size, and action when full.
    /// </summary>
    /// <param name="startingCapacity">The initial capacity of the list.</param>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    /// <param name="isThreadSafe">If true, all operations will be synchronized for thread safety.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize or startingCapacity is less than 1.</exception>
    public SizeLimitedList(int startingCapacity, int maxSize,
        SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem,
        bool isThreadSafe = false)
    {
        if (maxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than or equal to 1.");
        }

        if (startingCapacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(startingCapacity),
                "Starting capacity must be greater than or equal to 1.");
        }

        this.internalList = new List<T>(startingCapacity);
        this.MaxSize = maxSize;
        this.ActionWhenFull = actionWhenFull;
        this.IsThreadSafe = isThreadSafe;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SizeLimitedList{T}" /> class with an existing list, maximum size, and
    ///     action when full.
    /// </summary>
    /// <param name="baseList">The existing list to initialize with.</param>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    /// <param name="isThreadSafe">If true, all operations will be synchronized for thread safety.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize is less than 0 or internalList exceeds maxSize.</exception>
    /// <exception cref="ArgumentNullException">Thrown when internalList is null.</exception>
    [JsonConstructor]
    public SizeLimitedList(IList<T> baseList, int maxSize,
        SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem,
        bool isThreadSafe = false)
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

        this.internalList = new List<T>(maxSize);
        for (var i = 0; i < baseList.Count; i++)
        {
            this.internalList.Add(baseList[i]);
        }

        this.ActionWhenFull = actionWhenFull;
        this.MaxSize = maxSize;
        this.IsThreadSafe = isThreadSafe;
    }

    [JsonPropertyName("internalList")]
    public ImmutableList<T> UnderlyingList
    {
        get
        {
            if (this.IsThreadSafe)
            {
                this._lock.EnterReadLock();
                try
                {
                    return this.internalList.ToImmutableList();
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }

            return this.internalList.ToImmutableList();
        }
    }

    /// <summary>
    ///     Gets or sets the action to perform when the list exceeds its maximum size.
    /// </summary>
    [JsonPropertyName("actionWhenFull")]
    public SizeLimitedCollectionFullAction ActionWhenFull { get; set; }

    /// <summary>
    ///     Gets or sets whether operations on this list are thread-safe.
    /// </summary>
    [field: JsonIgnore]
    public bool IsThreadSafe { get; set; }

    /// <summary>
    ///     Gets the maximum size of the list.
    /// </summary>
    [JsonPropertyName("maxSize")]
    [field: JsonIgnore]
    public int MaxSize { get; private set; }

    /// <summary>
    ///     Gets the number of elements contained in the list.
    /// </summary>
    public int Count
    {
        get
        {
            if (this.IsThreadSafe)
            {
                this._lock.EnterReadLock();
                try
                {
                    return this.internalList.Count;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }

            return this.internalList.Count;
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the list is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Adds an item to the list. If the list is full, performs the specified action to handle the overflow.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the list is full and no valid action is defined.</exception>
    public void Add(T item)
    {
        AddAndReturn(item);
    }

    /// <summary>
    ///     Removes all items from the list.
    /// </summary>
    public void Clear()
    {
        if (this.IsThreadSafe)
        {
            this._lock.EnterWriteLock();
            try
            {
                this.internalList.Clear();
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }
        else
        {
            this.internalList.Clear();
        }
    }

    /// <summary>
    ///     Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The item to locate in the list.</param>
    /// <returns>True if the item is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        if (this.IsThreadSafe)
        {
            this._lock.EnterReadLock();
            try
            {
                return this.internalList.Contains(item);
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        return this.internalList.Contains(item);
    }

    /// <summary>
    ///     Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (this.IsThreadSafe)
        {
            this._lock.EnterReadLock();
            try
            {
                this.internalList.CopyTo(array, arrayIndex);
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }
        else
        {
            this.internalList.CopyTo(array, arrayIndex);
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the list.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        // Return a snapshot to avoid enumeration during modification exceptions
        if (this.IsThreadSafe)
        {
            this._lock.EnterReadLock();
            try
            {
                return new List<T>(this.internalList).GetEnumerator();
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        return this.internalList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Determines the index of a specific item in the list.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>The index of the item if found; otherwise, -1.</returns>
    public int IndexOf(T item)
    {
        if (this.IsThreadSafe)
        {
            this._lock.EnterReadLock();
            try
            {
                return this.internalList.IndexOf(item);
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        return this.internalList.IndexOf(item);
    }

    /// <summary>
    ///     Inserts an item to the list at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the item should be inserted.</param>
    /// <param name="item">The item to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is greater than the max size.</exception>
    public void Insert(int index, T item)
    {
        if (index > this.MaxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be greater than the max size.");
        }

        if (this.IsThreadSafe)
        {
            this._lock.EnterWriteLock();
            try
            {
                this.internalList.Insert(index, item);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }
        else
        {
            this.internalList.Insert(index, item);
        }
    }

    /// <summary>
    ///     Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public T this[int index]
    {
        get
        {
            if (this.IsThreadSafe)
            {
                this._lock.EnterReadLock();
                try
                {
                    return this.internalList[index];
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }

            return this.internalList[index];
        }
        set
        {
            if (this.IsThreadSafe)
            {
                this._lock.EnterWriteLock();
                try
                {
                    if (index < 0 || index >= this.internalList.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                    }

                    this.internalList[index] = value;
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
            else
            {
                if (index < 0 || index >= this.internalList.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }

                this.internalList[index] = value;
            }
        }
    }

    /// <summary>
    ///     Removes the first occurrence of a specific object from the list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        if (this.IsThreadSafe)
        {
            this._lock.EnterWriteLock();
            try
            {
                return this.internalList.Remove(item);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        return this.internalList.Remove(item);
    }

    /// <summary>
    ///     Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        if (this.IsThreadSafe)
        {
            this._lock.EnterWriteLock();
            try
            {
                this.internalList.RemoveAt(index);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }
        else
        {
            this.internalList.RemoveAt(index);
        }
    }

    /// <summary>
    ///     Adds an item to the list. If the list is full, performs the specified action to handle the overflow, and returns
    ///     any popped items.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was popped from the list, if any.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the list is full and no valid action is defined.</exception>
    public T? AddAndReturn(T item)
    {
        if (this.IsThreadSafe)
        {
            lock (this._syncLock)
            {
                return AddAndReturnInternal(item);
            }
        }

        return AddAndReturnInternal(item);
    }

    private T? AddAndReturnInternal(T item)
    {
        T? poppedItem = default;
        if (this.internalList.Count >= this.MaxSize)
        {
            if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopOldestItem)
            {
                poppedItem = this.internalList[0];
                this.internalList.RemoveAt(0);
            }
            else if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopNewestItem)
            {
                poppedItem = this.internalList[this.internalList.Count - 1];
                this.internalList.RemoveAt(this.internalList.Count - 1);
            }
            else
            {
                throw new InvalidOperationException("The collection is full and no valid action is defined.");
            }
        }

        this.internalList.Add(item);
        return poppedItem;
    }

    /// <summary>
    ///     Returns a thread-safe snapshot of the list.
    /// </summary>
    /// <returns>An immutable copy of the current list state</returns>
    public ImmutableList<T> AsImmutable()
    {
        if (this.IsThreadSafe)
        {
            lock (this._syncLock)
            {
                return this.internalList.ToImmutableList();
            }
        }

        return this.internalList.ToImmutableList();
    }

    /// <summary>
    ///     Changes the maximum size of the list and removes excess elements if necessary.
    /// </summary>
    /// <param name="newMaxSize">The new maximum size of the list.</param>
    /// <returns>A list of elements that were removed to fit the new size.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when newMaxSize is less than 1.</exception>
    public List<T> ChangeMaxSize(int newMaxSize)
    {
        if (this.IsThreadSafe)
        {
            lock (this._syncLock)
            {
                return ChangeMaxSizeInternal(newMaxSize);
            }
        }

        return ChangeMaxSizeInternal(newMaxSize);
    }

    private List<T> ChangeMaxSizeInternal(int newMaxSize)
    {
        if (newMaxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newMaxSize),
                "New max size must be greater than or equal to 1.");
        }

        if (newMaxSize >= this.internalList.Count)
        {
            this.MaxSize = newMaxSize;
            return new List<T>();
        }

        this.MaxSize = newMaxSize;
        List<T> poppedItems = new();
        var remainingItemsToPop = this.internalList.Count - newMaxSize;

        if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopNewestItem)
        {
            poppedItems.AddRange(this.internalList.GetRange(newMaxSize, remainingItemsToPop));
            this.internalList.RemoveRange(newMaxSize, remainingItemsToPop);
        }
        else
        {
            poppedItems.AddRange(this.internalList.GetRange(0, remainingItemsToPop));
            this.internalList.RemoveRange(0, remainingItemsToPop);
        }

        return poppedItems;
    }
}
