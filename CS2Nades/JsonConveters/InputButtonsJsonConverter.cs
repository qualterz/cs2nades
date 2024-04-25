using System.Text.Json;
using System.Text.Json.Serialization;
using DemoFile.Sdk;

namespace CS2Nades.JsonConverters;

public class InputButtonsJsonConverter : JsonConverter<InputButtons>
{
    public override InputButtons Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, InputButtons value, JsonSerializerOptions options)
    {
        if (value == InputButtons.None)
        {
            writer.WriteNullValue();
            return;
        }

        var strings = value.ToString().Split(' ');

        if (options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase)
            strings = strings.Select(JsonNamingPolicy.CamelCase.ConvertName).ToArray();

        writer.WriteStartArray();

        foreach (var @string in strings)
            writer.WriteStringValue(@string);

        writer.WriteEndArray();
    }
}