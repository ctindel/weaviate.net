namespace Weaviate.Client;

public class ReplicationConfig
{
    public int? Factor { get; set; }
    public bool? AsyncEnabled { get; set; }
    public ReplicationDeletionStrategy? DeletionStrategy { get; set; }
}
