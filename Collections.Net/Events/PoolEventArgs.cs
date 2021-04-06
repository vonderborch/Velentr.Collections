﻿using System;

namespace Collections.Net.Events
{
    /// <summary>
    /// Event arguments for Pool Collection events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.EventArgs" />
    public class PoolEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <value>
        /// The item.
        /// </value>
        public T Item { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolEventArgs{T}"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public PoolEventArgs(T item)
        {
            Item = item;
        }
    }
}
