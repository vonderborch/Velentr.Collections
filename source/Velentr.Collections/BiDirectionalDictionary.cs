using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Velentr.Collections;

[DebuggerDisplay("Count = {Count}")]
public class BiDirectionalDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary where TKey : notnull
    where TValue : notnull
{
    [JsonIgnore] private readonly Dictionary<TKey, TValue> keyToValue;

    [JsonIgnore] private readonly Dictionary<TValue, TKey> valueToKey;

    public BiDirectionalDictionary()
    {
        this.SyncRoot = new object();
        this.keyToValue = new Dictionary<TKey, TValue>();
        this.valueToKey = new Dictionary<TValue, TKey>();
    }

    [JsonConstructor]
    public BiDirectionalDictionary(IDictionary<TKey, TValue> source)
    {
        this.SyncRoot = new object();
        this.keyToValue = new Dictionary<TKey, TValue>(source.Count);
        this.valueToKey = new Dictionary<TValue, TKey>(source.Count);
        foreach (KeyValuePair<TKey, TValue> kvp in source)
        {
            this.keyToValue.Add(kvp.Key, kvp.Value);
            this.valueToKey.Add(kvp.Value, kvp.Key);
        }
    }


    [JsonPropertyName("source")] public ReadOnlyDictionary<TKey, TValue> KeysToValues => new(this.keyToValue);

    [JsonIgnore] public ReadOnlyDictionary<TValue, TKey> ValuesToKeys => new(this.valueToKey);

    int ICollection.Count => this.keyToValue.Count;

    public bool IsFixedSize => false;

    bool IDictionary.IsReadOnly => false;
    public bool IsSynchronized => false;

    ICollection IDictionary.Keys => this.keyToValue.Keys;
    
    [field: JsonIgnore] public object SyncRoot { get; }

    ICollection IDictionary.Values => this.keyToValue.Values;

    // Explicit interface implementations for IDictionary
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

    public bool Contains(object key)
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

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return ((IDictionary)this.keyToValue).GetEnumerator();
    }

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

    public int Count => this.keyToValue.Count;

    public bool IsReadOnly => false;

    // Explicit interface implementations for IDictionary<TKey, TValue>
    public ICollection<TKey> Keys => this.keyToValue.Keys;

    public ICollection<TValue> Values => this.keyToValue.Values;

    public void Add(TKey key, TValue value)
    {
        lock (this.SyncRoot)
        {
            if (this.keyToValue.ContainsKey(key) || this.valueToKey.ContainsKey(value))
            {
                throw new ArgumentException("Duplicate key or value.");
            }

            this.keyToValue[key] = value;
            this.valueToKey[value] = key;
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        lock (this.SyncRoot)
        {
            this.keyToValue.Clear();
            this.valueToKey.Clear();
        }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return this.keyToValue.TryGetValue(item.Key, out TValue? value) &&
               EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    public bool ContainsKey(TKey key)
    {
        lock (this.SyncRoot)
        {
            return this.keyToValue.ContainsKey(key);
        }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this.keyToValue).CopyTo(array, arrayIndex);
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return this.keyToValue.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.keyToValue.GetEnumerator();
    }

    public TValue this[TKey key]
    {
        get
        {
            if (!this.keyToValue.TryGetValue(key, out TValue? value))
            {
                throw new KeyNotFoundException();
            }

            return value;
        }
        set
        {
            lock (this.SyncRoot)
            {
                if (this.keyToValue.TryGetValue(key, out TValue? existingValue))
                {
                    this.valueToKey.Remove(existingValue);
                }

                if (this.valueToKey.TryGetValue(value, out TKey? existingKey))
                {
                    this.keyToValue.Remove(existingKey);
                }

                this.keyToValue[key] = value;
                this.valueToKey[value] = key;
            }
        }
    }

    public bool Remove(TKey key)
    {
        lock (this.SyncRoot)
        {
            if (!this.keyToValue.TryGetValue(key, out TValue? value))
            {
                return false;
            }

            this.keyToValue.Remove(key);
            this.valueToKey.Remove(value);
            return true;
        }
    }

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

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return this.keyToValue.TryGetValue(key, out value);
    }

    public bool ContainsValue(TValue value)
    {
        lock (this.SyncRoot)
        {
            return this.valueToKey.ContainsKey(value);
        }
    }

    public TKey GetKey(TValue value)
    {
        lock (this.SyncRoot)
        {
            if (!this.valueToKey.TryGetValue(value, out TKey? key))
            {
                throw new KeyNotFoundException();
            }

            return key;
        }
    }

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

    public bool RemoveValue(TValue value)
    {
        lock (this.SyncRoot)
        {
            if (!this.valueToKey.TryGetValue(value, out TKey? key))
            {
                return false;
            }

            this.valueToKey.Remove(value);
            this.keyToValue.Remove(key);
            return true;
        }
    }

    public bool TryGetKey(TValue value, [MaybeNullWhen(false)] out TKey key)
    {
        return this.valueToKey.TryGetValue(value, out key);
    }
}
