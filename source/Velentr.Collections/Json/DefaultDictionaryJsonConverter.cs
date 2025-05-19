using System.Text.Json;
using System.Text.Json.Serialization;

namespace Velentr.Collections.Json;

public class DefaultDictionaryJsonConverter<TKey, TValue> : JsonConverter<DefaultDictionary<TKey, TValue>>
    where TKey : notnull
{
    public override DefaultDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        Dictionary<TKey, TValue>? dict = null;
        TValue? defaultValue = default;
        string? factoryExpr = null;
        var setBefore = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var prop = reader.GetString();
            reader.Read();
            switch (prop)
            {
                case "dictionary":
                    dict = JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(ref reader, options);
                    break;
                case "defaultValue":
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        defaultValue = default;
                    }
                    else
                    {
                        defaultValue = JsonSerializer.Deserialize<TValue?>(ref reader, options);
                    }

                    break;
                case "serializedDefaultValueFactory":
                    factoryExpr = reader.GetString();
                    break;
                case "setDefaultValueBeforeSettingValue":
                    setBefore = reader.GetBoolean();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new DefaultDictionary<TKey, TValue>(
            dict ?? new Dictionary<TKey, TValue>(),
            defaultValue!,
            factoryExpr ?? string.Empty,
            setBefore
        );
    }

    public override void Write(Utf8JsonWriter writer, DefaultDictionary<TKey, TValue> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("dictionary");
        JsonSerializer.Serialize(writer, value.ToDictionary(), options);

        writer.WritePropertyName("defaultValue");
        JsonSerializer.Serialize(writer, value.DefaultValue, options);

        writer.WritePropertyName("serializedDefaultValueFactory");
        writer.WriteStringValue(value.SerializedDefaultValueFactory);

        writer.WritePropertyName("setDefaultValueBeforeSettingValue");
        writer.WriteBooleanValue(value.SetDefaultValueBeforeSettingValue);

        writer.WriteEndObject();
    }
}
