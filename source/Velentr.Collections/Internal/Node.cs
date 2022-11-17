using System;

using Velentr.Core.Helpers.General;

namespace Velentr.Collections.Internal
{
    /// <summary>
    ///     A singly-linked node
    /// </summary>
    /// <typeparam name="T">The type of the value for the node</typeparam>
    internal class Node<T> : IDisposable
    {
        /// <summary>
        ///     Whether the object has been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     The next node
        /// </summary>
        public Node<T> Next;

        /// <summary>
        ///     The value
        /// </summary>
        public T Value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Node{T}" /> class.
        /// </summary>
        public Node()
            : this(default) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Node{T}" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public Node(T value)
        {
            this.Next = null;
            this.Value = value;
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Node{T}" /> is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        public bool Disposed => this._disposed;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    DisposingHelpers.DisposeIfPossible(this.Value);
                }

                // Indicate that the instance has been disposed.
                this._disposed = true;
            }
        }
    }
}
