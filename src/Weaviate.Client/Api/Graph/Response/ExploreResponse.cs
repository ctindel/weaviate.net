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

public class ExploreResponse
{
    public string? Beacon { get; set; }
    public string? Collection { get; set; }
    public float? Certainty { get; set; }
    public float? Distance { get; set; }
    public string? Id { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public Dictionary<string, object>? Additional { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
