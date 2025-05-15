using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using Velentr.Collections.CollectionFullActions;

namespace Velentr.Collections.Concurrent;

/// <summary>
///     A thread-safe list with a maximum size limit. When the limit is reached, it performs a specified action to handle
///     the overflow.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class ConcurrentSizeLimitedList<T> : IList<T>
{
    /// <summary>
    ///     The underlying size-limited list that handles all operations
    /// </summary>
    private readonly SizeLimitedList<T> _internalList;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentSizeLimitedList{T}" /> class with a specified maximum size
    ///     and action when full.
    /// </summary>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    public ConcurrentSizeLimitedList(int maxSize,
        SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        this._internalList = new SizeLimitedList<T>(maxSize, actionWhenFull, true);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentSizeLimitedList{T}" /> class with a specified starting
    ///     capacity,
    ///     maximum size, and action when full.
    /// </summary>
    /// <param name="startingCapacity">The initial capacity of the list.</param>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    public ConcurrentSizeLimitedList(int startingCapacity, int maxSize,
        SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        this._internalList = new SizeLimitedList<T>(startingCapacity, maxSize, actionWhenFull, true);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConcurrentSizeLimitedList{T}" /> class with an existing list,
    ///     maximum size, and action when full.
    /// </summary>
    /// <param name="baseList">The existing list to initialize with.</param>
    /// <param name="maxSize">The maximum size of the list.</param>
    /// <param name="actionWhenFull">The action to perform when the list exceeds its maximum size.</param>
    public ConcurrentSizeLimitedList(IList<T> baseList, int maxSize,
        SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        this._internalList = new SizeLimitedList<T>(baseList, maxSize, actionWhenFull, true);
    }

    /// <summary>
    ///     Gets the maximum size of the list.
    /// </summary>
    public int MaxSize => this._internalList.MaxSize;

    /// <summary>
    ///     Gets or sets the action to perform when the list exceeds its maximum size.
    /// </summary>
    public SizeLimitedCollectionFullAction ActionWhenFull
    {
        get => this._internalList.ActionWhenFull;
        set => this._internalList.ActionWhenFull = value;
    }

    /// <summary>
    ///     Gets the number of elements contained in the list.
    /// </summary>
    public int Count => this._internalList.Count;

    /// <summary>
    ///     Gets a value indicating whether the list is read-only.
    /// </summary>
    public bool IsReadOnly => this._internalList.IsReadOnly;

    /// <summary>
    ///     Adds an item to the list. If the list is full, performs the specified action to handle the overflow.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        this._internalList.Add(item);
    }

    /// <summary>
    ///     Removes all items from the list.
    /// </summary>
    public void Clear()
    {
        this._internalList.Clear();
    }

    /// <summary>
    ///     Determines whether the list contains a specific value.
    /// </summary>
    /// <param name="item">The item to locate in the list.</param>
    /// <returns>True if the item is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        return this._internalList.Contains(item);
    }

    /// <summary>
    ///     Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        this._internalList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the list.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return this._internalList.GetEnumerator();
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the list.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this._internalList.GetEnumerator();
    }

    /// <summary>
    ///     Determines the index of a specific item in the list.
    /// </summary>
    /// <param name="item">The item to locate.</param>
    /// <returns>The index of the item if found; otherwise, -1.</returns>
    public int IndexOf(T item)
    {
        return this._internalList.IndexOf(item);
    }

    /// <summary>
    ///     Inserts an item to the list at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the item should be inserted.</param>
    /// <param name="item">The item to insert.</param>
    public void Insert(int index, T item)
    {
        this._internalList.Insert(index, item);
    }

    /// <summary>
    ///     Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    public T this[int index]
    {
        get => this._internalList[index];
        set => this._internalList[index] = value;
    }

    /// <summary>
    ///     Removes the first occurrence of a specific object from the list.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        return this._internalList.Remove(item);
    }

    /// <summary>
    ///     Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    public void RemoveAt(int index)
    {
        this._internalList.RemoveAt(index);
    }

    /// <summary>
    ///     Adds an item to the list. If the list is full, performs the specified action to handle the overflow, and returns
    ///     any popped items.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was popped from the list, if any.</returns>
    public T? AddAndReturn(T item)
    {
        return this._internalList.AddAndReturn(item);
    }

    /// <summary>
    ///     Returns an immutable snapshot of the list.
    /// </summary>
    /// <returns>An immutable copy of the current list state</returns>
    public ImmutableList<T> AsImmutable()
    {
        return this._internalList.AsImmutable();
    }

    /// <summary>
    ///     Changes the maximum size of the list and removes excess elements if necessary.
    /// </summary>
    /// <param name="newMaxSize">The new maximum size of the list.</param>
    /// <returns>A list of elements that were removed to fit the new size.</returns>
    public List<T> ChangeMaxSize(int newMaxSize)
    {
        return this._internalList.ChangeMaxSize(newMaxSize);
    }
}
