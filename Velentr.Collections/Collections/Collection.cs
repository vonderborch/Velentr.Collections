using Collections.Net.Helpers;
using System;
using System.Threading;

namespace Collections.Net.Collections
{
    /// <summary>
    /// An abstract Collection
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class Collection : IDisposable
    {
        /// <summary>
        /// The count
        /// </summary>
        protected long _count;

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public long Count => _count;

        /// <summary>
        /// The disposed
        /// </summary>
        protected bool _disposed = false;

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        protected long Version => _version;

        /// <summary>
        /// The version
        /// </summary>
        protected long _version;

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Determines whether this instance is empty.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEmpty()
        {
            return _count == 0;
        }

        /// <summary>
        /// Increments the version.
        /// </summary>
        protected void IncrementVersion()
        {
            Interlocked.Increment(ref _version);
        }

        /// <summary>
        /// Decrements the count.
        /// </summary>
        protected void DecrementCount()
        {
            Interlocked.Increment(ref _version);
            Interlocked.Decrement(ref _count);
        }

        /// <summary>
        /// Increments the count.
        /// </summary>
        protected void IncrementCount()
        {
            Interlocked.Increment(ref _version);
            Interlocked.Increment(ref _count);
        }

        /// <summary>
        /// Updates the count.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void UpdateCount(long value)
        {
            long newCount;

            do
            {
                newCount = _count + value;
                if (newCount < 0) newCount = 0;
            } while (!AtomicOperations.CAS(ref _count, newCount, _count));
        }

    }
}
