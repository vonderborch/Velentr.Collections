namespace Velentr.Collections.CollectionFullActions;

/// <summary>
///     Defines the actions to take when a pool reaches its maximum capacity.
/// </summary>
public enum PoolFullAction
{
    /// <summary>
    ///     Removes the oldest item in the pool to make space for a new item.
    /// </summary>
    PopOldestItem = 0,

    /// <summary>
    ///     Removes the newest item in the pool to make space for a new item.
    /// </summary>
    PopNewestItem = 1,

    /// <summary>
    ///     Grows the pool when it is full.
    /// </summary>
    Grow = 2,

    /// <summary>
    ///     Ignore the request when the pool is full.
    /// </summary>
    Ignore = 3,

    /// <summary>
    ///     Throw an exception when the pool is full.
    /// </summary>
    ThrowException = 4
}
