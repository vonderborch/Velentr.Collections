using System.Threading;

namespace Collections.Net.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class Guard
    {
        /// <summary>
        /// The false
        /// </summary>
        public const int FALSE = 0;

        /// <summary>
        /// The true
        /// </summary>
        private const int TRUE = 1;

        /// <summary>
        /// The current state
        /// </summary>
        private int _state = FALSE;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Guard"/> is checked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if checked; otherwise, <c>false</c>.
        /// </value>
        public bool Check => _state == TRUE;

        /// <summary>
        /// Gets a value indicating whether [check set].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [check set]; otherwise, <c>false</c>.
        /// </value>
        public bool CheckSet => Interlocked.Exchange(ref _state, TRUE) == FALSE;

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Interlocked.Exchange(ref _state, FALSE);
        }
    }
}
