using System;
using System.Collections.Generic;

namespace Velentr.Collections.Events
{
    /// <summary>
    /// An event that fires as part of a Collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionEvent<T> where T : EventArgs
    {
        /// <summary>
        /// The delegates
        /// </summary>
        internal List<EventHandler<T>> Delegates = new List<EventHandler<T>>();

        /// <summary>
        /// Occurs when [event].
        /// </summary>
        public event EventHandler<T> Event
        {
            add
            {
                InternalEvent += value;
                Delegates.Add(value);
            }

            remove
            {
                InternalEvent -= value;
                Delegates.Remove(value);
            }
        }

        /// <summary>
        /// Occurs when [internal event].
        /// </summary>
        internal event EventHandler<T> InternalEvent;

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static CollectionEvent<T> operator -(CollectionEvent<T> left, EventHandler<T> right)
        {
            left.InternalEvent -= right;
            left.Delegates.Remove(right);

            return left;
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static CollectionEvent<T> operator +(CollectionEvent<T> left, EventHandler<T> right)
        {
            left.InternalEvent += right;
            left.Delegates.Add(right);

            return left;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            var list = Delegates;
            for (var i = 0; i < list.Count; i++)
            {
                InternalEvent -= list[i];
            }

            Delegates.Clear();
        }

        /// <summary>
        /// Triggers the event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        public void TriggerEvent(object sender, T e)
        {
            InternalEvent?.Invoke(sender, e);
        }
    }
}