namespace Weaviate.Client;

public class BatchResponse<T> where T : WeaviateObject
{
    public BatchResults Results { get; set; } = new();
    public string Output { get; set; } = BatchOutput.Minimal;
    public T[]? Objects { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
}
