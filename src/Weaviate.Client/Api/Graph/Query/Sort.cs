namespace Weaviate.Client;

public class Sort
{
    public string[]? Path { get; set; }
    public SortOrder Order { get; set; }

    public override string ToString()
    {
        if (Path == null) return string.Empty;
        return $"{{path:[\"{string.Join("\",\"", Path)}\"],order:\"{Order}\"}}";
    }
}
