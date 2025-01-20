namespace Weaviate.Client;

public class Hybrid
{
    public string Query { get; set; } = string.Empty;
    public float? Alpha { get; set; }
    public string[]? Properties { get; set; }
    public float? Distance { get; set; }
}
