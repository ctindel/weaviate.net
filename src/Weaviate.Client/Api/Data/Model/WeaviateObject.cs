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

public class WeaviateObject
{
    public string? Id { get; set; }
    [Obsolete("Use Collection instead. This property will be removed in v5.")]
    [JsonIgnore]
    public string? Class
    {
        get => Collection;
        set => Collection = value;
    }
    [JsonPropertyName("class")]
    public string? Collection { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
    public Dictionary<string, object>? Additional { get; set; }
    public float[]? Vector { get; set; }
    [JsonPropertyName("moduleConfig")]
    public Dictionary<string, Dictionary<string, object>>? ModuleConfig { get; set; }

    [JsonIgnore]
    public Dictionary<string, object>? VectorizerConfig
    {
        get => ModuleConfig?["text2vec-ollama"];
        set
        {
            if (value != null)
            {
                var moduleConfigDict = new Dictionary<string, object>();

                // Copy over model and apiEndpoint if provided
                if (value.TryGetValue("model", out var model))
                    moduleConfigDict["model"] = model?.ToString()?.EndsWith(":latest") == true ? model : $"{model}:latest";
                if (value.TryGetValue("apiEndpoint", out var endpoint))
                    moduleConfigDict["apiEndpoint"] = (endpoint?.ToString() ?? "http://host.docker.internal:11434").Replace("localhost", "host.docker.internal");

                // Always set these fields to match Python client behavior
                moduleConfigDict["skip"] = value.TryGetValue("skip", out var skip) ? skip : false;
                moduleConfigDict["vectorizePropertyName"] = value.TryGetValue("vectorizePropertyName", out var vectorizePropertyName) ? vectorizePropertyName : true;
                moduleConfigDict["vectorizeClassName"] = value.TryGetValue("vectorizeClassName", out var vectorizeClassName) ? vectorizeClassName : true;

                var vectorizerName = "text2vec-ollama";  // Default vectorizer
                ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                {
                    [vectorizerName] = moduleConfigDict
                };
            }
        }
    }
}
