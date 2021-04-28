using System;

namespace Velentr.Collections.Helpers
{
    public static class Validation
    {
        /// <summary>
        /// Nots the null or empty check.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void NotNullOrEmptyCheck(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{parameterName} can not be null or empty.");
            }
        }

        /// <summary>
        /// Nots the null or white space check.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="ArgumentException"></exception>
        public static void NotNullOrWhiteSpaceCheck(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} can not be null, empty, or contain only white space.");
            }
        }

        /// <summary>
        /// Validates the range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <exception cref="ArgumentOutOfRangeException">The parameter [{parameterName}] is out of the range (min: {minValue}, max:{maxValue})!</exception>
        public static void ValidateRange(int value, string parameterName, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            if (minValue > value || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"The parameter [{parameterName}] is out of the range (min: {minValue}, max:{maxValue})!");
            }
        }

        /// <summary>
        /// Validates the range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <exception cref="ArgumentOutOfRangeException">The parameter [{parameterName}] is out of the range (min: {minValue}, max:{maxValue})!</exception>
        public static void ValidateRange(long value, string parameterName, long minValue = long.MinValue, long maxValue = long.MaxValue)
        {
            if (minValue > value || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"The parameter [{parameterName}] is out of the range (min: {minValue}, max:{maxValue})!");
            }
        }
    }
}