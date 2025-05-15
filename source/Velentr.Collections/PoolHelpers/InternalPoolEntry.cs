using Velentr.Core.Threading;

namespace Velentr.Collections.PoolHelpers;

/// <summary>
///     Represents an entry in a pool, which can hold an item and track whether the slot is free.
/// </summary>
/// <typeparam name="T">The type of the item stored in the pool entry.</typeparam>
internal class InternalPoolEntry<T>
{
    /// <summary>
    ///     Indicates whether the slot is claimed or free.
    /// </summary>
    public Guard IsSlotClaimed;

    /// <summary>
    ///     The item stored in the pool entry.
    /// </summary>
    public T? Item;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InternalPoolEntry{T}" /> class with the slot marked as free.
    /// </summary>
    public InternalPoolEntry() : this(default)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InternalPoolEntry{T}" /> class with the specified item.
    /// </summary>
    /// <param name="item">The item to store in the pool entry.</param>
    public InternalPoolEntry(T? item)
    {
        this.IsSlotClaimed = new Guard();
        this.Item = item;
    }

    /// <summary>
    ///     Claims the slot and assigns the specified item to it.
    /// </summary>
    /// <param name="item">The item to assign to the slot.</param>
    /// <param name="checkSet">True to run the checkSet operation on the slot, False otherwise.</param>
    /// <returns>True if the slot was claimed, False otherwise.</returns>
    public bool ClaimSlot(T item, bool checkSet = true)
    {
        if (checkSet && !this.IsSlotClaimed.CheckSet)
        {
            return false;
        }

        this.Item = item;
        return true;
    }

    /// <summary>
    ///     Clears the slot, marking it as free.
    /// </summary>
    public void ClearSlot()
    {
        this.IsSlotClaimed.Reset();
    }
}
