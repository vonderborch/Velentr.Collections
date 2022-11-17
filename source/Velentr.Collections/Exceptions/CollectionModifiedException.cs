using System;

namespace Velentr.Collections.Exceptions
{
    /// <summary>
    /// Defines an Exception that occurs when a collection is modified while enumerating through it.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class CollectionModifiedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionModifiedException"/> class.
        /// </summary>
        public CollectionModifiedException() : base("The Collection has been modified!")
        {
        }
    }
}