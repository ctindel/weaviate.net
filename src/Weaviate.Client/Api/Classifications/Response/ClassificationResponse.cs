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

public class ClassificationResponse
{
    public string? Id { get; set; }
    public string[]? BasedOnProperties { get; set; }
    public string[]? ClassifyProperties { get; set; }
    public ClassificationType? Type { get; set; }
    public object? Settings { get; set; }
    public string? Status { get; set; }
}
