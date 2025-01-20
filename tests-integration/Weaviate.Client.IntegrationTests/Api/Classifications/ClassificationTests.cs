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

using System.Net;
using System.Text.Json;
using Xunit;

namespace Weaviate.Client.IntegrationTests.Api.Classifications;

[Collection("Sequential")]
public class ClassificationTests : TestBase
{
	[Fact]
	public void Scheduler()
	{
		CreateClassificationClasses();

		var classifyProperties = new[] { "tagged" };
		var basedOnProperties = new[] { "description" };

		var result1 = Client.Classification.Schedule(new(
			COLLECTION_NAME_PIZZA,
			ClassificationType.Contextual,
			classifyProperties,
			basedOnProperties));

		var result2 = Client.Classification.Schedule(new(
			COLLECTION_NAME_PIZZA,
			ClassificationType.Contextual,
			classifyProperties,
			basedOnProperties)
		{
			WaitForCompletion = true
		});
		Assert.NotNull(result2);
		Assert.NotNull(result2.Result);
		Assert.NotNull(result2.Result.BasedOnProperties);
		Assert.NotNull(result2.Result.ClassifyProperties);
		Assert.Contains("description", result2.Result.BasedOnProperties);
		Assert.Contains("tagged", result2.Result.ClassifyProperties);
	}

	[Fact]
	public void Getter()
	{
		CreateClassificationClasses();

		var classifyProperties = new[] { "tagged" };
		var basedOnProperties = new[] { "description" };

		var result = Client.Classification.Schedule(new(
			COLLECTION_NAME_PIZZA,
			ClassificationType.KNN,
			classifyProperties,
			basedOnProperties)
		{
			Settings = new ParamsKnn { K = 3 }
		});

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Id);
		var knnClassification = Client.Classification.Get(new(result.Result.Id));
		Assert.NotNull(knnClassification.Result);
		Assert.NotNull(knnClassification.Result.Settings);
		Assert.Equal(3, ((JsonElement)knnClassification.Result.Settings).GetProperty("k").GetInt32());
	}

	private void CreateClassificationClasses()
	{
		CreateWeaviateTestSchemaFood(Client);

		var tagProperty = new Property { Name = "name", Description = "name", DataType = new[] { DataType.String } };

		var tagResult = Client.Schema.CreateCollection(new CreateCollectionRequest("Tag")
		{
			Description = "tag for a pizza", Properties = new[] { tagProperty }
		});
		Assert.Equal(HttpStatusCode.OK, tagResult.HttpStatusCode);

		var tagPropertyResult = Client.Schema.CreateProperty(new(COLLECTION_NAME_PIZZA)
		{
			Property = new() { Name = "tagged", Description = "tag of pizza", DataType = new[] { "Tag" } }
		});
		Assert.Equal(HttpStatusCode.OK, tagPropertyResult.HttpStatusCode);

		var id1 = "97fa5147-bdad-4d74-9a81-f8babc811b09";
		var id2 = "97fa5147-bdad-4d74-9a81-f8babc811b19";

		var batchResult = Client.Batch.CreateObjects(new(
			new WeaviateObject
			{
				Id = id1,
				Collection = COLLECTION_NAME_PIZZA,
				Properties = new()
				{
					{ "name", "Quattro Formaggi" },
					{
						"description",
						"Pizza quattro formaggi Italian: [ˈkwattro forˈmaddʒi] (four cheese pizza) is a variety of pizza in Italian cuisine that is topped with a combination of four kinds of cheese, usually melted together, with (rossa, red) or without (bianca, white) tomato sauce. It is popular worldwide, including in Italy,[1] and is one of the iconic items from pizzerias's menus."
					}
				}
			},
			new WeaviateObject
			{
				Id = id2,
				Collection = COLLECTION_NAME_PIZZA,
				Properties = new()
				{
					{ "name", "Frutti di Mare" },
					{
						"description",
						"Frutti di Mare is an Italian type of pizza that may be served with scampi, mussels or squid. It typically lacks cheese, with the seafood being served atop a tomato sauce."
					}
				}
			}
		));
		Assert.Equal(HttpStatusCode.OK, batchResult.HttpStatusCode);

		var tagBatch = Client.Batch.CreateObjects(new(
			new WeaviateObject { Collection = "Tag", Properties = new() { { "name", "vegetarian" } } },
			new WeaviateObject { Collection = "Tag", Properties = new() { { "name", "seafood" } } }
		));
		Assert.Equal(HttpStatusCode.OK, tagBatch.HttpStatusCode);
	}
}
