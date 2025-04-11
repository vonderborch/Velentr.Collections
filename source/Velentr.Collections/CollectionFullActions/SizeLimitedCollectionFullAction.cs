namespace Velentr.Collections.CollectionFullActions;

/// <summary>
///     Defines the actions to take when a size-limited collection reaches its maximum capacity.
/// </summary>
public enum SizeLimitedCollectionFullAction
{
    /// <summary>
    ///     Removes the oldest item in the collection to make space for a new item.
    /// </summary>
    PopOldestItem = 0,

    /// <summary>
    ///     Removes the newest item in the collection to make space for a new item.
    /// </summary>
    PopNewestItem = 1
}
