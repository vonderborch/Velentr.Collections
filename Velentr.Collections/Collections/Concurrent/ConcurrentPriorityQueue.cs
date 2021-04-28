using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Velentr.Collections.Collections.LockFree;
using Velentr.Collections.Exceptions;
using Velentr.Collections.PriorityConverters;

namespace Velentr.Collections.Collections.Concurrent
{
    /// <summary>
    /// Defines a Concurrent Priority Queue Collection (FIFO).
    /// </summary>
    /// <typeparam name="T">The type associated with the Priority Queue instance</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    /// <seealso cref="System.Collections.IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class ConcurrentPriorityQueue<T> : Collection, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// The queues
        /// </summary>
        private Dictionary<QueuePriority, ConcurrentQueue<T>> _queues;

        /// <summary>
        /// The valid integer values
        /// </summary>
        private List<int> _validIntegerValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockFreePriorityQueue{T}"/> class.
        /// </summary>
        public ConcurrentPriorityQueue()
        {
            _queues = new Dictionary<QueuePriority, ConcurrentQueue<T>>(Enum.GetValues(typeof(QueuePriority)).Length);
            _validIntegerValues = new List<int>(Enum.GetValues(typeof(QueuePriority)).Length);
            var values = Enum.GetValues(typeof(QueuePriority));
            for (var i = 0; i < values.Length; i++)
            {
                _queues.Add((QueuePriority)values.GetValue(i), new ConcurrentQueue<T>());
                _validIntegerValues.Add((int)values.GetValue(i));
            }
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public override void Clear()
        {
            _version = 0;
            long count = 0;
            foreach (var queue in _queues)
            {
                count += queue.Value.Count;
                while (queue.Value.TryDequeue(out var _)) ;
            }

            UpdateCount(-count);
        }

        /// <summary>
        /// Dequeues the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="CollectionDisposedException"></exception>
        public bool Dequeue(out T value)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            foreach (var queue in _queues)
            {
                if (queue.Value.TryDequeue(out value))
                {
                    DecrementCount();
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Dequeues this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CollectionDisposedException"></exception>
        public T Dequeue()
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            Dequeue(out var value);
            return value;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _disposed = true;
            Clear();
        }

        /// <summary>
        /// Enqueues the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="CollectionDisposedException"></exception>
        public void Enqueue(T value, QueuePriority priority)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            _queues[priority].Enqueue(value);
            IncrementCount();
        }

        /// <summary>
        /// Enqueues the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="CollectionDisposedException"></exception>
        /// <exception cref="InvalidPriorityException"></exception>
        public void Enqueue(T value, int priority)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            if (IsValidPriority(priority))
            {
                throw new InvalidPriorityException();
            }

            Enqueue(value, (QueuePriority)priority);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        /// Determines whether [is valid priority] [the specified priority].
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <returns>
        ///   <c>true</c> if [is valid priority] [the specified priority]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="CollectionDisposedException"></exception>
        public bool IsValidPriority(int priority)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            return _validIntegerValues.Contains(priority);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        private IEnumerator<T> GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        /// Internals the get enumerator.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="CollectionDisposedException"></exception>
        /// <exception cref="CollectionModifiedException"></exception>
        private IEnumerator<T> InternalGetEnumerator()
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            var enumeratorVersion = _version;
            foreach (var queue in _queues)
            {
                foreach (var item in queue.Value)
                {
                    if (enumeratorVersion != _version)
                    {
                        throw new CollectionModifiedException();
                    }
                    yield return item;
                }
            }
        }
    }
}