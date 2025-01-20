// Copyright (C) 2023 Weaviate - https://weaviate.io
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//         http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// ReSharper disable once CheckNamespace
namespace Weaviate.Client;

public class Get
{
    [Obsolete("Use Collection instead. This property will be removed in v5.")]
    public string? Class { get; set; }
    public string? Collection { get; set; }
    public Field[]? Fields { get; set; }
    public int? Offset { get; set; }
    public int? Limit { get; set; }
    public string? After { get; set; }
    public Where? Where { get; set; }
    public NearText? NearText { get; set; }
    public BM25? BM25 { get; set; }
    public Hybrid? Hybrid { get; set; }
    public NearObject? NearObject { get; set; }
    public Ask? Ask { get; set; }
    public NearImage? NearImage { get; set; }
    public NearVector? NearVector { get; set; }
    public Group? Group { get; set; }
    public Sort[]? Sorts { get; set; }

    public override string ToString() => $"{{Get{{{Collection ?? string.Empty}{CreateFilterClause()}{{{CreateFields()}}}}}}}";

    private string CreateFields() =>
	    Fields is { Length: > 0 }
		    ? string.Join(" ", Fields.Select(f => f.ToString()).ToArray())
		    : string.Empty;

    private string CreateFilterClause()
    {
        if (Where == null && NearText == null && BM25 == null && Hybrid == null && NearObject == null
            && NearVector == null && NearImage == null && Group == null
            && Ask == null && Limit == null && Offset == null && Sorts == null)
            return string.Empty;

        var filters = new HashSet<string>();
        if (Where != null) filters.Add(Where.BuildWithWhere());
        if (NearText?.ToString() is string nearText) filters.Add(nearText);
        if (BM25?.ToString() is string bm25) filters.Add(bm25);
        if (Hybrid?.ToString() is string hybrid) filters.Add(hybrid);
        if (NearObject?.ToString() is string nearObject) filters.Add(nearObject);
        if (NearVector?.ToString() is string nearVector) filters.Add(nearVector);
        if (Group?.ToString() is string group) filters.Add(group);
        if (Ask?.ToString() is string ask) filters.Add(ask);
        if (NearImage?.ToString() is string nearImage) filters.Add(nearImage);
        if (Limit != null) filters.Add($"limit:{Limit}");
        if (After != null) filters.Add($"after:\"{After}\"");
        if (Offset != null) filters.Add($"offset:{Offset}");
        if (Sorts != null)
        {
            if (Sorts is { Length: > 0 })
            {
                var args = Sorts.Select(s => s.ToString()).ToArray();
                var sorts = string.Join(",", args);
                filters.Add($"sort:[{sorts}]");
            }
            else
            {
                filters.Add("sort:[]");
            }
        }

        var s = string.Join(",", filters.ToArray());
        return $"({s})";
    }
}
