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

public class WeaviateObjectResponse : WeaviateObject
{
    public string? Status { get; set; }
    public string[]? Errors { get; set; }
    public string? Message { get; set; }  // Added for error handling
    public ObjectResult? Result { get; set; }
}

public class ObjectResult
{
    public string? Status { get; set; }
    public ErrorResponse? Errors { get; set; }
}
