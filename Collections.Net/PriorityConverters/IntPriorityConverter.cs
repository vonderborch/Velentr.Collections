using System.Collections.Generic;

namespace Collections.Net.PriorityConverters
{
    /// <summary>
    /// Defines an int priority converter for Priority Queues.
    /// </summary>
    /// <seealso cref="Collections.Net.PriorityConverters.PriorityConverter{System.Int32}" />
    public class IntPriorityConverter : PriorityConverter<int>
    {

        /// <summary>
        /// Generates the priority converter options.
        /// </summary>
        /// <returns>The number of options for this Converter.</returns>
        public override int GenerateOptions()
        {
            _options = new Dictionary<int, int>()
            {
                {0, 0},
                {1, 1},
                {2, 2},
                {3, 3},
                {4, 4},
                {5, 5},
                {6, 6},
            };

            return _options.Count;
        }
    }
}
