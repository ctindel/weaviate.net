namespace Weaviate.Client;

public class CreateObjectsBatchRequest
{
    public WeaviateObject[] Objects { get; set; }
    public ConsistencyLevel? ConsistencyLevel { get; set; }

    public CreateObjectsBatchRequest(params WeaviateObject[] objects)
    {
        Objects = objects ?? throw new ArgumentNullException(nameof(objects));
    }
}
