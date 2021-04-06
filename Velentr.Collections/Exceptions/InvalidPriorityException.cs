using System;

namespace Collections.Net.Exceptions
{
    /// <summary>
    /// Defines an Exception that occurs with Priority Queues when the requested priority is invalid.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class InvalidPriorityException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPriorityException"/> class.
        /// </summary>
        public InvalidPriorityException() : base("Priority must be set to a valid priority!")
        {
        }
    }
}
