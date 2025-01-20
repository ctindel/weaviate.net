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

public class Aggregate
{
    [Obsolete("Use Collection instead. This property will be removed in v5.")]
    public string? Class { get; set; }
    public string? Collection { get; set; }
    public Field[]? Fields { get; set; }
    public string? GroupBy { get; set; }
    public Where? Where { get; set; }
    public NearText? NearText { get; set; }
    public NearObject? NearObject { get; set; }
    public NearVector? NearVector { get; set; }
    public Ask? Ask { get; set; }
    public NearImage? NearImage { get; set; }
    public int? ObjectLimit { get; set; }
    public int? Limit { get; set; }

    public override string ToString() => $"{{Aggregate{{{Collection ?? string.Empty}{CreateFilterClause()}{{{CreateFields()}}}}}}}";

    private string CreateFields() =>
	    Fields is { Length: > 0 }
		    ? string.Join(" ", Fields.Select(f => f.ToString()).ToArray())
		    : string.Empty;

    private string CreateFilterClause()
    {
        if (Where == null && NearText == null && NearObject == null
            && NearVector == null && ObjectLimit == null && Ask == null
            && NearImage == null && Limit == null && string.IsNullOrEmpty(GroupBy))
            return string.Empty;

        var filters = new List<string>();
        if (!string.IsNullOrEmpty(GroupBy)) filters.Add($"groupBy:\"{GroupBy}\"");
        if (Where != null) filters.Add($"where:{Where.BuildWithWhere()}");
        if (NearText != null) filters.Add($"nearText:{NearText}");
        if (NearObject != null) filters.Add($"nearObject:{NearObject}");
        if (NearVector != null) filters.Add($"nearVector:{NearVector}");
        if (Ask != null) filters.Add($"ask:{Ask}");
        if (NearImage != null) filters.Add($"nearImage:{NearImage}");
        if (Limit != null) filters.Add($"limit:{Limit}");
        if (ObjectLimit != null) filters.Add($"objectLimit:{ObjectLimit}");
        var s = string.Join(",", filters);
        return filters.Count > 0 ? $"({s})" : string.Empty;
    }
}
