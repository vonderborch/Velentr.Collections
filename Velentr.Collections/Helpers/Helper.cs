using System;
using System.Linq;

namespace Collections.Net.Helpers
{
    public static class Helper
    {

        /// <summary>
        /// Disposes an object if possible
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="obj">The object to dispose of.</param>
        public static void DisposeIfPossible<T>(T obj)
        {
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
