using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client;

[JsonConverter(typeof(ReplicationDeletionStrategyConverter))]
public enum ReplicationDeletionStrategy
{
    DeleteOnConflict,
    NoAutomatedResolution,
    TimeBasedResolution
}

public class ReplicationDeletionStrategyConverter : JsonConverter<ReplicationDeletionStrategy>
{
    public override ReplicationDeletionStrategy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "DeleteOnConflict" => ReplicationDeletionStrategy.DeleteOnConflict,
            "NoAutomatedResolution" => ReplicationDeletionStrategy.NoAutomatedResolution,
            "TimeBasedResolution" => ReplicationDeletionStrategy.TimeBasedResolution,
            _ => throw new JsonException($"Cannot convert '{value}' to ReplicationDeletionStrategy")
        };
    }

    public override void Write(Utf8JsonWriter writer, ReplicationDeletionStrategy value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            ReplicationDeletionStrategy.DeleteOnConflict => "DeleteOnConflict",
            ReplicationDeletionStrategy.NoAutomatedResolution => "NoAutomatedResolution",
            ReplicationDeletionStrategy.TimeBasedResolution => "TimeBasedResolution",
            _ => throw new JsonException($"Cannot convert ReplicationDeletionStrategy '{value}' to string")
        };
        writer.WriteStringValue(stringValue);
    }
}
