using System;

namespace Weaviate.Client;

public static class AdditionalField
{
    public static readonly Field Id = "_id".AsAdditional();
    public static readonly Field Vector = "_vector".AsAdditional();
    public static readonly Field Certainty = "_certainty".AsAdditional();
    public static readonly Field CreationTime = "_creationTimeUnix".AsAdditional();
    public static readonly Field LastUpdateTime = "_lastUpdateTimeUnix".AsAdditional();
}
