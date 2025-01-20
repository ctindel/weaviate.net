using System.Linq;

namespace Weaviate.Client;

public class Field
{
    public string Name { get; set; } = string.Empty;
    public Field[]? Fields { get; set; }

    public static implicit operator Field(string name) => new() { Name = name };

    public override string ToString()
    {
        if (Fields == null || Fields.Length == 0) return Name;
        var subFields = string.Join(" ", Fields.Select(f => f.ToString()));
        return $"{Name}{{{subFields}}}";
    }
}
