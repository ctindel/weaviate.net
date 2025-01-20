namespace Weaviate.Client;

public class CreateClassRequest
{
    public string Name { get; set; } = string.Empty;
    
    [Obsolete("Use Name instead. This property will be removed in v5.")]
    public string Class { get => Name; set => Name = value; }
    
    public string? Description { get; set; }
    public VectorIndexType VectorIndexType { get; set; }
    public Vectorizer Vectorizer { get; set; }
    public Property[]? Properties { get; set; }
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }
    public VectorIndexConfig? VectorIndexConfig { get; set; }
    public ShardingConfig? ShardingConfig { get; set; }
    public ReplicationConfig? ReplicationConfig { get; set; }

    public CreateClassRequest(string name)
    {
        Name = name;
    }
}
