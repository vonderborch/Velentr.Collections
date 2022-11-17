namespace Velentr.Collections.CollectionActions
{
    /// <summary>
    /// Actions to take when a pool is full.
    /// </summary>
    public enum PoolFullAction
    {
        /// <summary>
        /// Return null when the pool is full.
        /// </summary>
        ReturnNull,

        /// <summary>
        /// Increase the pool size when the pool is full.
        /// </summary>
        IncreaseSize,

        /// <summary>
        /// Throw an exception when the pool is full.
        /// </summary>
        ThrowException,
    }
}