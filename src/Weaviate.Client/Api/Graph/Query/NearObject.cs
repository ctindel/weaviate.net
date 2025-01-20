namespace Weaviate.Client;

public class NearObject
{
    public string Id { get; set; } = string.Empty;
    public string Beacon { get; set; } = string.Empty;
    public float? Distance { get; set; }
    public float? Certainty { get; set; }

    public override string ToString()
    {
        var result = !string.IsNullOrEmpty(Id) ? $"{{id:\"{Id}\"" : $"{{beacon:\"{Beacon}\"";
        if (Distance.HasValue)
            result += $",distance:{Distance.Value}";
        if (Certainty.HasValue)
            result += $",certainty:{Certainty.Value}";
        return result + "}";
    }
}
