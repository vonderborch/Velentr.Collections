using System.Text.Json;
using System.Text.Json.Serialization;

namespace Velentr.Collections.Json;

public class DefaultDictionaryJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(DefaultDictionary<,>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type[] genericArguments = typeToConvert.GetGenericArguments();
        Type converterType = typeof(DefaultDictionaryJsonConverter<,>).MakeGenericType(genericArguments);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
