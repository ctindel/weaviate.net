namespace Weaviate.Client;

public class ShardingConfig
{
    public int VirtualPerPhysical { get; set; }
    public int DesiredCount { get; set; }
    public int DesiredVirtualCount { get; set; }
    public string Key { get; set; } = "_id";
    public string Strategy { get; set; } = "hash";
    public string Function { get; set; } = "murmur3";
    public int? ActualCount { get; set; }
    public int? ActualVirtualCount { get; set; }
}
