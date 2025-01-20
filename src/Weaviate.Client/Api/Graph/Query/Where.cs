namespace Weaviate.Client;

public class Where
{
    public string[]? Path { get; set; }
    public Operator Operator { get; set; }
    public string? ValueString { get; set; }
    public Where[]? Operands { get; set; }
    public string? ValueText { get; set; }
    public DateTime? ValueDate { get; set; }

    public string BuildWithWhere()
    {
        if (Path == null) return string.Empty;
        var path = $"{{path:[\"{string.Join("\",\"", Path)}\"],operator:\"{Operator}\"";
        
        if (ValueString != null) return $"{path},valueString:\"{ValueString}\"}}";
        if (ValueText != null) return $"{path},valueText:\"{ValueText}\"}}";
        if (ValueDate.HasValue) return $"{path},valueDate:\"{ValueDate:yyyy-MM-ddTHH:mm:sszzz}\"}}";
        
        return path + "}";
    }
}

public enum Operator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanEqual,
    LessThan,
    LessThanEqual,
    Like,
    ContainsAll,
    ContainsAny,
    WithinGeoRange,
    Or,
    And
}
