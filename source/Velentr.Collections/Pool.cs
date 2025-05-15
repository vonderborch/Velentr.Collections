using System.Collections;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Velentr.Collections.CollectionFullActions;
using Velentr.Collections.Events;
using Velentr.Collections.Internal;
using Velentr.Collections.PoolHelpers;
using Velentr.Core.Validation;

namespace Velentr.Collections;

/// <summary>
///     An exception that is thrown when the pool is full and cannot accept new items.
/// </summary>
/// <param name="message">The message for the exception.</param>
public class PoolFullException(string message) : Exception(message);

/// <summary>
///     Represents a pool collection that manages a fixed number of reusable objects.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class Pool<T> : IPool<T>
{
    /// <summary>
    ///     Indicates that a slot in the pool is free.
    /// </summary>
    private const bool IS_FREE = false;

    /// <summary>
    ///     Indicates that a slot in the pool is used.
    /// </summary>
    private const bool IS_USED = !IS_FREE;

    /// <summary>
    ///     The internal data structure storing the pool entries.
    /// </summary>
    private readonly List<InternalPoolEntry<T>> internalStructure;

    /// <summary>
    ///     The number of items in the pool.
    /// </summary>
    private int count;

    /// <summary>
    ///     The history of when indexes were used in the pool.
    /// </summary>
    private readonly History<int> indexHistory;

    /// <summary>
    ///     The version of the pool. Used to track changes to the pool.
    /// </summary>
    private long version;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Pool{T}" /> class.
    /// </summary>
    /// <param name="maxSize">The maximum size of the pool.</param>
    /// <param name="actionWhenFull">The action to take when the pool is full.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxSize is less than 1.</exception>
    public Pool(int maxSize = 32,
        PoolFullAction actionWhenFull = PoolFullAction.PopOldestItem)
    {
        Validations.ValidateRange(maxSize, nameof(maxSize), 1, int.MaxValue);

        this.version = 0;
        this.count = 0;
        this.internalStructure = new List<InternalPoolEntry<T>>(maxSize);
        this.indexHistory = new History<int>(maxSize);
        this.ClaimedSlotEvent = new CollectionEvent<ClaimedSlotPoolEventArgs<T>>();
        this.ReleasedSlotEvent = new CollectionEvent<ReleasedSlotPoolEventArgs<T>>();
        this.SlotClaimFailureEvent = new CollectionEvent<SlotClaimFailureEventArgs<T>>();

        this.ActionWhenFull = actionWhenFull;
        this.MaxSize = maxSize;
        for (var i = 0; i < maxSize; i++)
        {
            this.internalStructure.Add(new InternalPoolEntry<T>());
        }
    }

    /// <summary>
    ///     Gets the number of items currently in the pool.
    /// </summary>
    public int Count => this.count;

    /// <summary>
    ///     Gets a value indicating whether the pool is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Gets the remaining capacity of the pool.
    /// </summary>
    public int RemainingCapacity => this.MaxSize - this.Count;

    /// <summary>
    ///     Gets or sets the action to take when the pool is full.
    /// </summary>
    public PoolFullAction ActionWhenFull { get; set; }

    /// <summary>
    ///     Adds an item to the pool.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(T item)
    {
        AddAndReturn(item);
    }

    /// <summary>
    ///     Adds an item to the pool and returns any item that was removed due to the pool being full.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <returns>The item that was removed, or the default value of <typeparamref name="T" /> if no item was removed.</returns>
    public T? AddAndReturn(T item)
    {
        // Two scenarios
        // Scenario 1: free slot -> used first unused slot
        // Scenario 2: no free slots -> use the specified Action When Full

        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            if (this.internalStructure[i].IsSlotClaimed.CheckSet)
            {
                this.internalStructure[i].ClaimSlot(item, false);
                Interlocked.Increment(ref this.count);
                this.ClaimedSlotEvent?.EventTriggered(this, new ClaimedSlotPoolEventArgs<T>(item));
                this.indexHistory.Add(i);

                return default;
            }
        }

        T? oldItem;
        int actionIndex;
        switch (this.ActionWhenFull)
        {
            case PoolFullAction.PopNewestItem:
                actionIndex = this.indexHistory.NewestItem;
                oldItem = this.internalStructure[actionIndex].Item;

                this.ReleasedSlotEvent?.EventTriggered(this, new ReleasedSlotPoolEventArgs<T>(oldItem));
                this.internalStructure[actionIndex].ClaimSlot(item, false);
                this.ClaimedSlotEvent?.EventTriggered(this, new ClaimedSlotPoolEventArgs<T>(item));

                return oldItem;

            case PoolFullAction.PopOldestItem:
                actionIndex = this.indexHistory.OldestItem;
                oldItem = this.internalStructure[actionIndex].Item;
                this.ReleasedSlotEvent?.EventTriggered(this, new ReleasedSlotPoolEventArgs<T>(oldItem));
                this.internalStructure[actionIndex].ClaimSlot(item, false);
                this.indexHistory.Add(actionIndex);
                this.ClaimedSlotEvent?.EventTriggered(this, new ClaimedSlotPoolEventArgs<T>(item));

                return oldItem;

            case PoolFullAction.Grow:
                InternalPoolEntry<T> newEntry = new(item);
                newEntry.IsSlotClaimed.MarkChecked();
                this.internalStructure.Add(newEntry);
                this.MaxSize++;
                this.indexHistory.SetMaxSize(this.MaxSize);
                this.indexHistory.Add(this.internalStructure.Count - 1);
                Interlocked.Increment(ref this.count);
                this.ClaimedSlotEvent?.EventTriggered(this, new ClaimedSlotPoolEventArgs<T>(item));
                break;

            case PoolFullAction.Ignore:
                this.SlotClaimFailureEvent?.EventTriggered(this, new SlotClaimFailureEventArgs<T>(item, null));
                return default;

            case PoolFullAction.ThrowException:
                PoolFullException exception = new("The pool is full and cannot accept new items.");
                this.SlotClaimFailureEvent?.EventTriggered(this, new SlotClaimFailureEventArgs<T>(item, exception));
                throw exception;
        }

        return default;
    }

    /// <summary>
    ///     Adds a range of items to the pool.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            AddAndReturn(item);
        }
    }

    /// <summary>
    ///     Adds a range of items to the pool and returns any items that were removed.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <returns>A list of items that were removed from the pool.</returns>
    public List<T?> AddRangeAndReturnRemoved(IEnumerable<T> items)
    {
        List<T?> poppedItems = new();
        foreach (T item in items)
        {
            poppedItems.Add(AddAndReturn(item));
        }

        return poppedItems;
    }

    /// <summary>
    ///     Event that is triggered when a slot is claimed in the pool.
    /// </summary>
    [JsonIgnore]
    public CollectionEvent<ClaimedSlotPoolEventArgs<T>> ClaimedSlotEvent { get; set; }

    /// <summary>
    ///     Clears all items from the pool.
    /// </summary>
    public void Clear()
    {
        var startingVersion = this.version;
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            this.internalStructure[i].ClearSlot();
        }

        Interlocked.Exchange(ref this.count, 0);
        this.indexHistory.Clear();
        Interlocked.Increment(ref this.version);
    }

    /// <summary>
    ///     Clears all items from the pool and emits events for each item cleared.
    /// </summary>
    public void ClearAndEmitEvents()
    {
        var startingVersion = this.version;
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            if (!this.internalStructure[i].IsSlotClaimed.Check)
            {
                continue;
            }

            this.ReleasedSlotEvent?.EventTriggered(this,
                new ReleasedSlotPoolEventArgs<T>(this.internalStructure[i].Item));
            this.internalStructure[i].ClearSlot();
        }

        Interlocked.Exchange(ref this.count, 0);
        this.indexHistory.Clear();
        Interlocked.Increment(ref this.version);
    }

    /// <summary>
    ///     Determines whether the pool contains a specific item.
    /// </summary>
    /// <param name="item">The item to locate in the pool.</param>
    /// <returns>True if the item is found; otherwise, false.</returns>
    public bool Contains(T item)
    {
        var startingVersion = this.version;
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            if (this.internalStructure[i].IsSlotClaimed.Check &&
                EqualityComparer<T>.Default.Equals(this.internalStructure[i].Item, item))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Copies the elements of the pool to an array, starting at a particular array index.
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

        if (array.Length - arrayIndex < this.Count)
        {
            throw new ArgumentException("The array does not have enough space to copy the elements.");
        }

        var startingVersion = this.version;
        foreach (InternalPoolEntry<T> entry in this.internalStructure)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            if (entry.IsSlotClaimed.Check)
            {
                array[arrayIndex++] = entry.Item;
            }
        }
    }

    /// <summary>
    ///     Releases all resources used by the pool.
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

        Clear();
        this.internalStructure.Clear();
        this.ReleasedSlotEvent?.Clear();
        this.ClaimedSlotEvent?.Clear();
        this.SlotClaimFailureEvent?.Clear();
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the pool.
    /// </summary>
    /// <returns>An enumerator for the pool.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        var startingVersion = this.version;
        foreach (InternalPoolEntry<T> entry in this.internalStructure)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            if (entry.IsSlotClaimed == IS_USED)
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
    ///     Gets or sets the item at the specified index.
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

            if (!this.internalStructure[index].IsSlotClaimed.Check)
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

            if (!this.internalStructure[index].IsSlotClaimed.CheckSet)
            {
                throw new Exception("Item already in the slot!");
            }

            this.internalStructure[index].ClaimSlot(value, false);

            this.indexHistory.Add(index);
            Interlocked.Increment(ref this.version);
            Interlocked.Increment(ref this.count);
        }
    }

    /// <summary>
    ///     Gets the maximum size of the pool.
    /// </summary>
    public int MaxSize { get; private set; }

    /// <summary>
    ///     Event that is triggered when a slot is released in the pool.
    /// </summary>
    [JsonIgnore]
    public CollectionEvent<ReleasedSlotPoolEventArgs<T>> ReleasedSlotEvent { get; set; }

    /// <summary>
    ///     Removes the first occurrence of a specific item from the pool.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        var startingVersion = this.version;
        for (var i = 0; i < this.internalStructure.Count; i++)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            if (this.internalStructure[i].IsSlotClaimed == IS_USED &&
                EqualityComparer<T>.Default.Equals(this.internalStructure[i].Item, item))
            {
                this.ReleasedSlotEvent?.EventTriggered(this,
                    new ReleasedSlotPoolEventArgs<T>(this.internalStructure[i].Item));
                this.internalStructure[i].ClearSlot();
                Interlocked.Increment(ref this.version);
                Interlocked.Decrement(ref this.count);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Event that is triggered when a slot claim operation fails.
    /// </summary>
    [JsonIgnore]
    public CollectionEvent<SlotClaimFailureEventArgs<T>> SlotClaimFailureEvent { get; set; }

    /// <summary>
    ///     Gets an array of all items in the pool.
    /// </summary>
    /// <returns>An array of items in the pool.</returns>
    public T[] ToArray()
    {
        T[] items = new T[this.Count];
        var startingVersion = this.version;
        var index = 0;
        foreach (InternalPoolEntry<T> entry in this.internalStructure)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            if (entry.IsSlotClaimed == IS_USED)
            {
                items[index++] = entry.Item;
            }
        }

        return items;
    }

    /// <summary>
    ///     Gets a list of all items in the pool.
    /// </summary>
    /// <returns>A list of items in the pool.</returns>
    public List<T> ToList()
    {
        List<T> items = new(this.Count);
        var startingVersion = this.version;
        foreach (InternalPoolEntry<T> entry in this.internalStructure)
        {
            CollectionValidators.ValidateCollectionState(startingVersion, this.version);

            if (entry.IsSlotClaimed == IS_USED)
            {
                items.Add(entry.Item);
            }
        }

        return items;
    }
}
