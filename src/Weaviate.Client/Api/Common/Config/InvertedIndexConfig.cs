namespace Weaviate.Client;

public class InvertedIndexConfig
{
    public BM25Config? Bm25 { get; set; }
    public int? CleanupIntervalSeconds { get; set; }
    public bool? IndexTimestamps { get; set; }
    public bool? IndexPropertyLength { get; set; }
    public bool? IndexNullState { get; set; }
    public StopwordsConfig? Stopwords { get; set; }
}
