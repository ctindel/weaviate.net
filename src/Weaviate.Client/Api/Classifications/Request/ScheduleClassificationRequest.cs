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

public class ScheduleClassificationRequest
{
    public ScheduleClassificationRequest(string collection, ClassificationType classificationType, string[] classifyProperties, string[] basedOnProperties)
    {
        Collection = collection ?? throw new ArgumentNullException(nameof(collection));
        ClassificationType = classificationType;
        ClassifyProperties = classifyProperties ?? throw new ArgumentNullException(nameof(classifyProperties));
        BasedOnProperties = basedOnProperties ?? throw new ArgumentNullException(nameof(basedOnProperties));
    }

    public string Collection { get; }
    public ClassificationType ClassificationType { get; }
    public string[] ClassifyProperties { get; }
    public string[] BasedOnProperties { get; }
    public object? Settings { get; init; }
    public bool? WaitForCompletion { get; init; }
}
