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
                                var skipValue = config.TryGetValue("skip", out var skipObj) && skipObj is bool skip ? skip : property.SkipVectorization;
                                var vectorizePropertyName = config.TryGetValue("vectorizePropertyName", out var vectorizePropertyNameObj) && vectorizePropertyNameObj is bool vpn ? vpn : true;
                                property.VectorizerConfig = new PropertyVectorizerConfig
                                {
                                    Vectorizer = vectorizerName,
                                    SkipVectorization = skipValue,
                                    VectorizerOptions = new Dictionary<string, object>
                                    {
                                        ["skip"] = skipValue,
                                        ["vectorizePropertyName"] = vectorizePropertyName
                                    }
                                };
                                // Also set the skip value in the property itself for consistency
                                property.SkipVectorization = skipValue;
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
                                        var skipValue = config.TryGetValue("skip", out var skipObj) && skipObj is bool skip ? skip : nestedProp.SkipVectorization;
                                        var vectorizePropertyName = config.TryGetValue("vectorizePropertyName", out var vectorizePropertyNameObj) && vectorizePropertyNameObj is bool vpn ? vpn : true;
                                        nestedProp.VectorizerConfig = new PropertyVectorizerConfig
                                        {
                                            Vectorizer = vectorizer == "text2vec-ollama" ? "Text2VecOllama" : vectorizer,
                                            SkipVectorization = skipValue,
                                            VectorizerOptions = new Dictionary<string, object>
                                            {
                                                ["skip"] = skipValue,
                                                ["vectorizePropertyName"] = vectorizePropertyName
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
                                        nestedProp.VectorizerConfig = new PropertyVectorizerConfig
                                        {
                                            Vectorizer = vectorizer == "text2vec-ollama" ? "Text2VecOllama" : vectorizer,
                                            SkipVectorization = skipValue,
                                            VectorizerOptions = new Dictionary<string, object>
                                            {
                                                ["skip"] = skipValue,
                                                ["vectorizePropertyName"] = vectorizePropertyName
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

        if (value.NestedProperties != null)
        {
            writer.WritePropertyName("nestedProperties");
            JsonSerializer.Serialize(writer, value.NestedProperties, options);
        }

        writer.WriteEndObject();
    }
}
