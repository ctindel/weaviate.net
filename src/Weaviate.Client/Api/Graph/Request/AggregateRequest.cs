using System.Linq;

namespace Weaviate.Client;

public class AggregateRequest
{
    public string? Collection { get; set; }
    public Field[]? Fields { get; set; }
    public Where? Where { get; set; }
    public string? GroupBy { get; set; }
    public NearVector? NearVector { get; set; }
    public NearObject? NearObject { get; set; }
    public NearText? NearText { get; set; }
    public int? ObjectLimit { get; set; }

    public override string ToString()
    {
        if (Collection == null || Fields == null) return string.Empty;
        var fields = string.Join(" ", Fields.Select(f => f.ToString()));
        var query = $"query={{ Aggregate {{ {Collection} ";

        if (Where != null) query += $"where: {Where.BuildWithWhere()} ";
        if (GroupBy != null) query += $"groupBy: \"{GroupBy}\" ";
        if (NearVector?.Vector != null) query += $"nearVector: {{ vector: [{string.Join(",", NearVector.Vector)}] }} ";
        if (NearObject?.Id != null) query += $"nearObject: {{ id: \"{NearObject.Id}\" }} ";
        if (NearText?.Concepts != null) query += $"nearText: {{ concepts: [\"{string.Join("\",\"", NearText.Concepts)}\"] }} ";
        if (ObjectLimit.HasValue) query += $"limit: {ObjectLimit} ";

        query += $"{{ {fields} }} }} }}";
        return query;
    }
}
