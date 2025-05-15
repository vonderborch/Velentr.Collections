namespace Velentr.Collections.Internal;

/// <summary>
///     Internal helper methods for the Velentr.Collections library.
/// </summary>
internal static class Helpers
{
    /// <summary>
    ///     Performs an atomic compare-and-swap operation on a node reference.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the node.</typeparam>
    /// <param name="valueReference">A reference to the node that may be updated.</param>
    /// <param name="newValue">The new value to assign if the comparison succeeds.</param>
    /// <param name="expectedValueAtValueReference">The value that the reference is expected to currently hold.</param>
    /// <returns>
    ///     <c>true</c> if the comparison was successful and the swap was performed;
    ///     <c>false</c> if the reference did not contain the expected value and no swap occurred.
    /// </returns>
    /// <remarks>
    ///     This method is thread-safe and can be used in concurrent operations.
    ///     It's typically used in lock-free data structures to safely update references.
    /// </remarks>
    internal static bool CompareAndSwap<T>(ref Node<T>? valueReference, Node<T> newValue,
        Node<T>? expectedValueAtValueReference)
    {
        Node<T>? originalValue =
            Interlocked.CompareExchange(ref valueReference, newValue, expectedValueAtValueReference);
        return EqualityComparer<Node<T>>.Default.Equals(originalValue, expectedValueAtValueReference);
    }

    internal static int Decrement(ref int variable, int minimum)
    {
        var originalValue = Interlocked.Decrement(ref variable);
        if (originalValue < minimum)
        {
            Interlocked.Increment(ref variable);
            return minimum;
        }

        return originalValue;
    }
}
