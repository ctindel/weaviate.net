using System.Linq;

namespace Weaviate.Client;

public class GetRequest
{
    public string? Collection { get; set; }
    public Field[]? Fields { get; set; }
    public Where? Where { get; set; }
    public Sort[]? Sorts { get; set; }
    public Group? Group { get; set; }
    public NearVector? NearVector { get; set; }
    public NearObject? NearObject { get; set; }
    public NearText? NearText { get; set; }
    public BM25? BM25 { get; set; }
    public Hybrid? Hybrid { get; set; }
    public string? After { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }

    public override string ToString()
    {
        if (Collection == null || Fields == null) return string.Empty;
        var fields = string.Join(" ", Fields.Select(f => f.ToString()));
        var query = $"query={{ Get {{ {Collection} ";

        if (Where != null) query += $"where: {Where.BuildWithWhere()} ";
        if (Sorts?.Length > 0) query += $"sort: [{string.Join(",", Sorts.Select(s => s.ToString()))}] ";
        if (Group != null) query += $"group: {Group} ";
        if (NearVector?.Vector != null) query += $"nearVector: {{ vector: [{string.Join(",", NearVector.Vector)}] }} ";
        if (NearObject?.Id != null) query += $"nearObject: {{ id: \"{NearObject.Id}\" }} ";
        if (NearText?.Concepts != null) query += $"nearText: {{ concepts: [\"{string.Join("\",\"", NearText.Concepts)}\"] }} ";
        if (BM25 != null) query += $"bm25: {BM25} ";
        if (Hybrid != null) query += $"hybrid: {Hybrid} ";
        if (After != null) query += $"after: \"{After}\" ";
        if (Limit.HasValue) query += $"limit: {Limit} ";
        if (Offset.HasValue) query += $"offset: {Offset} ";

        query += $"{{ {fields} }} }} }}";
        return query;
    }
}
