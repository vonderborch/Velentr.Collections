using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Velentr.Collections.Exceptions;
using Velentr.Collections.Internal;
using Velentr.Core.Helpers.Threading;

namespace Velentr.Collections.LockFree
{
    /// <summary>
    ///     Defines a Lock-Free Queue Collection (FIFO).
    /// </summary>
    /// <typeparam name="T">The type associated with the Queue instance</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="IEnumerable{T}" />
    /// <seealso cref="IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class LockFreeQueue<T> : Collection, IEnumerable<T>, IEnumerable
    {
        /// <summary>
        ///     The head
        /// </summary>
        private Node<T> _head;

        /// <summary>
        ///     The tail
        /// </summary>
        private Node<T> _tail;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LockFreeQueue{T}" /> class.
        /// </summary>
        public LockFreeQueue()
        {
            this._count = 0;
            this._head = new Node<T>();
            this._tail = this._head;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LockFreeQueue{T}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public LockFreeQueue(T value)
        {
            this._count = 0;
            this._head = new Node<T>();
            this._tail = this._head;
            Enqueue(value);
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
            var oldHead = this._head;
            this._head = this._tail = new Node<T>();

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
        ///     Dequeues the next value from the Queue.
        /// </summary>
        /// <returns>The dequeued value.</returns>
        public T Dequeue()
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            Dequeue(out var result);

            return result;
        }

        /// <summary>
        ///     Dequeues the next value from the Queue.
        /// </summary>
        /// <param name="value">The dequeued value.</param>
        /// <returns>Whether the dequeue was successful or not</returns>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public bool Dequeue(out T value)
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            var result = default(T);

            var updated = false;
            do
            {
                var previousHead = this._head;
                var previousTail = this._tail;
                var previousNextHead = previousHead.Next;

                if (previousHead == this._head)
                {
                    if (previousHead == previousTail)
                    {
                        if (previousNextHead == null)
                        {
                            value = default;

                            return false;
                        }

                        AtomicOperations.CAS(ref this._head, previousNextHead, previousHead);
                    }
                }
                else
                {
                    result = previousNextHead.Value;
                    updated = AtomicOperations.CAS(ref this._head, previousNextHead, previousHead);
                }
            } while (!updated);

            DecrementCount();
            value = result;

            return true;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this._disposed = true;
            Clear();
            this._head.Dispose();
            this._tail.Dispose();
        }

        /// <summary>
        ///     Enqueues the specified value onto the Queue.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="Collections.Net.Exceptions.CollectionDisposedException"></exception>
        public void Enqueue(T value)
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            Node<T> previousTail = null;
            Node<T> previousNext = null;
            var newNode = new Node<T>(value);

            var updated = false;
            do
            {
                previousTail = this._tail;
                previousNext = previousTail.Next;

                if (this._tail == previousTail)
                {
                    if (previousNext == null)
                    {
                        updated = AtomicOperations.CAS(ref this._tail.Next, newNode, null);
                    }
                    else
                    {
                        AtomicOperations.CAS(ref this._tail, previousNext, previousTail);
                    }
                }
            } while (!updated);

            IncrementCount();
            AtomicOperations.CAS(ref this._tail, newNode, previousTail);
        }

        /// <summary>
        ///     Peeks at the value at the top of the Queue.
        /// </summary>
        /// <returns>The next value.</returns>
        public T Peek()
        {
            if (this._disposed)
            {
                throw new CollectionDisposedException();
            }

            return this._head.Next.Value;
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
            var head = this._head;

            do
            {
                if (enumeratorVersion != this._version)
                {
                    throw new CollectionModifiedException();
                }

                yield return head.Value;
                head = head.Next;
            } while (head != null);
        }
    }
}
