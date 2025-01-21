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

using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Weaviate.Client;

/// <summary>
/// Request to create a new collection.
/// </summary>
public class CreateCollectionRequest : WeaviateCollection
{
    public CreateCollectionRequest(string name)
    {
        Name = name;
        VectorIndexType = VectorIndexType.HNSW;
        Vectorizer = "text2vec-ollama";
    }

    [Obsolete("Use Name property instead. This property will be removed in v5.")]
    [JsonIgnore]
    public new string Class
    {
        get => Name;
        set => Name = value;
    }

    [JsonIgnore]
    public new Dictionary<string, object>? VectorizerConfig
    {
        get => base.VectorizerConfig;
        set
        {
            if (value != null)
            {
                var moduleConfigDict = new Dictionary<string, object>
                {
                    ["model"] = value.TryGetValue("model", out var model) ? model?.ToString()?.EndsWith(":latest") == true ? model : $"{model}:latest" : "mxbai-embed-large:latest",
                    ["apiEndpoint"] = value.TryGetValue("apiEndpoint", out var endpoint) ? (endpoint?.ToString() ?? "http://host.docker.internal:11434").Replace("localhost", "host.docker.internal") : "http://host.docker.internal:11434",
                    ["vectorizeClassName"] = value.TryGetValue("vectorizeClassName", out var vectorizeClassName) ? vectorizeClassName : true
                };

                ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                {
                    ["text2vec-ollama"] = moduleConfigDict
                };
            }
        }
    }
}
