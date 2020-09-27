using Collections.Net.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Collections.Net.Collections
{
    internal abstract class AbstractCollection : IDisposable
    {
        public long _count;

        public long Count => _count;

        public abstract void Clear();

        public abstract void Dispose();

        public bool IsEmpty()
        {
            return _count == 0;
        }

        internal void DecrementCount()
        {
            Interlocked.Decrement(ref _count);
        }

        internal void IncrementCount()
        {
            Interlocked.Increment(ref _count);
        }

        internal void UpdateCount(long value)
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
