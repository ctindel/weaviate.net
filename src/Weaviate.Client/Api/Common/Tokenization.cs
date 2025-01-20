using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client;

[JsonConverter(typeof(TokenizationJsonConverter))]
public enum Tokenization
{
    Word,
    Whitespace,
    Lowercase,
    Field,
    Gse,
    Trigram,
    KagomeJa,
    KagomeKr,
    Default = Word
}

public class TokenizationJsonConverter : JsonConverter<Tokenization>
{
    public override Tokenization Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()?.ToLowerInvariant();
        return value switch
        {
            "word" => Tokenization.Word,
            "whitespace" => Tokenization.Whitespace,
            "lowercase" => Tokenization.Lowercase,
            "field" => Tokenization.Field,
            "gse" => Tokenization.Gse,
            "trigram" => Tokenization.Trigram,
            "kagome_ja" => Tokenization.KagomeJa,
            "kagome_kr" => Tokenization.KagomeKr,
            _ => Tokenization.Default
        };
    }

    public override void Write(Utf8JsonWriter writer, Tokenization value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            Tokenization.Word => "word",
            Tokenization.Whitespace => "whitespace",
            Tokenization.Lowercase => "lowercase",
            Tokenization.Field => "field",
            Tokenization.Gse => "gse",
            Tokenization.Trigram => "trigram",
            Tokenization.KagomeJa => "kagome_ja",
            Tokenization.KagomeKr => "kagome_kr",
            _ => "word"
        };
        writer.WriteStringValue(stringValue);
    }
}
