using System.Text.Json.Serialization;

namespace Weaviate.Client;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VectorIndexType
{
    [JsonPropertyName("hnsw")]
    HNSW,
    [JsonPropertyName("flat")]
    Flat,
    [JsonPropertyName("skip")]
    Skip
}
