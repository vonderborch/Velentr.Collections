using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

using Velentr.Collections.Exceptions;
using Velentr.Collections.LockFree;
using Velentr.Collections.PriorityConverters;

namespace Velentr.Collections.Concurrent
{
    /// <summary>
    ///     Defines a Concurrent Priority Queue Collection (FIFO).
    /// </summary>
    /// <typeparam name="T">The type associated with the Priority Queue instance</typeparam>
    /// <typeparam name="ConverterType">The type associated with the Priority Converter for the Priority Queue instance</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="IEnumerable{T}" />
    /// <seealso cref="IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class ConcurrentLimitedPriorityQueue<T, ConverterType> : Collection, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        ///     The converter
        /// </summary>
        private readonly PriorityConverter<ConverterType> _converter;

        /// <summary>
        ///     The queues
        /// </summary>
        private readonly Dictionary<ConverterType, ConcurrentQueue<T>> _queues;

        /// <summary>
        ///     The valid integer values
        /// </summary>
        private readonly List<int> _validIntegerValues;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LockFreePriorityQueue{T}" /> class.
        /// </summary>
        /// <param name="converter">The converter.</param>
        public ConcurrentLimitedPriorityQueue(PriorityConverter<ConverterType> converter)
        {
            this._converter = converter;

            this._queues = new Dictionary<ConverterType, ConcurrentQueue<T>>(this._converter.OptionCount);
            this._validIntegerValues = new List<int>(this._converter.OptionCount);
            foreach (var option in this._converter.Options)
            {
                this._queues.Add(option.Key, new ConcurrentQueue<T>());
                this._validIntegerValues.Add(option.Value);
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        ///     Clears the collection.
        /// </summary>
        public override void Clear()
        {
            this._version = 0;
            long count = 0;
            foreach (var queue in this._queues)
            {
                count += queue.Value.Count;

                while (queue.Value.TryDequeue(out var _))
                {
                    ;
                }
            }

            UpdateCount(-count);
        }

        /// <summary>
        ///     Dequeues the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public bool Dequeue(out T value)
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            foreach (var queue in this._queues)
            {
                if (queue.Value.TryDequeue(out value))
                {
                    DecrementCount();

                    return true;
                }
            }

            value = default;

            return false;
        }

        /// <summary>
        ///     Dequeues this instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public T Dequeue()
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            Dequeue(out var value);

            return value;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this._disposed = true;
            Clear();
        }

        /// <summary>
        ///     Enqueues the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public void Enqueue(T value, ConverterType priority)
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            this._queues[priority].Enqueue(value);
            IncrementCount();
        }

        /// <summary>
        ///     Enqueues the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        /// <exception cref="Collections.Net.Exceptions.InvalidPriorityException"></exception>
        public void Enqueue(T value, int priority)
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            if (IsValidPriority(priority))
            {
                throw new InvalidPriorityException();
            }

            Enqueue(value, this._converter.ConvertFromInt(priority));
        }

        /// <summary>
        ///     Determines whether [is valid priority] [the specified priority].
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <returns>
        ///     <c>true</c> if [is valid priority] [the specified priority]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public bool IsValidPriority(int priority)
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            return this._validIntegerValues.Contains(priority);
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        private IEnumerator<T> GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        ///     Internals the get enumerator.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        /// <exception cref="Collections.Net.Exceptions.CollectionModifiedException"></exception>
        private IEnumerator<T> InternalGetEnumerator()
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            var enumeratorVersion = this._version;
            foreach (var queue in this._queues)
            {
                foreach (var item in queue.Value)
                {
                    if (enumeratorVersion != this._version)
                    {
                        throw new CollectionModifiedException();
                    }

                    yield return item;
                }
            }
        }
    }
}
