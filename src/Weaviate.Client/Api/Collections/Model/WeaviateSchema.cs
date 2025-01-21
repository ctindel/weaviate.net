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

/// <summary>
/// Represents all collections in Weaviate.
/// </summary>
public class WeaviateCollections
{
    /// <summary>
    /// The name of the collections group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The maintainer of the collections.
    /// </summary>
    public string Maintainer { get; set; } = string.Empty;

    /// <summary>
    /// The collections defined in Weaviate.
    /// </summary>
    public List<WeaviateCollection> Collections { get; set; } = new();

    /// <summary>
    /// For backward compatibility with v3 API.
    /// </summary>
    [Obsolete("Use Collections property instead. This property will be removed in v5.")]
    public List<WeaviateCollection> Classes
    {
        get => Collections;
        set => Collections = value;
    }
}
