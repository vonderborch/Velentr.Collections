using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Velentr.Collections.Collections.Internal;
using Velentr.Collections.Exceptions;
using Velentr.Collections.Helpers;

namespace Velentr.Collections.Collections.LockFree
{
    /// <summary>
    /// Defines a Lock-Free Queue Collection (FIFO).
    /// </summary>
    /// <typeparam name="T">The type associated with the Queue instance</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    /// <seealso cref="System.Collections.IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class LockFreeQueue<T> : Collection, IEnumerable<T>, IEnumerable
    {

        /// <summary>
        /// The head
        /// </summary>
        private Node<T> _head;

        /// <summary>
        /// The tail
        /// </summary>
        private Node<T> _tail;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockFreeQueue{T}"/> class.
        /// </summary>
        public LockFreeQueue()
        {
            _count = 0;
            _head = new Node<T>();
            _tail = _head;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockFreeQueue{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public LockFreeQueue(T value)
        {
            _count = 0;
            _head = new Node<T>();
            _tail = _head;
            Enqueue(value);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public override void Clear()
        {
            _version = 0;
            var oldHead = _head;
            _head = _tail = new Node<T>();

            var nodes = 0;
            while (oldHead != null)
            {
                nodes++;
                var nextHead = oldHead.Next;
                oldHead.Dispose();

                oldHead = nextHead;
            }
            UpdateCount(-nodes);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _disposed = true;
            Clear();
            _head.Dispose();
            _tail.Dispose();
        }

        /// <summary>
        /// Dequeues the next value from the Queue.
        /// </summary>
        /// <returns>The dequeued value.</returns>
        public T Dequeue()
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            Dequeue(out var result);
            return result;
        }

        /// <summary>
        /// Dequeues the next value from the Queue.
        /// </summary>
        /// <param name="value">The dequeued value.</param>
        /// <returns>Whether the dequeue was successful or not</returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public bool Dequeue(out T value)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            var result = default(T);

            var updated = false;
            do
            {
                var previousHead = _head;
                var previousTail = _tail;
                var previousNextHead = previousHead.Next;

                if (previousHead == _head)
                {
                    if (previousHead == previousTail)
                    {
                        if (previousNextHead == null)
                        {
                            value = default;
                            return false;
                        }

                        AtomicOperations.CAS(ref _head, previousNextHead, previousHead);
                    }
                }
                else
                {
                    result = previousNextHead.Value;
                    updated = AtomicOperations.CAS(ref _head, previousNextHead, previousHead);
                }
            } while (!updated);

            DecrementCount();
            value = result;
            return true;
        }

        /// <summary>
        /// Enqueues the specified value onto the Queue.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public void Enqueue(T value)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            Node<T> previousTail = null;
            Node<T> previousNext = null;
            var newNode = new Node<T>(value);

            var updated = false;
            do
            {
                previousTail = _tail;
                previousNext = previousTail.Next;

                if (_tail == previousTail)
                {
                    if (previousNext == null)
                    {
                        updated = AtomicOperations.CAS(ref _tail.Next, newNode, null);
                    }
                    else
                    {
                        AtomicOperations.CAS(ref _tail, previousNext, previousTail);
                    }
                }
            } while (!updated);

            IncrementCount();
            AtomicOperations.CAS(ref _tail, newNode, previousTail);
        }

        /// <summary>
        /// Peeks at the value at the top of the Queue.
        /// </summary>
        /// <returns>The next value.</returns>
        public T Peek()
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            return _head.Next.Value;
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
            var head = _head;

            do
            {
                if (enumeratorVersion != _version)
                {
                    throw new CollectionModifiedException();
                }

                yield return head.Value;
                head = head.Next;
            } while (head != null);
        }
    }
}
