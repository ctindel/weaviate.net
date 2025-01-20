namespace Weaviate.Client;

public class ObjectMove
{
    public string? Id { get; set; }
    public string? Beacon { get; set; }

    public override string ToString()
    {
        if (Id == null && Beacon == null) return string.Empty;
        if (Id != null) return $"{{ id: \"{Id}\" }}";
        return $"{{ beacon: \"{Beacon}\" }}";
    }
}
