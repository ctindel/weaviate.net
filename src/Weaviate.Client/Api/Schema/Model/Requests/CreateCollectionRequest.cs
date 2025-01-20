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
/// Request to create a new collection.
/// </summary>
public class CreateCollectionRequest : WeaviateCollection
{
    public CreateCollectionRequest(string name)
    {
        Name = name;
    }

    [Obsolete("Use Name property instead. This property will be removed in v5.")]
    public new string Class
    {
        get => Name;
        set => Name = value;
    }
}
