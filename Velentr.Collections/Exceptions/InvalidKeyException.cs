using System;

namespace Velentr.Collections.Exceptions
{
    /// <summary>
    /// Defines an Exception that occurs when a collection is disposed and an attempt to use it occurs.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidKeyException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidKeyException"/> class.
        /// </summary>
        public InvalidKeyException() : base("An item with the key failed to be found!")
        {
        }
    }
}
