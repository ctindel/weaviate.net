namespace Weaviate.Client;

public class NearImage
{
    public string Image { get; set; } = string.Empty;
    public FileInfo? ImageFile { get; set; }
    public float? Distance { get; set; }
    public float? Certainty { get; set; }

    public override string ToString()
    {
        var result = $"{{image:\"{Image}\"";
        if (Distance.HasValue)
            result += $",distance:{Distance.Value}";
        if (Certainty.HasValue)
            result += $",certainty:{Certainty.Value}";
        return result + "}";
    }
}
