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
/// Represents a Weaviate collection configuration.
/// </summary>
public class WeaviateCollection
{
    /// <summary>
    /// The vectorizer to use.
    /// </summary>
    [JsonPropertyName("vectorizer")]
    public string Vectorizer { get; set; } = string.Empty;

    /// <summary>
    /// Module-specific configuration.
    /// </summary>
    [JsonPropertyName("moduleConfig")]
    public Dictionary<string, Dictionary<string, object>>? ModuleConfig { get; set; }

    /// <summary>
    /// The type of vector index to use.
    /// </summary>
    [JsonPropertyName("vectorIndexType")]
    public VectorIndexType VectorIndexType { get; set; } = VectorIndexType.HNSW;

    /// <summary>
    /// The name of the collection.
    /// </summary>
    [JsonPropertyName("class")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the collection.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Properties defined in the collection.
    /// </summary>
    [JsonPropertyName("properties")]
    public Property[]? Properties { get; set; }

    /// <summary>
    /// Configuration for the inverted index.
    /// </summary>
    [JsonPropertyName("invertedIndexConfig")]
    public InvertedIndexConfig? InvertedIndexConfig { get; set; }

    /// <summary>
    /// Configuration for the vector index.
    /// </summary>
    [JsonPropertyName("vectorIndexConfig")]
    public VectorIndexConfig? VectorIndexConfig { get; set; }

    /// <summary>
    /// Configuration for sharding.
    /// </summary>
    [JsonPropertyName("shardingConfig")]
    public ShardingConfig? ShardingConfig { get; set; }

    /// <summary>
    /// Configuration for the vectorizer.
    /// </summary>
    [JsonPropertyName("vectorizerConfig")]
    [JsonIgnore]
    public Dictionary<string, object>? VectorizerConfig
    {
        get => !string.IsNullOrEmpty(Vectorizer) ? ModuleConfig?[VectorizerExtensions.FromWeaviateString(Vectorizer).ToWeaviateString()] : ModuleConfig?["text2vec-ollama"];
        set
        {
            if (value != null && !string.IsNullOrEmpty(Vectorizer))
            {
                var config = new Dictionary<string, object>
                {
                    ["skip"] = false,
                    ["vectorizePropertyName"] = true,
                    ["vectorizeClassName"] = true
                };

                // Copy over model and apiEndpoint if provided
                if (value.TryGetValue("model", out var model))
                    config["model"] = model?.ToString()?.EndsWith(":latest") == true ? model : $"{model}:latest";
                if (value.TryGetValue("apiEndpoint", out var endpoint))
                    config["apiEndpoint"] = (endpoint?.ToString() ?? "http://host.docker.internal:11434").Replace("localhost", "host.docker.internal");

                var vectorizerName = !string.IsNullOrEmpty(Vectorizer) ? VectorizerExtensions.FromWeaviateString(Vectorizer).ToWeaviateString() : "text2vec-ollama";
                ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                {
                    [vectorizerName] = config
                };
            }
        }
    }

    /// <summary>
    /// Configuration for replication.
    /// </summary>
    [JsonPropertyName("replicationConfig")]
    public ReplicationConfig? ReplicationConfig { get; set; }

    /// <summary>
    /// Multi-tenancy configuration.
    /// </summary>
    [JsonPropertyName("multiTenancyConfig")]
    public object? MultiTenancyConfig { get; set; }

    /// <summary>
    /// For backward compatibility with v3 API.
    /// </summary>
    [Obsolete("Use Name property instead. This property will be removed in v5.")]
    [JsonIgnore]
    public string Class
    {
        get => Name;
        set => Name = value;
    }
}
