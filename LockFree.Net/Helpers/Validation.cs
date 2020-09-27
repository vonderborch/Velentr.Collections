// ***********************************************************************
// Assembly         : LockFree.Net
// Component        : Validation.cs
// Author           : Christian Webber
// Created          : 2020-09-27
//
// Version          : 1.0.0
// Last Modified By : Christian Webber
// Last Modified On : 2020-09-27
// ***********************************************************************
// <copyright file="Validation.cs">
//     Copyright © 2020
// </copyright>
// <summary>
//     A collection of various Atomic operations
// </summary>
//
// Changelog:
//            - 1.0.0 (2020-09-27) - Initial commit.
// ***********************************************************************

using System;

namespace LockFree.Net.Helpers
{
    public static class Validation
    {
        public static void ValidateRange(int value, string parameterName, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            if (minValue > value || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"The parameter [{parameterName}] is out of the range (min: {minValue}, max:{maxValue})!");
            }
        }

        public static void ValidateRange(long value, string parameterName, long minValue = long.MinValue, long maxValue = long.MaxValue)
        {
            if (minValue > value || value > maxValue)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"The parameter [{parameterName}] is out of the range (min: {minValue}, max:{maxValue})!");
            }
        }

        public static void NotNullOrWhiteSpaceCheck(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} can not be null, empty, or contain only white space.");
            }
        }

        public static void NotNullOrEmptyCheck(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"{parameterName} can not be null or empty.");
            }
        }
    }
}
