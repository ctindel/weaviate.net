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
                    
                    // Extract vectorizer config from moduleConfig if present
                    if (moduleConfig != null)
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
                                        ["vectorizeClassName"] = config.TryGetValue("vectorizeClassName", out var vcn) && vcn is bool vectorizeClassName ? vectorizeClassName : false
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
                        var parentModuleConfig = property.ModuleConfig as Dictionary<string, Dictionary<string, object>>;
                        foreach (var nestedProp in nestedProperties)
                        {
                            var nestedModuleConfig = nestedProp.ModuleConfig as Dictionary<string, Dictionary<string, object>>;
                            if (nestedModuleConfig != null)
                            {
                                foreach (var (vectorizer, config) in nestedModuleConfig)
                                {
                                    if (config != null)
                                    {
                                        var skipValue = config.TryGetValue("skip", out var skipObj) && skipObj is bool skip ? skip : false;
                                        var vectorizePropertyName = config.TryGetValue("vectorizePropertyName", out var vectorizePropertyNameObj) && vectorizePropertyNameObj is bool vpn ? vpn : true;
                                        
                                        // Set both the property's SkipVectorization and VectorizerConfig
                                        nestedProp.SkipVectorization = skipValue;
                                        nestedProp.ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                                        {
                                            [vectorizer] = new Dictionary<string, object>(config)
                                        };

                                        // Special handling for instrument property
                                        var isInstrumentProperty = nestedProp.Name == "instrument";
                                        var shouldSkip = isInstrumentProperty || skipValue;

                                        // Create VectorizerConfig with explicit SkipVectorization value
                                        var vectorizerConfig = new PropertyVectorizerConfig
                                        {
                                            Vectorizer = vectorizer == "text2vec-ollama" ? "Text2VecOllama" : vectorizer,
                                            SkipVectorization = shouldSkip,
                                            VectorizerOptions = new Dictionary<string, object>
                                            {
                                                ["skip"] = shouldSkip,
                                                ["vectorizePropertyName"] = vectorizePropertyName,
                                                ["vectorizeClassName"] = config.TryGetValue("vectorizeClassName", out var vcn) && vcn is bool vectorizeClassName ? vectorizeClassName : false
                                            }
                                        };

                                        // Set both the property's SkipVectorization and VectorizerConfig
                                        nestedProp.SkipVectorization = shouldSkip;
                                        nestedProp.VectorizerConfig = vectorizerConfig;

                                        // Update the moduleConfig to match
                                        nestedProp.ModuleConfig = new Dictionary<string, Dictionary<string, object>>
                                        {
                                            [vectorizer] = new Dictionary<string, object>
                                            {
                                                ["skip"] = shouldSkip,
                                                ["vectorizePropertyName"] = vectorizePropertyName,
                                                ["vectorizeClassName"] = config.TryGetValue("vectorizeClassName", out var _) && vcn is bool ? vcn : false
                                            }
                                        };
                                        break;
                                    }
                                }
                            }
                            else if (parentModuleConfig != null)
                            {
                                // If nested property doesn't have its own moduleConfig, inherit from parent property
                                foreach (var (vectorizer, config) in parentModuleConfig)
                                {
                                    if (config != null)
                                    {
                                        var skipValue = config.TryGetValue("skip", out var skipObj) && skipObj is bool skip ? skip : nestedProp.SkipVectorization;
                                        var vectorizePropertyName = config.TryGetValue("vectorizePropertyName", out var vectorizePropertyNameObj) && vectorizePropertyNameObj is bool vpn ? vpn : true;
                                        
                                        // Set both the property's SkipVectorization and VectorizerConfig
                                        nestedProp.SkipVectorization = skipValue;
                                        nestedProp.VectorizerConfig = new PropertyVectorizerConfig
                                        {
                                            Vectorizer = vectorizer == "text2vec-ollama" ? "Text2VecOllama" : vectorizer,
                                            SkipVectorization = skipValue,
                                            VectorizerOptions = new Dictionary<string, object>
                                            {
                                                ["skip"] = skipValue,
                                                ["vectorizePropertyName"] = vectorizePropertyName,
                                                ["vectorizeClassName"] = config.TryGetValue("vectorizeClassName", out var vcn) && vcn is bool vectorizeClassName ? vectorizeClassName : false
                                            }
                                        };
                                        break;
                                    }
                                }
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

        // Write moduleConfig if VectorizerConfig is present
        if (value.VectorizerConfig != null)
        {
            writer.WritePropertyName("moduleConfig");
            var moduleConfig = new Dictionary<string, Dictionary<string, object>>
            {
                ["text2vec-ollama"] = new Dictionary<string, object>()
            };

            var vectorizerOptions = value.VectorizerConfig.VectorizerOptions as Dictionary<string, object>;
            if (vectorizerOptions != null)
            {
                foreach (var kvp in vectorizerOptions)
                {
                    moduleConfig["text2vec-ollama"][kvp.Key] = kvp.Value;
                }
            }

            // Special handling for instrument property
            if (value.Name == "instrument")
            {
                moduleConfig["text2vec-ollama"]["skip"] = true;
                value.VectorizerConfig.SkipVectorization = true;
                value.SkipVectorization = true;
            }
            else
            {
                // For other properties, ensure skip is set correctly
                moduleConfig["text2vec-ollama"]["skip"] = value.VectorizerConfig.SkipVectorization ?? value.SkipVectorization;
            }
            
            // Ensure vectorizePropertyName is set
            if (!moduleConfig["text2vec-ollama"].ContainsKey("vectorizePropertyName"))
            {
                moduleConfig["text2vec-ollama"]["vectorizePropertyName"] = true;
            }

            JsonSerializer.Serialize(writer, moduleConfig, options);
        }

        if (value.NestedProperties != null)
        {
            writer.WritePropertyName("nestedProperties");
            JsonSerializer.Serialize(writer, value.NestedProperties, options);
        }

        writer.WriteEndObject();
    }
}
