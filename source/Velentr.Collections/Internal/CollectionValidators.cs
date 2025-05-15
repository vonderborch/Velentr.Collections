using System.Runtime.CompilerServices;

namespace Velentr.Collections.Internal;

public static class CollectionValidators
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ValidateCollectionState(long startingVersion, long currentVersion)
    {
        if (startingVersion != currentVersion)
        {
            throw new InvalidOperationException("The collection has been modified.");
        }
    }
}
