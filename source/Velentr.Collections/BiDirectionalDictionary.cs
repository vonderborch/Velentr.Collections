using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Velentr.Collections;

/// <summary>
///     A dictionary that allows bi-directional lookup between keys and values.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
[DebuggerDisplay("Count = {Count}")]
public class BiDirectionalDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary where TKey : notnull
    where TValue : notnull
{
    [JsonIgnore] private readonly Dictionary<TKey, TValue> keyToValue;
    [JsonIgnore] private readonly ReaderWriterLockSlim lockSlim = new(LockRecursionPolicy.SupportsRecursion);
    [JsonIgnore] private readonly Dictionary<TValue, TKey> valueToKey;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}" /> class.
    /// </summary>
    public BiDirectionalDictionary() : this(0)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}" /> class with the specified
    ///     capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    public BiDirectionalDictionary(int capacity)
    {
        this.SyncRoot = new object();
        this.keyToValue = new Dictionary<TKey, TValue>(capacity);
        this.valueToKey = new Dictionary<TValue, TKey>(capacity);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BiDirectionalDictionary{TKey, TValue}" /> class with the specified
    ///     source dictionary.
    /// </summary>
    /// <param name="source">The source dictionary to initialize from.</param>
    [JsonConstructor]
    public BiDirectionalDictionary(IDictionary<TKey, TValue> source)
    {
        this.SyncRoot = new object();
        this.keyToValue = new Dictionary<TKey, TValue>(source.Count);
        this.valueToKey = new Dictionary<TValue, TKey>(source.Count);

        // Perform the population without acquiring locks multiple times
        foreach (KeyValuePair<TKey, TValue> kvp in source)
        {
            this.keyToValue.Add(kvp.Key, kvp.Value);
            this.valueToKey.Add(kvp.Value, kvp.Key);
        }
    }

    /// <summary>
    ///     Gets a read-only dictionary of keys to values.
    /// </summary>
    [JsonPropertyName("source")]
    public ReadOnlyDictionary<TKey, TValue> KeysToValues => new(this.keyToValue);

    /// <summary>
    ///     Gets a read-only dictionary of values to keys.
    /// </summary>
    [JsonIgnore]
    public ReadOnlyDictionary<TValue, TKey> ValuesToKeys => new(this.valueToKey);

    int ICollection.Count => this.Count;

    /// <summary>
    ///     Gets a value indicating whether the dictionary has a fixed size.
    /// </summary>
    public bool IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;

    /// <summary>
    ///     Gets a value indicating whether access to the dictionary is synchronized (thread-safe).
    /// </summary>
    public bool IsSynchronized => true;

    ICollection IDictionary.Keys => this.keyToValue.Keys;

    [field: JsonIgnore] public object SyncRoot { get; }

    ICollection IDictionary.Values => this.keyToValue.Values;

    /// <summary>
    ///     Adds a key-value pair to the dictionary using object types.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentException">Thrown if the key or value is of an invalid type.</exception>
    public void Add(object key, object? value)
    {
        if (key is TKey tKey && value is TValue tValue)
        {
            Add(tKey, tValue);
        }
        else
        {
            throw new ArgumentException("Invalid key or value type.");
        }
    }

    /// <summary>
    ///     Determines whether the dictionary contains the specified key or value.
    /// </summary>
    /// <param name="key">The key or value to locate.</param>
    /// <returns>True if the key or value exists; otherwise, false.</returns>
    public bool Contains(object key)
    {
        this.lockSlim.EnterReadLock();
        try
        {
            if (key is TKey tKey)
            {
                return this.keyToValue.ContainsKey(tKey);
            }

            if (key is TValue tValue)
            {
                return this.valueToKey.ContainsKey(tValue);
            }

            return false;
        }
        finally
        {
            this.lockSlim.ExitReadLock();
        }
    }

    /// <summary>
    ///     Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="index">The zero-based index in the array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown if the array is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the array is not one-dimensional or is too small.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    public void CopyTo(Array array, int index)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (array.Rank != 1)
        {
            throw new ArgumentException("Array must be one-dimensional.");
        }

        if (index < 0 || index >= array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (array.Length - index < this.keyToValue.Count)
        {
            throw new ArgumentException("The array is too small to copy the elements.");
        }

        foreach (KeyValuePair<TKey, TValue> kvp in this.keyToValue)
        {
            array.SetValue(new DictionaryEntry(kvp.Key, kvp.Value), index++);
        }
    }

    /// <summary>
    ///     Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An <see cref="IDictionaryEnumerator" /> for the dictionary.</returns>
    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return ((IDictionary)this.keyToValue).GetEnumerator();
    }

    /// <summary>
    ///     Gets or sets the value associated with the specified key or value using object types.
    /// </summary>
    /// <param name="key">The key or value to locate.</param>
    /// <returns>The associated value or key, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown if the key or value is of an invalid type.</exception>
    public object? this[object key]
    {
        get
        {
            if (key is TKey tKey1 && this.keyToValue.TryGetValue(tKey1, out TValue? value))
            {
                return value;
            }

            if (key is TValue tValue && this.valueToKey.TryGetValue(tValue, out TKey? tKey2))
            {
                return tKey2;
            }

            return null;
        }
        set
        {
            if (key is TKey tKey && value is TValue tValue)
            {
                this[tKey] = tValue;
            }
            else
            {
                throw new ArgumentException("Invalid key or value type.");
            }
        }
    }

    /// <summary>
    ///     Removes the key-value pair with the specified key or value using object types.
    /// </summary>
    /// <param name="key">The key or value to remove.</param>
    /// <exception cref="ArgumentException">Thrown if the key is of an invalid type.</exception>
    public void Remove(object key)
    {
        if (key is TKey tKey)
        {
            Remove(tKey);
        }
        else if (key is TValue tValue)
        {
            RemoveValue(tValue);
        }
        else
        {
            throw new ArgumentException("Invalid key type.");
        }
    }

    /// <summary>
    ///     Gets the number of key-value pairs in the dictionary.
    /// </summary>
    public int Count
    {
        get
        {
            this.lockSlim.EnterReadLock();
            try
            {
                return this.keyToValue.Count;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Gets the collection of keys in the dictionary.
    /// </summary>
    public ICollection<TKey> Keys
    {
        get
        {
            this.lockSlim.EnterReadLock();
            try
            {
                return this.keyToValue.Keys.ToArray();
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }
    }

    /// <summary>
    ///     Gets the collection of values in the dictionary.
    /// </summary>
    public ICollection<TValue> Values
    {
        get
        {
            this.lockSlim.EnterReadLock();
            try
            {
                return this.keyToValue.Values.ToArray();
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }
    }

    /// <summary>
    ///     Adds a key-value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key to add.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentException">Thrown if the key or value already exists in the dictionary.</exception>
    public void Add(TKey key, TValue value)
    {
        this.lockSlim.EnterWriteLock();
        try
        {
            if (this.keyToValue.ContainsKey(key) || this.valueToKey.ContainsKey(value))
            {
                throw new ArgumentException("Duplicate key or value.");
            }

            this.keyToValue[key] = value;
            this.valueToKey[value] = key;
        }
        finally
        {
            this.lockSlim.ExitWriteLock();
        }
    }

    /// <summary>
    ///     Adds a key-value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to add.</param>
    /// <exception cref="ArgumentException">Thrown if the key or value already exists in the dictionary.</exception>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    ///     Removes all key-value pairs from the dictionary.
    /// </summary>
    public void Clear()
    {
        this.lockSlim.EnterWriteLock();
        try
        {
            this.keyToValue.Clear();
            this.valueToKey.Clear();
        }
        finally
        {
            this.lockSlim.ExitWriteLock();
        }
    }

    /// <summary>
    ///     Determines whether the dictionary contains the specified key-value pair.
    /// </summary>
    /// <param name="item">The key-value pair to locate.</param>
    /// <returns>True if the key-value pair exists; otherwise, false.</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return this.keyToValue.TryGetValue(item.Key, out TValue? value) &&
               EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    /// <summary>
    ///     Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    public bool ContainsKey(TKey key)
    {
        this.lockSlim.EnterReadLock();
        try
        {
            return this.keyToValue.ContainsKey(key);
        }
        finally
        {
            this.lockSlim.ExitReadLock();
        }
    }

    /// <summary>
    ///     Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this.keyToValue).CopyTo(array, arrayIndex);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        this.lockSlim.EnterReadLock();
        try
        {
            // Return a copy to avoid enumeration issues if the collection changes
            return this.keyToValue.ToArray().AsEnumerable().GetEnumerator();
        }
        finally
        {
            this.lockSlim.ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
    }

    /// <summary>
    ///     Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>The value associated with the key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key does not exist in the dictionary.</exception>
    public TValue this[TKey key]
    {
        get
        {
            this.lockSlim.EnterReadLock();
            try
            {
                if (!this.keyToValue.TryGetValue(key, out TValue? value))
                {
                    throw new KeyNotFoundException();
                }

                return value;
            }
            finally
            {
                this.lockSlim.ExitReadLock();
            }
        }
        set
        {
            this.lockSlim.EnterUpgradeableReadLock();
            try
            {
                TValue? oldValue = default;
                var hasOldValue = false;

                // Check if the key exists
                if (this.keyToValue.TryGetValue(key, out oldValue))
                {
                    hasOldValue = true;
                }

                // Check if value already exists with a different key
                TKey? existingKey = default;
                var valueExists = this.valueToKey.TryGetValue(value, out existingKey);

                // Only enter write lock if we're actually changing things
                if (hasOldValue || valueExists)
                {
                    this.lockSlim.EnterWriteLock();
                    try
                    {
                        // Remove old mappings
                        if (hasOldValue)
                        {
                            this.valueToKey.Remove(oldValue!);
                        }

                        if (valueExists && !EqualityComparer<TKey>.Default.Equals(existingKey!, key))
                        {
                            this.keyToValue.Remove(existingKey!);
                        }

                        // Add new mappings
                        this.keyToValue[key] = value;
                        this.valueToKey[value] = key;
                    }
                    finally
                    {
                        this.lockSlim.ExitWriteLock();
                    }
                }
                else
                {
                    // Simple case - just add new mappings
                    this.lockSlim.EnterWriteLock();
                    try
                    {
                        this.keyToValue[key] = value;
                        this.valueToKey[value] = key;
                    }
                    finally
                    {
                        this.lockSlim.ExitWriteLock();
                    }
                }
            }
            finally
            {
                this.lockSlim.ExitUpgradeableReadLock();
            }
        }
    }

    /// <summary>
    ///     Removes the key-value pair with the specified key.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>True if the key was removed; otherwise, false.</returns>
    public bool Remove(TKey key)
    {
        this.lockSlim.EnterUpgradeableReadLock();
        try
        {
            if (!this.keyToValue.TryGetValue(key, out TValue? value))
            {
                return false;
            }

            this.lockSlim.EnterWriteLock();
            try
            {
                this.keyToValue.Remove(key);
                this.valueToKey.Remove(value);
                return true;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
        finally
        {
            this.lockSlim.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    ///     Removes the specified key-value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to remove.</param>
    /// <returns>True if the key-value pair was removed; otherwise, false.</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        lock (this.SyncRoot)
        {
            if (Contains(item))
            {
                return Remove(item.Key);
            }

            return false;
        }
    }

    /// <summary>
    ///     Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <param name="value">
    ///     When this method returns, contains the value associated with the key, if found; otherwise, the
    ///     default value.
    /// </param>
    /// <returns>True if the key was found; otherwise, false.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        this.lockSlim.EnterReadLock();
        try
        {
            return this.keyToValue.TryGetValue(key, out value);
        }
        finally
        {
            this.lockSlim.ExitReadLock();
        }
    }

    /// <summary>
    ///     Adds a range of key-value pairs to the dictionary.
    /// </summary>
    /// <param name="pairs">The key-value pairs to add.</param>
    /// <exception cref="ArgumentException">Thrown if any key or value already exists in the dictionary.</exception>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        if (pairs == null)
        {
            throw new ArgumentNullException(nameof(pairs));
        }

        // Make a single copy to check without locking
        KeyValuePair<TKey, TValue>[] pairsArray = pairs.ToArray();

        this.lockSlim.EnterWriteLock();
        try
        {
            // Pre-check all items to avoid partial additions
            foreach (KeyValuePair<TKey, TValue> pair in pairsArray)
            {
                if (this.keyToValue.ContainsKey(pair.Key) || this.valueToKey.ContainsKey(pair.Value))
                {
                    throw new ArgumentException($"Collection contains duplicate key {pair.Key} or value {pair.Value}.");
                }
            }

            // Now add all items
            foreach (KeyValuePair<TKey, TValue> pair in pairsArray)
            {
                this.keyToValue[pair.Key] = pair.Value;
                this.valueToKey[pair.Value] = pair.Key;
            }
        }
        finally
        {
            this.lockSlim.ExitWriteLock();
        }
    }

    /// <summary>
    ///     Determines whether the dictionary contains the specified value.
    /// </summary>
    /// <param name="value">The value to locate.</param>
    /// <returns>True if the value exists; otherwise, false.</returns>
    public bool ContainsValue(TValue value)
    {
        this.lockSlim.EnterReadLock();
        try
        {
            return this.valueToKey.ContainsKey(value);
        }
        finally
        {
            this.lockSlim.ExitReadLock();
        }
    }

    /// <summary>
    ///     Releases resources used by the BiDirectionalDictionary.
    /// </summary>
    public void Dispose()
    {
        this.lockSlim.Dispose();
    }

    /// <summary>
    ///     Gets the key associated with the specified value.
    /// </summary>
    /// <param name="value">The value to locate.</param>
    /// <returns>The key associated with the value.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the value does not exist in the dictionary.</exception>
    public TKey GetKey(TValue value)
    {
        this.lockSlim.EnterReadLock();
        try
        {
            if (!this.valueToKey.TryGetValue(value, out TKey? key))
            {
                throw new KeyNotFoundException();
            }

            return key;
        }
        finally
        {
            this.lockSlim.ExitReadLock();
        }
    }

    /// <summary>
    ///     Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>The value associated with the key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key does not exist in the dictionary.</exception>
    public TValue GetValue(TKey key)
    {
        lock (this.SyncRoot)
        {
            if (!this.keyToValue.TryGetValue(key, out TValue? value))
            {
                throw new KeyNotFoundException();
            }

            return value;
        }
    }

    /// <summary>
    ///     Removes the key-value pair with the specified value.
    /// </summary>
    /// <param name="value">The value to remove.</param>
    /// <returns>True if the value was removed; otherwise, false.</returns>
    public bool RemoveValue(TValue value)
    {
        this.lockSlim.EnterUpgradeableReadLock();
        try
        {
            if (!this.valueToKey.TryGetValue(value, out TKey? key))
            {
                return false;
            }

            this.lockSlim.EnterWriteLock();
            try
            {
                this.valueToKey.Remove(value);
                this.keyToValue.Remove(key);
                return true;
            }
            finally
            {
                this.lockSlim.ExitWriteLock();
            }
        }
        finally
        {
            this.lockSlim.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    ///     Attempts to get the key associated with the specified value.
    /// </summary>
    /// <param name="value">The value to locate.</param>
    /// <param name="key">
    ///     When this method returns, contains the key associated with the value, if found; otherwise, the
    ///     default value.
    /// </param>
    /// <returns>True if the value was found; otherwise, false.</returns>
    public bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey key)
    {
        this.lockSlim.EnterReadLock();
        try
        {
            return this.valueToKey.TryGetValue(value, out key);
        }
        finally
        {
            this.lockSlim.ExitReadLock();
        }
    }
}
