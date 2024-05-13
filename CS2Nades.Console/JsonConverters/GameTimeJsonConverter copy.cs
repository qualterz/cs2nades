using System.Text.Json;
using System.Text.Json.Serialization;
using DemoFile;

namespace CS2Nades.Console.JsonConverters;

public class GameTimeJsonConverter : JsonConverter<GameTime>
{
    public override GameTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, GameTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}