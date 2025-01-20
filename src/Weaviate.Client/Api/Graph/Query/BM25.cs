namespace Weaviate.Client;

public class BM25
{
    public string? Query { get; set; }
    public string[]? Properties { get; set; }

    public override string ToString()
    {
        if (Query == null || Properties == null) return string.Empty;
        return $"{{ query: \"{Query}\", properties: [\"{string.Join("\",\"", Properties)}\"] }}";
    }
}
