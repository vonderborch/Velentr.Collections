using System.Collections.Generic;

namespace Velentr.Collections.PriorityConverters
{
    /// <summary>
    /// Defines a string priority converter for Priority Queues.
    /// </summary>
    /// <seealso cref="Collections.Net.PriorityConverters.PriorityConverter{System.string}" />
    public class StringPriorityConverter : PriorityConverter<string>
    {

        /// <summary>
        /// Generates the priority converter options.
        /// </summary>
        /// <returns>The number of options for this Converter.</returns>
        public override int GenerateOptions()
        {
            _options = new Dictionary<string, int>()
            {
                {"HIGH", 0},
                {"MEDIUM", 1},
                {"NORMAL", 2},
                {"LOW", 3},
            };

            return _options.Count;
        }
    }
}
