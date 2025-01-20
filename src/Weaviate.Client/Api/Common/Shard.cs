namespace Weaviate.Client;

public class Shard
{
    public string? Name { get; set; }
    public ShardStatus Status { get; set; } = ShardStatus.Ready;
    public string? Collection { get; set; }
    public int ObjectCount { get; set; }
}
