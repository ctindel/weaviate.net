using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client;

[JsonConverter(typeof(DistanceConverter))]
public enum Distance
{
    Cosine,
    DotProduct,
    L2,
    Manhattan,
    Hamming
}

public class DistanceConverter : JsonConverter<Distance>
{
    public override Distance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()?.ToLowerInvariant();
        return value switch
        {
            "cosine" => Distance.Cosine,
            "dot" => Distance.DotProduct,
            "l2" => Distance.L2,
            "manhattan" => Distance.Manhattan,
            "hamming" => Distance.Hamming,
            _ => throw new JsonException($"Cannot convert '{value}' to Distance")
        };
    }

    public override void Write(Utf8JsonWriter writer, Distance value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            Distance.Cosine => "cosine",
            Distance.DotProduct => "dot",
            Distance.L2 => "l2",
            Distance.Manhattan => "manhattan",
            Distance.Hamming => "hamming",
            _ => throw new JsonException($"Cannot convert Distance '{value}' to string")
        };
        writer.WriteStringValue(stringValue);
    }
}
