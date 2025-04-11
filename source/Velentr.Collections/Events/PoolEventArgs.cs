namespace Velentr.Collections.Events;

/// <summary>
/// Provides data for events that occur when a slot is released in a pool.
/// </summary>
/// <typeparam name="T">The type of item stored in the pool.</typeparam>
/// <param name="item">The item that was released from the pool.</param>
public class ReleasedSlotPoolEventArgs<T>(T item) : EventArgs
{
    /// <summary>
    /// Gets or sets the item that was removed from the pool.
    /// </summary>
    public T OldItem { get; set; } = item;
}

/// <summary>
/// Provides data for events that occur when a slot is claimed in a pool.
/// </summary>
/// <typeparam name="T">The type of item stored in the pool.</typeparam>
/// <param name="item">The item that was added to the pool.</param>
public class ClaimedSlotPoolEventArgs<T>(T item) : EventArgs
{
    /// <summary>
    /// Gets or sets the item that was added to the pool.
    /// </summary>
    public T NewItem { get; set; } = item;
}

/// <summary>
/// Provides data for events that occur when a slot claim operation fails in a pool.
/// </summary>
/// <typeparam name="T">The type of item that could not be added to the pool.</typeparam>
/// <param name="item">The item that could not be added.</param>
/// <param name="ex">The exception that occurred, if any.</param>
public class SlotClaimFailureEventArgs<T>(T item, Exception? ex) : EventArgs
{
    /// <summary>
    /// Gets or sets the item that could not be added to the pool.
    /// </summary>
    public T ItemNotAdded { get; set; } = item;

    /// <summary>
    /// Gets or sets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; set; } = ex;
}
