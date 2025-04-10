using System.Collections;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Velentr.Collections.CollectionActions;

namespace Velentr.Collections;

/// <summary>
/// A dictionary with a maximum size limit. When the limit is reached, it performs a specified action to handle the overflow.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
[DebuggerDisplay("Count = {Count}, MaxSize = {MaxSize}")]
public class SizeLimitedOrderedDictionary<TKey, TValue> : IOrderedDictionary, IDictionary<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// The internal dictionary.
    /// </summary>
    private readonly OrderedDictionary internalDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeLimitedOrderedDictionary{TKey, TValue}"/> class with a specified maximum size and overflow action.
    /// </summary>
    /// <param name="maxSize">The maximum size of the dictionary.</param>
    /// <param name="actionWhenFull">The action to perform when the dictionary exceeds its maximum size.</param>
    public SizeLimitedOrderedDictionary(int maxSize, SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        this.internalDictionary = new OrderedDictionary(maxSize);
        this.MaxSize = maxSize;
        this.ActionWhenFull = actionWhenFull;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeLimitedOrderedDictionary{TKey, TValue}"/> class with a specified capacity, maximum size, and overflow action.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <param name="maxSize">The maximum size of the dictionary.</param>
    /// <param name="actionWhenFull">The action to perform when the dictionary exceeds its maximum size.</param>
    public SizeLimitedOrderedDictionary(int capacity, int maxSize, SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        this.internalDictionary = new OrderedDictionary(capacity);
        this.MaxSize = maxSize;
        this.ActionWhenFull = actionWhenFull;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SizeLimitedOrderedDictionary{TKey, TValue}"/> class from an existing immutable sorted dictionary.
    /// </summary>
    /// <param name="dictionary">The source immutable sorted dictionary.</param>
    /// <param name="maxSize">The maximum size of the dictionary.</param>
    /// <param name="actionWhenFull">The action to perform when the dictionary exceeds its maximum size.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the dictionary size exceeds the maximum size.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the dictionary is null.</exception>
    public SizeLimitedOrderedDictionary(ImmutableSortedDictionary<TKey, TValue> dictionary, int maxSize, SizeLimitedCollectionFullAction actionWhenFull = SizeLimitedCollectionFullAction.PopOldestItem)
    {
        if (maxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than 0.");
        }
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary), "Dictionary cannot be null.");
        }
        if (dictionary.Count > maxSize)
        {
            throw new ArgumentOutOfRangeException(nameof(dictionary), "Dictionary size exceeds maximum size.");
        }

        this.internalDictionary = new OrderedDictionary(maxSize);
        this.MaxSize = maxSize;
        this.ActionWhenFull = actionWhenFull;

        foreach (var entry in dictionary)
        {
            this.internalDictionary.Add(entry.Key, entry.Value);
        }
    }

    /// <summary>
    /// Gets the internal dictionary as an immutable sorted dictionary.
    /// </summary>
    [JsonPropertyName("dictionary")]
    public ImmutableSortedDictionary<TKey, TValue> InternalDictionary
    {
        get
        {
            var output = new SortedDictionary<TKey, TValue>();
            foreach (DictionaryEntry entry in this.internalDictionary)
            {
                output.Add((TKey)entry.Key, (TValue)entry.Value);
            }
            return output.ToImmutableSortedDictionary();
        }
    }

    /// <summary>
    /// Gets the maximum size of the dictionary.
    /// </summary>
    [JsonPropertyName("maxSize")]
    public int MaxSize { get; private set; }

    /// <summary>
    /// Gets or sets the action to perform when the dictionary exceeds its maximum size.
    /// </summary>
    [JsonPropertyName("actionWhenFull")]
    public SizeLimitedCollectionFullAction ActionWhenFull { get; set; }

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array to copy elements into.</param>
    /// <param name="index">The zero-based index in the array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown if the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the array is not large enough to contain the elements.</exception>
    public void CopyTo(Array array, int index)
    {
        foreach (DictionaryEntry entry in this.internalDictionary)
        {
            array.SetValue(entry, index++);
        }
    }

    /// <summary>
    /// Removes the specified key-value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (Contains(item))
        {
            this.internalDictionary.Remove(item.Key);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the number of elements contained in the dictionary.
    /// </summary>
    public int Count => this.internalDictionary.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => this.internalDictionary.IsReadOnly;

    public bool IsSynchronized => false;

    public object SyncRoot => throw new NotImplementedException();

    /// <summary>
    /// Adds a key-value pair to the dictionary and returns the removed item if the dictionary exceeds its maximum size.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <returns>The removed item if the dictionary exceeds its maximum size; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the key or value is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the dictionary is full and no valid action is defined.</exception>
    public TValue? AddAndReturn(object key, object? value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Key cannot be null.");
        }
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }
        TValue? entry = default;
        if (this.internalDictionary.Count >= this.MaxSize)
        {
            entry = HandleOverflow();
        }
        this.internalDictionary.Add(key, value);
        return entry;
    }

    /// <summary>
    /// Adds a key-value pair to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if the dictionary is full and no valid action is defined.</exception>
    public void Add(TKey key, TValue value)
    {
        if (this.internalDictionary.Count >= this.MaxSize)
        {
            HandleOverflow();
        }
        this.internalDictionary.Add(key, value);
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains the specified key; otherwise, false.</returns>
    public bool ContainsKey(TKey key)
    {
        return this.internalDictionary.Contains(key);
    }

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>True if the element was successfully removed; otherwise, false.</returns>
    public bool Remove(TKey key)
    {
        if (this.internalDictionary.Contains(key))
        {
            this.internalDictionary.Remove(key);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (this.internalDictionary.Contains(key))
        {
            value = (TValue)this.internalDictionary[key]!;
            return true;
        }
        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => (TValue)this.internalDictionary[key: key];
        set => this.internalDictionary[key: key] = value;
    }

    /// <summary>
    /// Handles overflow when the dictionary exceeds its maximum size.
    /// </summary>
    /// <returns>The removed item if applicable; otherwise, null.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no valid action is defined for handling overflow.</exception>
    private TValue? HandleOverflow()
    {
        TValue? entry;
        if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopOldestItem)
        {
            entry = (TValue?)this.internalDictionary[index: 0];
            this.internalDictionary.RemoveAt(0);
        }
        else if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopNewestItem)
        {
            entry = (TValue?)this.internalDictionary[index: this.internalDictionary.Count - 1];
            this.internalDictionary.RemoveAt(this.internalDictionary.Count - 1);
        }
        else
        {
            throw new InvalidOperationException("The dictionary is full and no valid action is defined.");
        }

        return entry;
    }

    /// <summary>
    /// Copies the elements of the dictionary to a strongly-typed array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array to copy elements into.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    /// <exception cref="ArgumentNullException">Thrown if the array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the arrayIndex is less than 0.</exception>
    /// <exception cref="ArgumentException">Thrown if the array is not large enough to contain the elements.</exception>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (DictionaryEntry entry in this.internalDictionary)
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
        }
    }

    public void Add(object key, object? value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key), "Key cannot be null.");
        }
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        }
        if (this.internalDictionary.Count >= this.MaxSize)
        {
            HandleOverflow();
        }
        this.internalDictionary.Add(key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (item.Key == null)
        {
            throw new ArgumentNullException(nameof(item.Key), "Key cannot be null.");
        }
        if (item.Value == null)
        {
            throw new ArgumentNullException(nameof(item.Value), "Value cannot be null.");
        }
        if (this.internalDictionary.Count >= this.MaxSize)
        {
            HandleOverflow();
        }
        this.internalDictionary.Add(item.Key, item.Value);
    }

    /// <summary>
    /// Clears all elements from the dictionary.
    /// </summary>
    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
        this.internalDictionary.Clear();
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key-value pair.
    /// </summary>
    /// <param name="item">The key-value pair to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains the specified key-value pair; otherwise, false.</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return this.internalDictionary.Contains(item.Key) &&
               EqualityComparer<TValue>.Default.Equals((TValue)this.internalDictionary[item.Key]!, item.Value);
    }

    void IDictionary.Clear()
    {
        this.internalDictionary.Clear();
    }

    public bool Contains(object key)
    {
        return this.internalDictionary.Contains(key);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        foreach (DictionaryEntry entry in this.internalDictionary)
        {
            yield return new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
        }
    }

    IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
    {
        return this.internalDictionary.GetEnumerator();
    }

    /// <summary>
    /// Inserts a key-value pair into the dictionary at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the key-value pair should be inserted.</param>
    /// <param name="key">The key of the element to insert.</param>
    /// <param name="value">The value of the element to insert.</param>
    /// <exception cref="InvalidOperationException">Thrown if the dictionary is full and no valid action is defined.</exception>
    public void Insert(int index, object key, object? value)
    {
        if (this.internalDictionary.Count >= this.MaxSize)
        {
            HandleOverflow();
        }
        this.internalDictionary.Insert(index, key, value);
    }

    /// <summary>
    /// Removes the element at the specified index from the dictionary.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    public void RemoveAt(int index)
    {
        this.internalDictionary.RemoveAt(index);
    }

    /// <summary>
    /// Gets or sets the value at the specified index in the dictionary.
    /// </summary>
    /// <param name="index">The zero-based index of the value to get or set.</param>
    /// <returns>The value at the specified index.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the value is null when setting.</exception>
    public object? this[int index]
    {
        get => this.internalDictionary[index];
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }
            this.internalDictionary[index] = value;
        }
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return this.internalDictionary.GetEnumerator();
    }

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    public void Remove(object key)
    {
        this.internalDictionary.Remove(key);
    }

    public bool IsFixedSize => true;

    bool IDictionary.IsReadOnly => false;

    /// <summary>
    /// Gets or sets the value associated with the specified key in the dictionary.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the key or value is null when setting.</exception>
    public object? this[object key]
    {
        get
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Key cannot be null.");
            }
            return this.internalDictionary[key];
        }
        set
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key), "Key cannot be null.");
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            }
            if (this.internalDictionary.Contains(key))
            {
                this.internalDictionary[key] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<TKey> Keys => this.internalDictionary.Keys.Cast<TKey>().ToList();

    ICollection IDictionary.Values => this.internalDictionary.Values;

    ICollection IDictionary.Keys => this.internalDictionary.Keys;

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public ICollection<TValue> Values => this.internalDictionary.Values.Cast<TValue>().ToList();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
    }

    /// <summary>
    /// Changes the maximum size of the dictionary and removes excess elements if necessary.
    /// </summary>
    /// <param name="newMaxSize">The new maximum size of the dictionary.</param>
    /// <returns>A list of elements that were removed to fit the new size.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when newMaxSize is less than 1.</exception>
    public List<TValue> ChangeMaxSize(int newMaxSize)
    {
        if (newMaxSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(newMaxSize), "New max size must be greater than or equal to 1.");
        }
        if (newMaxSize >= this.internalDictionary.Count)
        {
            this.MaxSize = newMaxSize;
            return new();
        }
        
        this.MaxSize = newMaxSize;
        List<TValue> poppedItems = new();
        int remainingItemsToPop = this.internalDictionary.Count - newMaxSize;
        int currentI = 0;
        int incrementor = 0;
        if (this.ActionWhenFull == SizeLimitedCollectionFullAction.PopNewestItem)
        {
            currentI = this.internalDictionary.Count - 1;
            incrementor = -1;
        }

        while (remainingItemsToPop > 0)
        {
            poppedItems.Add((TValue)this.internalDictionary[index: currentI]);
            this.internalDictionary.RemoveAt(currentI);
            
            currentI += incrementor;
            remainingItemsToPop--;
        }

        return poppedItems;
    }
}
