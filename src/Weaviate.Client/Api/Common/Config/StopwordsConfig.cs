namespace Weaviate.Client;

public class StopwordsConfig
{
    public string? Preset { get; set; }
    public string[]? Additions { get; set; }
    public string[]? Removals { get; set; }
}
