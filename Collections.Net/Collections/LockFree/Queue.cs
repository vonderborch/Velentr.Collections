using Collections.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Collections.Net.Collections.LockFree
{
    [DebuggerDisplay("Count = {Count}")]
    public class Queue<T> : AbstractCollection
    {
        private Node<T> _head;

        private Node<T> _tail;

        public Queue()
        {
            var node = new Node<T>();
            _head.Next = _tail.Next = node;
        }

        public T Peek()
        {
            return _head.Next.Value;
        }

        public bool Dequeue(out T returnValue)
        {
            T result = default;
            

            do
            {
                var 
            }
        }
    }
}
