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

public class BatchResults
{
    public int Successful { get; set; }
    public int Failed { get; set; }
    public long Limit { get; set; }
    public int Matches { get; set; }
    public BatchOutput[]? Objects { get; set; }
    public string Output { get; set; } = BatchOutput.Minimal;
    public BatchResponse<WeaviateObjectResponse>? Results { get; set; }
}
