using System;
using System.Collections.Generic;

namespace Collections.Net.PriorityConverters
{
    /// <summary>
    /// Defines a QueuePriority priority converter for Priority Queues.
    /// </summary>
    /// <seealso cref="QueuePriority" />
    public class QueuePriorityConverter : PriorityConverter<QueuePriority>
    {

        /// <summary>
        /// Generates the priority converter options.
        /// </summary>
        /// <returns>The number of options for this Converter.</returns>
        public override int GenerateOptions()
        {
            _options = new Dictionary<QueuePriority, int>();
            var rawOptions = Enum.GetValues(typeof(QueuePriority));
            for (var i = 0; i < rawOptions.Length; i++)
                _options.Add((QueuePriority)rawOptions.GetValue(i), (int)rawOptions.GetValue(i));

            return _options.Count;
        }
    }
}
