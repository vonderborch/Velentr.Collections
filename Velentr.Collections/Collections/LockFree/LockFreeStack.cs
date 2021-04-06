using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Velentr.Collections.Collections.Internal;
using Velentr.Collections.Exceptions;
using Velentr.Collections.Helpers;

namespace Velentr.Collections.Collections.LockFree
{
    /// <summary>
    /// Defines a Lock-Free Stack Collection (FILO).
    /// </summary>
    /// <typeparam name="T">The type associated with the Stack instance</typeparam>
    /// <seealso cref="Collections.Net.Collections.Collection" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    /// <seealso cref="System.Collections.IEnumerable" />
    [DebuggerDisplay("Count = {Count}")]
    public class LockFreeStack<T> : Collection, IEnumerable<T>, IEnumerable
    {

        /// <summary>
        /// The head
        /// </summary>
        private Node<T> _head;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockFreeStack{T}"/> class.
        /// </summary>
        public LockFreeStack()
        {
            _count = 0;
            _head = new Node<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockFreeStack{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public LockFreeStack(T value)
        {
            _count = 0;
            _head = new Node<T>();
            Push(value);
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
        /// Clears the collection.
        /// </summary>
        public override void Clear()
        {
            _version = 0;
            var oldHead = _head;
            _head = new Node<T>();

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
        }

        /// <summary>
        /// Pops the next value from the Stack.
        /// </summary>
        /// <returns>The popped value.</returns>
        public T Pop()
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            Pop(out var value);
            return value;
        }

        /// <summary>
        /// Pops the next value from the Stack.
        /// </summary>
        /// <param name="value">The popped value.</param>
        /// <returns>Whether the Pop was successful or not.</returns>
        public bool Pop(out T value)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            Node<T> node;

            do
            {
                node = _head.Next;
                if (node == null)
                {
                    value = default;
                    return false;
                }
            } while (!AtomicOperations.CAS(ref _head.Next, node.Next, node));

            value = node.Value;
            DecrementCount();
            return true;
        }

        /// <summary>
        /// Pushes the specified value onto the Stack.
        /// </summary>
        /// <param name="value">The value to push.</param>
        public void Push(T value)
        {
            if (_disposed)
            {
                throw new CollectionDisposedException();
            }

            var newNode = new Node<T>(value);

            do
            {
                newNode.Next = _head.Next;
            } while (!AtomicOperations.CAS(ref _head.Next, newNode, newNode.Next));

            IncrementCount();
        }

        /// <summary>
        /// Peeks at the value at the top of the Stack.
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
