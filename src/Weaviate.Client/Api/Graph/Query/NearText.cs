namespace Weaviate.Client;

public class NearText
{
    public string[]? Concepts { get; set; }
    public float? Distance { get; set; }
    public float? Certainty { get; set; }
    public MoveParameters? MoveTo { get; set; }
    public MoveParameters? MoveAwayFrom { get; set; }

    public override string ToString()
    {
        if (Concepts == null) return string.Empty;
        var result = $"{{ concepts: [\"{string.Join("\",\"", Concepts)}\"]";
        
        if (Distance.HasValue) result += $", distance: {Distance}";
        if (Certainty.HasValue) result += $", certainty: {Certainty}";
        if (MoveTo != null) result += $", moveTo: {MoveTo}";
        if (MoveAwayFrom != null) result += $", moveAwayFrom: {MoveAwayFrom}";
        
        return result + " }";
    }
}

public class MoveParameters
{
    public string[]? Concepts { get; set; }
    public ObjectMove[]? Objects { get; set; }
    public float Force { get; set; }

    public override string ToString()
    {
        var result = "{ ";
        if (Concepts != null) result += $"concepts: [\"{string.Join("\",\"", Concepts)}\"], ";
        if (Objects != null) result += $"objects: [{string.Join(",", Objects.Select(o => o.ToString()))}], ";
        result += $"force: {Force} }}";
        return result;
    }
}
