using System.Text.Json;
using System.Text.Json.Serialization;
using DemoFile;

namespace CS2Nades.Console.JsonConverters;

public class GameTickJsonConverter : JsonConverter<GameTick>
{
    public override GameTick Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, GameTick value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}