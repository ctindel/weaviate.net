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

[JsonConverter(typeof(PropertyJsonConverter))]
public class Property
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    private string[]? _dataType;

    [JsonPropertyName("dataType")]
    public string[]? DataType 
    { 
        get => _dataType;
        set
        {
            _dataType = value;
            // Update tokenization when data type changes
            if (value != null && 
                value.Any(dt => dt == "text" || dt == "string" || 
                               dt == "text[]" || dt == "string[]"))
            {
                Tokenization = Tokenization.Word;
            }
        }
    }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    private Tokenization _tokenization = Tokenization.Word;

    [JsonPropertyName("tokenization")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Tokenization Tokenization 
    { 
        get => _tokenization;
        set
        {
            // Set Word tokenization by default for text/string types
            if (DataType != null && 
                DataType.Any(dt => dt == "text" || dt == "string" || 
                                 dt == "text[]" || dt == "string[]"))
            {
                _tokenization = Tokenization.Word;
            }
            else
            {
                _tokenization = value;
            }
        }
    }
    
    // v4 API additions
    [JsonPropertyName("indexFilterable")]
    public bool IndexFilterable { get; set; } = true;

    [JsonPropertyName("indexSearchable")]
    public bool IndexSearchable { get; set; } = true;

    [JsonPropertyName("skipVectorization")]
    public bool SkipVectorization { get; set; } = false;

    [JsonPropertyName("indexInverted")]
    [Obsolete("Use IndexFilterable and IndexSearchable instead. This property will be removed in v5.")]
    public bool? IndexInverted { get; set; }
    
    // Property-level configurations
    [JsonPropertyName("vectorizerConfig")]
    public PropertyVectorizerConfig? VectorizerConfig { get; set; }

    [JsonPropertyName("vectorIndexConfig")]
    public PropertyVectorIndexConfig? VectorIndexConfig { get; set; }

    [JsonPropertyName("rerankerConfig")]
    public PropertyRerankerConfig? RerankerConfig { get; set; }
    
    // Module-specific configuration (for vectorizers, etc.)
    [JsonPropertyName("nestedProperties")]
    public Property[]? NestedProperties { get; set; }
    
    [JsonIgnore]
    public object? ModuleConfig { get; set; }
}

/// <summary>
/// Configuration for property-level vectorization settings.
/// </summary>
public class PropertyVectorizerConfig
{
    /// <summary>
    /// The vectorizer to use for this property. Must be one of the vectorizers configured at the collection level.
    /// </summary>
    public string Vectorizer { get; set; } = string.Empty;

    /// <summary>
    /// Whether to skip vectorization for this property. If true, the property will not be included in vector calculations.
    /// </summary>
    public bool? SkipVectorization { get; set; }

    /// <summary>
    /// Additional vectorizer-specific options.
    /// </summary>
    public object? VectorizerOptions { get; set; }
}

/// <summary>
/// Configuration for property-level vector indexing settings.
/// </summary>
public class PropertyVectorIndexConfig
{
    /// <summary>
    /// The distance metric to use for vector similarity calculations.
    /// </summary>
    public string Distance { get; set; } = string.Empty;

    /// <summary>
    /// How often the async process runs that "repairs" the HNSW graph after deletes and updates.
    /// </summary>
    public int? CleanupIntervalSeconds { get; set; }

    /// <summary>
    /// The maximum number of connections per element in all layers.
    /// </summary>
    public int? MaxConnections { get; set; }

    /// <summary>
    /// Controls index search speed / build speed tradeoff.
    /// </summary>
    public int? EfConstruction { get; set; }

    /// <summary>
    /// Larger values are more accurate at the expense of slower search.
    /// </summary>
    public int? Ef { get; set; }

    /// <summary>
    /// Whether to skip vector indexing for this property.
    /// </summary>
    public bool? Skip { get; set; }

    /// <summary>
    /// Maximum number of vectors to keep in memory.
    /// </summary>
    public long? VectorCacheMaxObjects { get; set; }
}

/// <summary>
/// Configuration for property-level reranking settings.
/// </summary>
public class PropertyRerankerConfig
{
    /// <summary>
    /// The reranker to use for this property.
    /// </summary>
    public string Reranker { get; set; } = string.Empty;

    /// <summary>
    /// Additional reranker-specific options.
    /// </summary>
    public object? RerankerOptions { get; set; }
}

/// <summary>
/// Available vector distance metrics.
/// </summary>
public static class VectorDistances
{
    public const string Cosine = "cosine";
    public const string Dot = "dot";
    public const string L2 = "l2";
    public const string Manhattan = "manhattan";
    public const string Hamming = "hamming";
}

/// <summary>
/// Available reranker types.
/// </summary>
public static class RerankerTypes
{
    public const string None = "none";
    public const string Transformers = "reranker-transformers";
    public const string Cohere = "reranker-cohere";
    public const string CrossEncoder = "reranker-cross-encoder";
}
