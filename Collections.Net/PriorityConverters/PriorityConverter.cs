using System.Collections.Generic;
using Collections.Net.Collections.LockFree;
using Collections.Net.Exceptions;

namespace Collections.Net.PriorityConverters
{
    /// <summary>
    /// Defines a priority converter for Priority Queues.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PriorityConverter<T>
    {

        /// <summary>
        /// The options
        /// </summary>
        protected Dictionary<T, int> _options;

        /// <summary>
        /// Gets the option count.
        /// </summary>
        /// <value>
        /// The option count.
        /// </value>
        public int OptionCount => _options?.Count ?? GenerateOptions();

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public Dictionary<T, int> Options
        {
            get
            {
                if (_options == null)
                {
                    GenerateOptions();
                }

                return _options;
            }
        }

        /// <summary>
        /// Converts the specified priority to the internal priority value.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <returns>The int representation of the requested priority</returns>
        public int Convert(T priority)
        {
            foreach (var option in _options)
            {
                if (option.Key.Equals(priority))
                {
                    return option.Value;
                }
            }

            throw new InvalidPriorityException();
        }

        /// <summary>
        /// Converts the specified priority to from internal priority value.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        public T ConvertFromInt(int priority)
        {
            foreach (var option in _options)
            {
                if (option.Value == priority)
                {
                    return option.Key;
                }
            }

            throw new InvalidPriorityException();
        }

        /// <summary>
        /// Generates the priority converter options.
        /// </summary>
        /// <returns>The number of options for this Converter.</returns>
        public abstract int GenerateOptions();
    }
}
