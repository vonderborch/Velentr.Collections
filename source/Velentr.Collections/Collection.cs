using System;
using System.Threading;

using Velentr.Core.Helpers.Threading;

namespace Velentr.Collections
{
    /// <summary>
    ///     An abstract Collection
    /// </summary>
    /// <seealso cref="IDisposable" />
    public abstract class Collection : IDisposable
    {
        /// <summary>
        ///     The count
        /// </summary>
        protected long _count;

        /// <summary>
        ///     The disposed
        /// </summary>
        protected bool _disposed = false;

        /// <summary>
        ///     The version
        /// </summary>
        protected long _version;

        /// <summary>
        ///     Gets the count.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        public long Count => this._count;

        /// <summary>
        ///     Gets the version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        protected long Version => this._version;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        ///     Determines whether this instance is empty.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </returns>
        public bool IsEmpty()
        {
            return this._count == 0;
        }

        /// <summary>
        ///     Decrements the count.
        /// </summary>
        protected void DecrementCount()
        {
            Interlocked.Increment(ref this._version);
            Interlocked.Decrement(ref this._count);
        }

        /// <summary>
        ///     Increments the count.
        /// </summary>
        protected void IncrementCount()
        {
            Interlocked.Increment(ref this._version);
            Interlocked.Increment(ref this._count);
        }

        /// <summary>
        ///     Increments the version.
        /// </summary>
        protected void IncrementVersion()
        {
            Interlocked.Increment(ref this._version);
        }

        /// <summary>
        ///     Updates the count.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void UpdateCount(long value)
        {
            IncrementVersion();
            long newCount;

            do
            {
                newCount = this._count + value;
                if (newCount < 0)
                {
                    newCount = 0;
                }
            } while (!AtomicOperations.CAS(ref this._count, newCount, this._count));
        }
    }
}
