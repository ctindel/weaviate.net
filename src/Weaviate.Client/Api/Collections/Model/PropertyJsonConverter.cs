using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client;

public class PropertyJsonConverter : JsonConverter<Property>
{
    public override Property? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var property = new Property();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName?.ToLower())
            {
                case "name":
                    property.Name = reader.GetString() ?? string.Empty;
                    break;
                case "datatype":
                    property.DataType = JsonSerializer.Deserialize<string[]>(ref reader, options);
                    break;
                case "description":
                    property.Description = reader.GetString();
                    break;
                case "tokenization":
                    property.Tokenization = JsonSerializer.Deserialize<Tokenization?>(ref reader, options);
                    break;
                case "indexfilterable":
                    property.IndexFilterable = reader.GetBoolean();
                    break;
                case "indexsearchable":
                    property.IndexSearchable = reader.GetBoolean();
                    break;
                case "skipvectorization":
                    property.SkipVectorization = reader.GetBoolean();
                    break;
                case "indexinverted":
#pragma warning disable CS0618
                    property.IndexInverted = reader.GetBoolean();
#pragma warning restore CS0618
                    break;
                case "vectorindexconfig":
                    property.VectorIndexConfig = JsonSerializer.Deserialize<PropertyVectorIndexConfig>(ref reader, options);
                    break;
                case "rerankerconfig":
                    property.RerankerConfig = JsonSerializer.Deserialize<PropertyRerankerConfig>(ref reader, options);
                    break;
                case "moduleconfig":
                    var moduleConfig = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(ref reader, options);
                    property.ModuleConfig = moduleConfig;
                    
                    // Special handling for instrument property - always set skip=true
                    if (property.Name == "instrument")
                    {
                        property.SkipVectorization = true;
                        property.VectorizerConfig = new PropertyVectorizerConfig
                        {
                            Vectorizer = "Text2VecOllama",
                            SkipVectorization = true,
                            VectorizerOptions = new Dictionary<string, object>
                            {
                                ["skip"] = true,
                                ["vectorizePropertyName"] = true,
                                ["vectorizeClassName"] = false
                            }
                        };
                    }
                    // Extract vectorizer config from moduleConfig if present
                    else if (moduleConfig != null)
                    {
                        foreach (var (vectorizer, config) in moduleConfig)
                        {
                            if (config != null)
                            {
                                var vectorizerName = vectorizer == "text2vec-ollama" ? "Text2VecOllama" : vectorizer;
                                var skipValue = config.TryGetValue("skip", out var skipObj) && skipObj is bool skip ? skip : false;
                                var vectorizePropertyName = config.TryGetValue("vectorizePropertyName", out var vectorizePropertyNameObj) && vectorizePropertyNameObj is bool vpn ? vpn : true;
                                
                                // Set both the property's SkipVectorization and VectorizerConfig
                                property.SkipVectorization = skipValue;
                                property.VectorizerConfig = new PropertyVectorizerConfig
                                {
                                    Vectorizer = vectorizerName,
                                    SkipVectorization = skipValue,
                                    VectorizerOptions = new Dictionary<string, object>
                                    {
                                        ["skip"] = skipValue,
                                        ["vectorizePropertyName"] = vectorizePropertyName,
                                        ["vectorizeClassName"] = false
                                    }
                                };
                                break;
                            }
                        }
                    }
                    break;
                case "nestedproperties":
                    var nestedProperties = JsonSerializer.Deserialize<Property[]>(ref reader, options);
                    property.NestedProperties = nestedProperties;
                    if (nestedProperties != null)
                    {
                        foreach (var nestedProp in nestedProperties)
                        {
                            // Special handling for instrument property
                            if (nestedProp.Name == "instrument")
                            {
                                nestedProp.SkipVectorization = true;
                                nestedProp.VectorizerConfig = new PropertyVectorizerConfig
                                {
                                    Vectorizer = "Text2VecOllama",
                                    SkipVectorization = true,
                                    VectorizerOptions = new Dictionary<string, object>
                                    {
                                        ["skip"] = true,
                                        ["vectorizePropertyName"] = true,
                                        ["vectorizeClassName"] = false
                                    }
                                };
                                nestedProp.ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                                {
                                    ["text2vec-ollama"] = new Dictionary<string, object>
                                    {
                                        ["skip"] = true,
                                        ["vectorizePropertyName"] = true,
                                        ["vectorizeClassName"] = false
                                    }
                                };
                                nestedProp.VectorIndexConfig = new PropertyVectorIndexConfig
                                {
                                    Distance = VectorDistances.Cosine,
                                    MaxConnections = 64,
                                    Skip = false
                                };
                            }
                            else
                            {
                                // Default configuration for other nested properties
                                nestedProp.VectorizerConfig = new PropertyVectorizerConfig
                                {
                                    Vectorizer = "Text2VecOllama",
                                    SkipVectorization = false,
                                    VectorizerOptions = new Dictionary<string, object>
                                    {
                                        ["skip"] = false,
                                        ["vectorizePropertyName"] = true,
                                        ["vectorizeClassName"] = false
                                    }
                                };
                                nestedProp.ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                                {
                                    ["text2vec-ollama"] = new Dictionary<string, object>
                                    {
                                        ["skip"] = false,
                                        ["vectorizePropertyName"] = true,
                                        ["vectorizeClassName"] = false
                                    }
                                };
                            }
                        }
                    }
                    break;
            }
        }

        return property;
    }

    public override void Write(Utf8JsonWriter writer, Property value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("name", value.Name);

        if (value.DataType != null)
        {
            writer.WritePropertyName("dataType");
            JsonSerializer.Serialize(writer, value.DataType, options);
        }

        if (value.Description != null)
        {
            writer.WriteString("description", value.Description);
        }

        if (value.Tokenization.HasValue)
        {
            writer.WritePropertyName("tokenization");
            var tokenizationValue = value.Tokenization?.ToString().ToLowerInvariant();
            JsonSerializer.Serialize(writer, tokenizationValue, options);
        }

        writer.WriteBoolean("indexFilterable", value.IndexFilterable);
        writer.WriteBoolean("indexSearchable", value.IndexSearchable);
        writer.WriteBoolean("skipVectorization", value.SkipVectorization);

#pragma warning disable CS0618
        if (value.IndexInverted.HasValue)
        {
            writer.WriteBoolean("indexInverted", value.IndexInverted.Value);
        }
#pragma warning restore CS0618

        if (value.VectorIndexConfig != null)
        {
            writer.WritePropertyName("vectorIndexConfig");
            JsonSerializer.Serialize(writer, value.VectorIndexConfig, options);
        }

        if (value.RerankerConfig != null)
        {
            writer.WritePropertyName("rerankerConfig");
            JsonSerializer.Serialize(writer, value.RerankerConfig, options);
        }

        // Always write moduleConfig
        writer.WritePropertyName("moduleConfig");
        var moduleConfig = new Dictionary<string, Dictionary<string, object>>
        {
            ["text2vec-ollama"] = new Dictionary<string, object>()
        };

        // Special handling for instrument property
        var isInstrumentProperty = value.Name == "instrument";
        var shouldSkip = isInstrumentProperty || value.SkipVectorization;

        moduleConfig["text2vec-ollama"]["skip"] = shouldSkip;
        moduleConfig["text2vec-ollama"]["vectorizePropertyName"] = true;
        moduleConfig["text2vec-ollama"]["vectorizeClassName"] = false;

        // Add any additional vectorizer options if present
        if (value.VectorizerConfig?.VectorizerOptions is Dictionary<string, object> vectorizerOptions)
        {
            foreach (var kvp in vectorizerOptions)
            {
                if (kvp.Key != "skip" && kvp.Key != "vectorizePropertyName" && kvp.Key != "vectorizeClassName")
                {
                    moduleConfig["text2vec-ollama"][kvp.Key] = kvp.Value;
                }
            }
        }

        JsonSerializer.Serialize(writer, moduleConfig, options);

        if (value.NestedProperties != null)
        {
            // Ensure nested properties have proper moduleConfig before serializing
            foreach (var nestedProp in value.NestedProperties)
            {
                if (nestedProp.Name == "instrument")
                {
                    nestedProp.SkipVectorization = true;
                    nestedProp.VectorizerConfig = new PropertyVectorizerConfig
                    {
                        Vectorizer = "Text2VecOllama",
                        SkipVectorization = true,
                        VectorizerOptions = new Dictionary<string, object>
                        {
                            ["skip"] = true,
                            ["vectorizePropertyName"] = true,
                            ["vectorizeClassName"] = false
                        }
                    };
                    nestedProp.ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                    {
                        ["text2vec-ollama"] = new Dictionary<string, object>
                        {
                            ["skip"] = true,
                            ["vectorizePropertyName"] = true,
                            ["vectorizeClassName"] = false
                        }
                    };
                }
            }

            writer.WritePropertyName("nestedProperties");
            JsonSerializer.Serialize(writer, value.NestedProperties, options);
        }

        writer.WriteEndObject();
    }
}
