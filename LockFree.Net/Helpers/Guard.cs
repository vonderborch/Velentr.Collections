// ***********************************************************************
// Assembly         : LockFree.Net
// Component        : Guard.cs
// Author           : Christian Webber
// Created          : 2020-09-27
//
// Version          : 1.0.0
// Last Modified By : Christian Webber
// Last Modified On : 2020-09-27
// ***********************************************************************
// <copyright file="Guard.cs">
//     Copyright © 2020
// </copyright>
// <summary>
//     A thread-safe boolean flag
// </summary>
//
// Changelog:
//            - 1.0.0 (2020-09-27) - Initial commit.
// ***********************************************************************

using System.Threading;

namespace LockFree.Net.Helpers
{
    public class Guard
    {
        public const int FALSE = 0;

        private const int TRUE = 1;

        private int _state = FALSE;

        public bool Check => _state == TRUE;

        public bool CheckSet => Interlocked.Exchange(ref _state, TRUE) == FALSE;

        public void Reset()
        {
            Interlocked.Exchange(ref _state, FALSE);
        }
    }
}
