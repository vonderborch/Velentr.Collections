using System.Collections;
using System.Diagnostics;
using Velentr.Collections.CollectionActions;
using Velentr.Collections.PoolHelpers;

namespace Velentr.Collections;

/// <summary>
/// Represents a pool collection that manages a fixed number of reusable objects.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class Pool<T> : ICollection<T>, IDisposable
{
    private const bool IS_FREE = true;
    
    private const bool IS_USED = !IS_FREE;

    private int actionIndex;

    private readonly List<InternalPoolEntry<T>> internalStructure;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pool{T}"/> class.
    /// </summary>
    /// <param name="maxSize">The maximum size of the pool.</param>
    /// <param name="actionWhenFull">The action to take when the pool is full.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize is less than 1.</exception>
    public Pool(int maxSize = 32,
        SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        if (maxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than 0.");
        }
        internalStructure = new(maxSize);
        
        ActionWhenFull = actionWhenFull;
        MaxSize = maxSize;
        for (var i = 0; i < maxSize; i++)
        {
            internalStructure.Add(new InternalPoolEntry<T>());
        }
    }
    
    /// <summary>
    /// Gets the maximum size of the pool.
    /// </summary>
    public int MaxSize { get; private set; }
    
    /// <summary>
    /// Gets or sets the action to take when the pool is full.
    /// </summary>
    public SizeLimitedCollectionFullAction ActionWhenFull { get; set; }
    
    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The item at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    /// <exception cref="Exception">Thrown when accessing an empty slot or overwriting an occupied slot.</exception>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= this.internalStructure.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (this.internalStructure[index].IsSlotFree == IS_FREE)
            {
                throw new Exception("No item in the slot!");
            }

            return this.internalStructure[index].Item;
        }

        set
        {
            if (index < 0 || index >= this.internalStructure.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (this.internalStructure[index].IsSlotFree == IS_USED)
            {
                throw new Exception("Item already in the slot!");
            }

            this.internalStructure[index].ClaimSlot(value);
        }
    }
    
    /// <summary>
    /// Returns an enumerator that iterates through the pool.
    /// </summary>
    /// <returns>An enumerator for the pool.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var entry in this.internalStructure)
        {
            if (entry.IsSlotFree == IS_USED)
            {
                yield return entry.Item;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds an item to the pool.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        AddAndReturn(item);
    }
    
    /// <summary>
    /// Adds a range of items to the pool.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            AddAndReturn(item);
        }
    }
    
    /// <summary>
    /// Adds a range of items to the pool and returns any items that were removed.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>A list of items that were removed from the pool.</returns>
    public List<T> AddRangeReturn(IEnumerable<T> items)
    {
        List<T> poppedItems = new();
        foreach (var item in items)
        {
            poppedItems.Add(AddAndReturn(item));
        }
        return poppedItems;
    }
    
    /// <summary>
    /// Adds an item to the pool and returns any item that was removed due to the pool being full.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was removed, or the default value of <typeparamref name="T"/> if no item was removed.</returns>
    public T AddAndReturn(T item)
    {
        // Two scenarios
        // Scenario 1: free slot -> used first unused slot
        // Scenario 2: no free slots -> use the specified Action When Full

        var usedFreeSlot = false;
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            if (this.internalStructure[i].IsSlotFree == IS_FREE)
            {
                this.internalStructure[i].ClaimSlot(item);
                usedFreeSlot = true;

                if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopNewestItem)
                {
                    this.actionIndex = i;
                }

                break;
            }
        }

        if (!usedFreeSlot)
        {
            T oldItem;
            switch (this.ActionWhenFull)
            {
                case SizeLimitedCollectionFullAction.PopNewestItem:
                    oldItem = this.internalStructure[this.actionIndex].Item;
                    this.internalStructure[this.actionIndex].ClaimSlot(item);

                    return oldItem;

                case SizeLimitedCollectionFullAction.PopOldestItem:
                    oldItem = this.internalStructure[this.actionIndex].Item;
                    this.internalStructure[this.actionIndex++].ClaimSlot(item);
                    if (this.actionIndex >= this.internalStructure.Count)
                    {
                        this.actionIndex = 0;
                    }

                    return oldItem;
            }
        }

        return default;
    }

    /// <summary>
    /// Clears all items from the pool.
    /// </summary>
    public void Clear()
    {
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            this.internalStructure[i].ClearSlot();
        }
    }

    /// <summary>
    /// Determines whether the pool contains a specific item.
    /// </summary>
    /// <param name="item">The item to locate in the pool.</param>
    /// <returns>True if the item is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        return this.internalStructure.Any(entry => entry.IsSlotFree == IS_USED && EqualityComparer<T>.Default.Equals(entry.Item, item));
    }

    /// <summary>
    /// Copies the elements of the pool to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the arrayIndex is out of range.</exception>
    /// <exception cref="ArgumentException">Thrown when the array does not have enough space to copy the elements.</exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0 || arrayIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (array.Length - arrayIndex < Count)
        {
            throw new ArgumentException("The array does not have enough space to copy the elements.");
        }

        foreach (var entry in this.internalStructure)
        {
            if (entry.IsSlotFree == IS_USED)
            {
                array[arrayIndex++] = entry.Item;
            }
        }
    }

    /// <summary>
    /// Removes the first occurrence of a specific item from the pool.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            if (this.internalStructure[i].IsSlotFree == IS_USED && EqualityComparer<T>.Default.Equals(this.internalStructure[i].Item, item))
            {
                this.internalStructure[i].ClearSlot();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets all items currently in the pool.
    /// </summary>
    /// <returns>A list of items in the pool.</returns>
    public List<T> GetItems()
    {
        var items = this.internalStructure.Where(x => x.IsSlotFree == IS_USED).Select(x => x.Item).ToList();
        return items;
    }

    /// <summary>
    /// Gets the number of items currently in the pool.
    /// </summary>
    public int Count => GetItems().Count;
    
    /// <summary>
    /// Gets the remaining capacity of the pool.
    /// </summary>
    public int RemainingCapacity => MaxSize - Count;

    /// <summary>
    /// Gets a value indicating whether the pool is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Releases all resources used by the pool.
    /// </summary>
    public void Dispose()
    {
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            if (this.internalStructure[i].Item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        this.internalStructure.Clear();
    }
}

