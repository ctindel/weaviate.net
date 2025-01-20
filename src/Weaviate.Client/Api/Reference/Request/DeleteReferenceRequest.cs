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

// ReSharper disable once CheckNamespace
namespace Weaviate.Client;

public class DeleteReferenceRequest
{
    public DeleteReferenceRequest(string id, string collection, string referenceProperty, object referencePayload)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        ReferenceProperty = referenceProperty ?? throw new ArgumentNullException(nameof(referenceProperty));
        ReferencePayload = referencePayload ?? throw new ArgumentNullException(nameof(referencePayload));
#pragma warning disable CS0618 // Type or member is obsolete
        Class = Collection;  // For backward compatibility
#pragma warning restore CS0618
    }

    public string Id { get; }
    [Obsolete("Use Collection instead. This property will be removed in v5.")]
    public string? Class { get; }
    public string Collection { get; }
    public string ReferenceProperty { get; }
    public object ReferencePayload { get; }
    public ConsistencyLevel? ConsistencyLevel { get; set; }
}
