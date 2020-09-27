using Collections.Net.Helpers;
using Collections.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Collections.Net.Collections.LockFree
{
    [DebuggerDisplay("Count = {Count}")]
    public class Stack<T> : AbstractCollection
    {
        private readonly Node<T> _head;

        public Stack()
        {
            _head = new Node<T>();
        }

        public Stack(T item)
        {
            Push(item);
        }

        public override void Clear(bool disposeOldStack = true)
        {
            // reset the stack
            Node<T> oldHead;
            long count;
            do
            {
                oldHead = _head.Next;
                count = Count;
            } while (!AtomicOperations.CAS(ref _head.Next, null, oldHead));
            UpdateCount(-count);

            // dispose of each of the old nodes
            // TODO: thread this so that it can happen in the background
            if (disposeOldStack)
            {
                InternalDisposeNodes(oldHead);
            }
        }

        private void InternalDisposeNodes(Node<T> head)
        {
            while (head != null)
            {
                var nextHead = head.Next;
                if (typeof(T).GetInterfaces().Contains(typeof(IDisposable)))
                {
                    ((IDisposable)head.Value).Dispose();
                }
                head = nextHead;
            }
        }

        public T Peek()
        {
            return _head.Next.Value;
        }

        public T Pop()
        {
            var success = Pop(out var result);
            if (!success)
            {
                throw new NullReferenceException("No nodes on the stack to pop!");
            }
            return result;
        }

        public bool Pop(out T returnValue)
        {
            Node<T> node;

            do
            {
                node = _head.Next;
                if (node == null)
                {
                    returnValue = default;
                    return false;
                }
            } while (!AtomicOperations.CASE(ref _head.Next, node.Next, node));

            DecrementCount();
            returnValue = node.Value;
            return true;
        }

        public List<T> PopRange(int amount)
        {
            T lastPopped;
            List<T> values = new List<T>();
            for (var i = 0; i < amount; i++)
            {
                if (Pop(out lastPopped))
                {
                    values.Add(lastPopped);
                }
                else
                {
                    break;
                }
            }

            return values;
        }

        public void Push(T value)
        {
            var newNode = new Node<T>(value);

            do
            {
                newNode.Next = _head.Next;
            } while (!AtomicOperations.CAS(ref _head.Next, newNode, newNode.Next));

            IncrementCount();
        }

        public void Push(List<T> values)
        {
            for (var i = 0; i < values.Count; i++)
            {
                Push(values[i]);
            }
        }
    }
}
