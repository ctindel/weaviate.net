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
    }

    [Obsolete("Use Name property instead. This property will be removed in v5.")]
    [JsonIgnore]
    public new string Class
    {
        get => Name;
        set => Name = value;
    }

    public new Dictionary<string, object>? VectorizerConfig
    {
        get => base.VectorizerConfig;
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

                ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                {
                    ["text2vec-ollama"] = moduleConfigDict
                };
            }
        }
    }
}
