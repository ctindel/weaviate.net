namespace Weaviate.Client;

public class Ask
{
    public string Question { get; set; } = string.Empty;
    public string[]? Properties { get; set; }
    public float? Distance { get; set; }
    public float? Certainty { get; set; }
    public bool? Rerank { get; set; }

    public override string ToString()
    {
        var result = $"{{question:\"{Question}\"";
        if (Properties?.Length > 0)
            result += $",properties:[\"{string.Join("\",\"", Properties)}\"]";
        if (Distance.HasValue)
            result += $",distance:{Distance.Value}";
        if (Certainty.HasValue)
            result += $",certainty:{Certainty.Value}";
        if (Rerank.HasValue)
            result += $",rerank:{Rerank.Value.ToString().ToLowerInvariant()}";
        return result + "}";
    }
}
