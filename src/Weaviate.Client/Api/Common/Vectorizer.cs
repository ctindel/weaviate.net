namespace Weaviate.Client;

public enum Vectorizer
{
    None,
    Text2VecContextionary,
    Text2VecTransformers,
    Text2VecOpenAI,
    Text2VecHuggingFace,
    Text2VecPalm,
    Text2VecCohere,
    Text2VecAWS,
    Text2VecGoogleGenAI,
    Text2VecOllama,
    Img2Vec,
    Img2VecNeural,
    Multi2Vec,
    Ref2Vec,
    Custom
}

public static class VectorizerExtensions
{
    public static string ToWeaviateString(this Vectorizer vectorizer)
    {
        return vectorizer switch
        {
            Vectorizer.None => "none",
            Vectorizer.Text2VecContextionary => "text2vec-contextionary",
            Vectorizer.Text2VecTransformers => "text2vec-transformers",
            Vectorizer.Text2VecOpenAI => "text2vec-openai",
            Vectorizer.Text2VecHuggingFace => "text2vec-huggingface",
            Vectorizer.Text2VecPalm => "text2vec-palm",
            Vectorizer.Text2VecCohere => "text2vec-cohere",
            Vectorizer.Text2VecAWS => "text2vec-aws",
            Vectorizer.Text2VecGoogleGenAI => "text2vec-googlegenai",
            Vectorizer.Text2VecOllama => "text2vec-ollama",
            Vectorizer.Img2Vec => "img2vec",
            Vectorizer.Img2VecNeural => "img2vec-neural",
            Vectorizer.Multi2Vec => "multi2vec",
            Vectorizer.Ref2Vec => "ref2vec",
            Vectorizer.Custom => "custom",
            _ => throw new ArgumentOutOfRangeException(nameof(vectorizer))
        };
    }

    public static Vectorizer FromWeaviateString(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "none" => Vectorizer.None,
            "text2vec-contextionary" => Vectorizer.Text2VecContextionary,
            "text2vec-transformers" => Vectorizer.Text2VecTransformers,
            "text2vec-openai" => Vectorizer.Text2VecOpenAI,
            "text2vec-huggingface" => Vectorizer.Text2VecHuggingFace,
            "text2vec-palm" => Vectorizer.Text2VecPalm,
            "text2vec-cohere" => Vectorizer.Text2VecCohere,
            "text2vec-aws" => Vectorizer.Text2VecAWS,
            "text2vec-googlegenai" => Vectorizer.Text2VecGoogleGenAI,
            "text2vec-ollama" => Vectorizer.Text2VecOllama,
            "img2vec" => Vectorizer.Img2Vec,
            "img2vec-neural" => Vectorizer.Img2VecNeural,
            "multi2vec" => Vectorizer.Multi2Vec,
            "ref2vec" => Vectorizer.Ref2Vec,
            "custom" => Vectorizer.Custom,
            _ => throw new ArgumentException($"Unknown vectorizer value: {value}", nameof(value))
        };
    }

    public static string ToString(this Vectorizer vectorizer) => vectorizer.ToWeaviateString();
}
