using System;

namespace Velentr.Collections.Exceptions
{
    /// <summary>
    /// Defines an Exception that occurs when a collection is disposed and an attempt to use it occurs.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CollectionDisposedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionDisposedException"/> class.
        /// </summary>
        public CollectionDisposedException() : base("The collection has already been disposed!")
        {
        }
    }
}