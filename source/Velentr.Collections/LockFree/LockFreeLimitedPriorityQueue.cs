﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Velentr.Collections.Exceptions;
using Velentr.Collections.PriorityConverters;

namespace Velentr.Collections.Collections.LockFree
{
    /// <summary>
    /// Defines a Lock-Free Priority Queue Collection (FIFO).
    /// </summary>
    /// <typeparam name="T">The type associated with the Priority Queue instance</typeparam>
    /// <typeparam name="ConverterType">The type associated with the Priority Converter for the Priority Queue instance</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    /// <seealso cref="System.Collections.IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class LockFreeLimitedPriorityQueue<T, ConverterType> : Collection, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        /// The valid integer values
        /// </summary>
        private readonly List<int> _validIntegerValues;

        /// <summary>
        /// The converter
        /// </summary>
        private PriorityConverter<ConverterType> _converter;

        /// <summary>
        /// The queues
        /// </summary>
        private Dictionary<ConverterType, LockFreeQueue<T>> _queues;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockFreePriorityQueue{T}"/> class.
        /// </summary>
        /// <param name="converter">The converter.</param>
        public LockFreeLimitedPriorityQueue(PriorityConverter<ConverterType> converter)
        {
            _converter = converter;

            _queues = new Dictionary<ConverterType, LockFreeQueue<T>>(_converter.OptionCount);
            _validIntegerValues = new List<int>(_converter.OptionCount);
            foreach (var option in _converter.Options)
            {
                _queues.Add(option.Key, new LockFreeQueue<T>());
                _validIntegerValues.Add(option.Value);
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
                queue.Value.Clear();
            }

            UpdateCount(-count);
        }

        /// <summary>
        /// Dequeues the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public bool Dequeue(out T value)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            foreach (var queue in _queues)
            {
                if (queue.Value.Dequeue(out value))
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
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
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
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public void Enqueue(T value, ConverterType priority)
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
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        /// <exception cref="Collections.Net.Exceptions.InvalidPriorityException"></exception>
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

            Enqueue(value, _converter.ConvertFromInt(priority));
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
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
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
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        /// <exception cref="Collections.Net.Exceptions.CollectionModifiedException"></exception>
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