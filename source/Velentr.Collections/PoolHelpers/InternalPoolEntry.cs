namespace Velentr.Collections.PoolHelpers;

/// <summary>
///     Represents an entry in a pool, which can hold an item and track whether the slot is free.
/// </summary>
/// <typeparam name="T">The type of the item stored in the pool entry.</typeparam>
internal class InternalPoolEntry<T>
{
    /// <summary>
    ///     Indicates whether the slot is free.
    /// </summary>
    public bool IsSlotFree;

    /// <summary>
    ///     The item stored in the pool entry.
    /// </summary>
    public T Item;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InternalPoolEntry{T}" /> class with the slot marked as free.
    /// </summary>
    public InternalPoolEntry()
    {
        this.IsSlotFree = true;
        this.Item = default;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InternalPoolEntry{T}" /> class with the specified item.
    /// </summary>
    /// <param name="item">The item to store in the pool entry.</param>
    public InternalPoolEntry(T item)
    {
        this.IsSlotFree = false;
        this.Item = item;
    }

    /// <summary>
    ///     Claims the slot and assigns the specified item to it.
    /// </summary>
    /// <param name="item">The item to assign to the slot.</param>
    public void ClaimSlot(T item)
    {
        this.Item = item;
        this.IsSlotFree = false;
    }

    /// <summary>
    ///     Clears the slot, marking it as free.
    /// </summary>
    public void ClearSlot()
    {
        this.IsSlotFree = true;
    }
}
