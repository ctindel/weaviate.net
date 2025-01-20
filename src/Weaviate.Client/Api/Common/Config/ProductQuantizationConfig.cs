namespace Weaviate.Client;

public class ProductQuantizationConfig
{
    public bool Enabled { get; set; }
    public bool BitCompression { get; set; }
    public int Segments { get; set; }
    public int Centroids { get; set; }
    public ProductQuantizationEncoder? Encoder { get; set; }
}

public class ProductQuantizationEncoder
{
    public string Type { get; set; } = string.Empty;
    public string Distribution { get; set; } = string.Empty;
}
