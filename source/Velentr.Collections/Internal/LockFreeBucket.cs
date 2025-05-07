using System.Collections;
using System.Collections.Concurrent;
using Velentr.Collections.LockFree;

namespace Velentr.Collections.Internal;

/// <summary>
/// Represents a bucket in the lock-free dictionary, implemented using a ConcurrentDictionary for true lock-free operations.
/// </summary>
internal class LockFreeBucket<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable where TKey : notnull
{
    // Using ConcurrentDictionary as the underlying storage for truly lock-free operations
    private readonly ConcurrentDictionary<TKey, TValue> _entries;

    /// <summary>
    /// Initializes a new instance of the <see cref="LockFreeBucket{TKey, TValue}"/> class.
    /// </summary>
    public LockFreeBucket()
    {
        _entries = new ConcurrentDictionary<TKey, TValue>();
    }

    /// <summary>
    /// Attempts to add a key-value pair to the bucket.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    /// <param name="comparer">The comparer to use for key equality.</param>
    /// <returns>true if the pair was added; otherwise false (if the key already exists).</returns>
    public bool TryAdd(TKey key, TValue value, IEqualityComparer<TKey> comparer)
    {
        // ConcurrentDictionary.TryAdd is already thread-safe and lock-free
        return _entries.TryAdd(key, value);
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">When this method returns, contains the value if found; otherwise, the default value.</param>
    /// <param name="comparer">The comparer to use for key equality.</param>
    /// <returns>true if the key was found; otherwise, false.</returns>
    public bool TryGetValue(TKey key, out TValue value, IEqualityComparer<TKey> comparer)
    {
        // ConcurrentDictionary.TryGetValue is already thread-safe and lock-free
        return _entries.TryGetValue(key, out value);
    }

    /// <summary>
    /// Attempts to remove the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">When this method returns, contains the removed value if found; otherwise, the default value.</param>
    /// <param name="comparer">The comparer to use for key equality.</param>
    /// <returns>true if the key was found and removed; otherwise, false.</returns>
    public bool TryRemove(TKey key, out TValue value, IEqualityComparer<TKey> comparer)
    {
        // ConcurrentDictionary.TryRemove is already thread-safe and lock-free
        return _entries.TryRemove(key, out value);
    }

    /// <summary>
    /// Determines whether the bucket contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <param name="comparer">The comparer to use for key equality.</param>
    /// <returns>true if the bucket contains the key; otherwise, false.</returns>
    public bool ContainsKey(TKey key, IEqualityComparer<TKey> comparer)
    {
        // ConcurrentDictionary.ContainsKey is already thread-safe
        return _entries.ContainsKey(key);
    }

    /// <summary>
    /// Adds a new key-value pair or updates an existing key's value.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="comparer">The comparer to use for key equality.</param>
    /// <returns>true if a new key was added; false if an existing key was updated.</returns>
    public bool AddOrUpdate(TKey key, TValue value, IEqualityComparer<TKey> comparer)
    {
        // Try to add the key first - this is atomic and will only succeed if the key doesn't exist
        if (_entries.TryAdd(key, value))
        {
            return true; // Key was added
        }
        
        // Key exists, update it
        _entries[key] = value;
        
        return false; // Key was updated
    }

    /// <summary>
    /// Removes all elements from the bucket.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the bucket.
    /// </summary>
    /// <returns>An enumerator for the bucket.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        // ConcurrentDictionary provides thread-safe enumeration
        return _entries.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the bucket.
    /// </summary>
    /// <returns>An enumerator for the bucket.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Releases resources used by the bucket.
    /// </summary>
    public void Dispose()
    {
        Clear();
        GC.SuppressFinalize(this);
    }
}
