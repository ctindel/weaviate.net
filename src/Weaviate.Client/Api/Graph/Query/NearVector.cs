namespace Weaviate.Client;

public class NearVector
{
    public float[]? Vector { get; set; }
    public float? Distance { get; set; }
    public float? Certainty { get; set; }

    public override string ToString()
    {
        if (Vector == null) return string.Empty;
        var result = $"{{vector:[{string.Join(",", Vector)}]";
        if (Distance.HasValue)
            result += $",distance:{Distance.Value}";
        if (Certainty.HasValue)
            result += $",certainty:{Certainty.Value}";
        return result + "}";
    }
}
