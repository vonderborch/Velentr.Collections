using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Velentr.Collections.Json;
using Velentr.Core.Json;

namespace Velentr.Collections;

/// <summary>
///     A dictionary that provides a default value or factory for missing keys.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
[JsonConverter(typeof(DefaultDictionaryJsonConverterFactory))]
[DebuggerDisplay("Count = {Count}")]
public class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    ///     The underlying dictionary used by the DefaultDictionary class to store key-value pairs.
    ///     This field is private, read-only, and not serialized due to the [JsonIgnore] attribute.
    /// </summary>
    [JsonIgnore] private readonly Dictionary<TKey, TValue> baseDictionary;

    /// <summary>
    ///     The default value to use when a key is missing.
    /// </summary>
    [JsonIgnore] private readonly TValue? defaultValue;

    /// <summary>
    ///     The compiled default value factory method.
    /// </summary>
    [JsonIgnore] private readonly Func<TValue>? defaultValueFactory;

    /// <summary>
    ///     The expression for the default value factory.
    /// </summary>
    [JsonIgnore] private readonly Expression<Func<TValue>>? defaultValueFactoryExpression;

    /// <summary>
    ///     Indicates whether to set the default value before setting a new value.
    /// </summary>
    public bool SetDefaultValueBeforeSettingValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}" /> class with a default value.
    /// </summary>
    /// <param name="defaultValue">The default value to use for missing keys.</param>
    /// <param name="setDefaultValueBeforeSettingValue">Whether to set the default value before setting a new value.</param>
    public DefaultDictionary(TValue defaultValue, bool setDefaultValueBeforeSettingValue = false)
    {
        this.baseDictionary = new Dictionary<TKey, TValue>();
        this.defaultValue = defaultValue;
        this.SetDefaultValueBeforeSettingValue = setDefaultValueBeforeSettingValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}" /> class with a specified capacity
    ///     and default value.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <param name="defaultValue">The default value to use for missing keys.</param>
    /// <param name="setDefaultValueBeforeSettingValue">Whether to set the default value before setting a new value.</param>
    public DefaultDictionary(int capacity, TValue defaultValue, bool setDefaultValueBeforeSettingValue = false)
    {
        this.baseDictionary = new Dictionary<TKey, TValue>(capacity);
        this.defaultValue = defaultValue;
        this.SetDefaultValueBeforeSettingValue = setDefaultValueBeforeSettingValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}" /> class with an existing dictionary
    ///     and default value.
    /// </summary>
    /// <param name="dictionary">The dictionary to copy.</param>
    /// <param name="defaultValue">The default value to use for missing keys.</param>
    /// <param name="setDefaultValueBeforeSettingValue">Whether to set the default value before setting a new value.</param>
    public DefaultDictionary(IDictionary<TKey, TValue> dictionary, TValue defaultValue,
        bool setDefaultValueBeforeSettingValue = false)
    {
        this.baseDictionary = new Dictionary<TKey, TValue>(dictionary);
        this.defaultValue = defaultValue;
        this.SetDefaultValueBeforeSettingValue = setDefaultValueBeforeSettingValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}" /> class with a default value
    ///     factory.
    /// </summary>
    /// <param name="defaultValueFactory">The factory to generate default values for missing keys.</param>
    /// <param name="setDefaultValueBeforeSettingValue">Whether to set the default value before setting a new value.</param>
    public DefaultDictionary(Expression<Func<TValue>> defaultValueFactory,
        bool setDefaultValueBeforeSettingValue = false)
    {
        this.baseDictionary = new Dictionary<TKey, TValue>();
        this.defaultValueFactoryExpression = defaultValueFactory;
        this.defaultValueFactory = defaultValueFactory.Compile();
        this.SetDefaultValueBeforeSettingValue = setDefaultValueBeforeSettingValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}" /> class with a specified capacity
    ///     and default value factory.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    /// <param name="defaultValueFactory">The factory to generate default values for missing keys.</param>
    /// <param name="setDefaultValueBeforeSettingValue">Whether to set the default value before setting a new value.</param>
    public DefaultDictionary(int capacity, Expression<Func<TValue>> defaultValueFactory,
        bool setDefaultValueBeforeSettingValue = false)
    {
        this.baseDictionary = new Dictionary<TKey, TValue>(capacity);
        this.defaultValueFactoryExpression = defaultValueFactory;
        this.defaultValueFactory = defaultValueFactory.Compile();
        this.SetDefaultValueBeforeSettingValue = setDefaultValueBeforeSettingValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}" /> class with an existing dictionary
    ///     and default value factory.
    /// </summary>
    /// <param name="dictionary">The dictionary to copy.</param>
    /// <param name="defaultValueFactory">The factory to generate default values for missing keys.</param>
    /// <param name="setDefaultValueBeforeSettingValue">Whether to set the default value before setting a new value.</param>
    public DefaultDictionary(IDictionary<TKey, TValue> dictionary, Expression<Func<TValue>> defaultValueFactory,
        bool setDefaultValueBeforeSettingValue = false)
    {
        this.baseDictionary = new Dictionary<TKey, TValue>(dictionary);
        this.defaultValueFactoryExpression = defaultValueFactory;
        this.defaultValueFactory = defaultValueFactory.Compile();
        this.SetDefaultValueBeforeSettingValue = setDefaultValueBeforeSettingValue;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultDictionary{TKey, TValue}" /> class with serialized data.
    /// </summary>
    /// <param name="baseDictionary">The base dictionary to copy.</param>
    /// <param name="defaultValue">The default value to use for missing keys.</param>
    /// <param name="serializedDefaultValueFactory">The serialized default value factory expression.</param>
    /// <param name="setDefaultValueBeforeSettingValue">Whether to set the default value before setting a new value.</param>
    [JsonConstructor]
    public DefaultDictionary(IDictionary<TKey, TValue> baseDictionary, TValue? defaultValue,
        string serializedDefaultValueFactory,
        bool setDefaultValueBeforeSettingValue)
    {
        this.defaultValue = defaultValue;
        this.baseDictionary = new Dictionary<TKey, TValue>(baseDictionary);
        if (!string.IsNullOrWhiteSpace(serializedDefaultValueFactory))
        {
            this.defaultValueFactoryExpression =
                ExpressionSerializer<Func<TValue>>.DeserializeExpression(serializedDefaultValueFactory);
            this.defaultValueFactory = this.defaultValueFactoryExpression?.Compile();
        }
        else
        {
            this.defaultValueFactoryExpression = null;
            this.defaultValueFactory = null;
        }

        this.SetDefaultValueBeforeSettingValue = setDefaultValueBeforeSettingValue;
    }

    /// <summary>
    ///     Gets the dictionary as a standard <see cref="Dictionary{TKey, TValue}" />.
    /// </summary>
    [JsonPropertyName("baseDictionary")]
    public Dictionary<TKey, TValue> AsDictionary => ToDictionary();

    /// <summary>
    ///     Gets the default value.
    /// </summary>
    [JsonPropertyName("defaultValue")]
    public TValue? DefaultValue => this.defaultValue;

    /// <summary>
    ///     Gets the compiled default value factory method.
    /// </summary>
    [JsonIgnore]
    private Func<TValue>? DefaultValueFactoryMethod => this.defaultValueFactory;

    /// <summary>
    ///     Indicates whether a default value factory is set.
    /// </summary>
    [JsonIgnore]
    public bool IsDefaultValueFactorySet => this.defaultValueFactory != null;

    /// <summary>
    ///     Gets the serialized default value factory expression.
    /// </summary>
    [JsonPropertyName("serializedDefaultValueFactory")]
    public string SerializedDefaultValueFactory => this.defaultValueFactoryExpression != null
        ? ExpressionSerializer<Func<TValue>>.SerializeExpression(this.defaultValueFactoryExpression)
        : string.Empty;

    public bool IsFixedSize => false;

    public bool IsSynchronized => false;

    ICollection IDictionary.Keys => this.Keys.ToList();

    public object SyncRoot => null;

    ICollection IDictionary.Values => this.Values.ToList();

    public void Add(object key, object? value)
    {
        if (key is not TKey typedKey)
        {
            throw new ArgumentException($"Key must be of type {typeof(TKey)}", nameof(key));
        }

        if (value is not TValue typedValue)
        {
            throw new ArgumentException($"Value must be of type {typeof(TValue)}", nameof(value));
        }

        Add(typedKey, typedValue);
    }

    public bool Contains(object key)
    {
        if (key is not TKey typedKey)
        {
            throw new ArgumentException($"Key must be of type {typeof(TKey)}", nameof(key));
        }

        return ContainsKey(typedKey);
    }

    public void CopyTo(Array array, int index)
    {
        if (array is not KeyValuePair<TKey, TValue>[] typedArray)
        {
            throw new ArgumentException($"Array must be of type {typeof(KeyValuePair<TKey, TValue>)}", nameof(array));
        }

        foreach (KeyValuePair<TKey, TValue> kvp in this.baseDictionary)
        {
            typedArray[index++] = kvp;
        }
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return new DictionaryEnumerator(this.baseDictionary.GetEnumerator());
    }

    public object? this[object key]
    {
        get => this[(TKey)key];
        set => this[(TKey)key] = (TValue)value;
    }

    public void Remove(object key)
    {
        if (key is not TKey typedKey)
        {
            throw new ArgumentException($"Key must be of type {typeof(TKey)}", nameof(key));
        }

        Remove(typedKey);
    }

    public int Count => this.baseDictionary.Count;
    public bool IsReadOnly => false;

    public ICollection<TKey> Keys => this.baseDictionary.Keys;

    public ICollection<TValue> Values => this.baseDictionary.Values;

    public void Add(TKey key, TValue value)
    {
        if (this.SetDefaultValueBeforeSettingValue && !ContainsKey(key))
        {
            this.baseDictionary[key] = this.IsDefaultValueFactorySet
                ? this.defaultValueFactory()
                : this.defaultValue;
        }

        this.baseDictionary[key] = value;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (this.SetDefaultValueBeforeSettingValue && !ContainsKey(item.Key))
        {
            this.baseDictionary[item.Key] = this.IsDefaultValueFactorySet
                ? this.defaultValueFactory()
                : this.defaultValue;
        }

        this.baseDictionary[item.Key] = item.Value;
    }

    public void Clear()
    {
        this.baseDictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (this.baseDictionary.TryGetValue(item.Key, out TValue? value))
        {
            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return this.baseDictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (arrayIndex < 0 || arrayIndex >= array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        foreach (KeyValuePair<TKey, TValue> kvp in this.baseDictionary)
        {
            array[arrayIndex++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (KeyValuePair<TKey, TValue> kvp in this.baseDictionary)
        {
            yield return kvp;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Gets or sets the value associated with the specified key.
    ///     If the key does not exist, the default value or factory is used.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    public TValue this[TKey key]
    {
        get
        {
            if (!ContainsKey(key))
            {
                this.baseDictionary[key] = this.IsDefaultValueFactorySet
                    ? this.defaultValueFactory()
                    : this.defaultValue;
            }

            return this.baseDictionary[key];
        }
        set
        {
            if (this.SetDefaultValueBeforeSettingValue && !ContainsKey(key))
            {
                this.baseDictionary[key] = this.IsDefaultValueFactorySet
                    ? this.defaultValueFactory()
                    : this.defaultValue;
            }

            this.baseDictionary[key] = value;
        }
    }

    public bool Remove(TKey key)
    {
        if (this.baseDictionary.ContainsKey(key))
        {
            this.baseDictionary.Remove(key);
            return true;
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (this.baseDictionary.TryGetValue(item.Key, out TValue? value))
        {
            if (EqualityComparer<TValue>.Default.Equals(value, item.Value))
            {
                this.baseDictionary.Remove(item.Key);
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (this.baseDictionary.TryGetValue(key, out value))
        {
            return true;
        }

        if (this.IsDefaultValueFactorySet)
        {
            value = this.defaultValueFactory();
            this.baseDictionary[key] = value;
            return true;
        }

        value = this.defaultValue;
        this.baseDictionary[key] = value;
        return true;
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

    /// <summary>
    ///     Converts the dictionary to a standard <see cref="Dictionary{TKey, TValue}" />.
    /// </summary>
    /// <returns>A new <see cref="Dictionary{TKey, TValue}" /> containing the same data.</returns>
    public Dictionary<TKey, TValue> ToDictionary()
    {
        return new Dictionary<TKey, TValue>(this);
    }

    private class DictionaryEnumerator : IDictionaryEnumerator
    {
        private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

        public DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
        {
            this.enumerator = enumerator;
        }

        public object Current => this.Entry;

        public DictionaryEntry Entry => new(this.enumerator.Current.Key, this.enumerator.Current.Value);

        public object Key => this.enumerator.Current.Key;

        public object? Value => this.enumerator.Current.Value;

        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        public void Reset()
        {
            this.enumerator.Reset();
        }
    }
}
