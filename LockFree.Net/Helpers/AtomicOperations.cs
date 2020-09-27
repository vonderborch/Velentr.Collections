// ***********************************************************************
// Assembly         : LockFree.Net
// Component        : AtomicOperations.cs
// Author           : Christian Webber
// Created          : 2020-09-27
//
// Version          : 1.0.0
// Last Modified By : Christian Webber
// Last Modified On : 2020-09-27
// ***********************************************************************
// <copyright file="AtomicOperations.cs">
//     Copyright © 2020
// </copyright>
// <summary>
//     A collection of various Atomic operations
// </summary>
//
// Changelog:
//            - 1.0.0 (2020-09-27) - Initial commit.
// ***********************************************************************

using System.Threading;

namespace LockFree.Net.Helpers
{
    public static class AtomicOperations
    {
        public static bool CAS<T>(ref T location, T newValue, T comparand) where T : class
        {
            return comparand == Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        public static bool CAS(ref int location, int newValue, int comparand)
        {
            return comparand == Interlocked.CompareExchange(ref location, newValue, comparand);
        }

        public static bool CAS(ref long location, long newValue, long comparand)
        {
            return comparand == Interlocked.CompareExchange(ref location, newValue, comparand);
        }
    }
}
