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

namespace Weaviate.Client.IntegrationTests.Api.Reference;

[Collection("Sequential")]
public class CreateReferencesTests : TestBase
{
	[Fact]
	public void CreateWithReferenceCreate()
	{
		CreateWeaviateTestSchemaFoodWithReferenceProperty(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var soupId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

		CreateAndVerifyReferences(pizzaId, soupId);
	}

	[Fact]
	public void CreateWithReferenceReplace()
	{
		CreateWeaviateTestSchemaFoodWithReferenceProperty(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var soupId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

		CreateAndVerifyReferences(pizzaId, soupId);

		var soupReference = Client.Reference.Replace(new ReplaceReferenceRequest(
			pizzaId,
			COLLECTION_NAME_PIZZA,
			"otherFoods",
			ReferenceApi.CreateReference(COLLECTION_NAME_PIZZA, pizzaId))
		{
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.OK, soupReference.HttpStatusCode);

		var pizzaReference = Client.Reference.Replace(new ReplaceReferenceRequest(
			soupId,
			COLLECTION_NAME_SOUP,
			"otherFoods",
			ReferenceApi.CreateReference(COLLECTION_NAME_SOUP, soupId))
		{
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.OK, pizzaReference.HttpStatusCode);

		var pizzaRef = Client.Data.Get(new() { Id = pizzaId, Collection = COLLECTION_NAME_PIZZA });
		Assert.Equal(HttpStatusCode.OK, pizzaRef.HttpStatusCode);
		Assert.NotNull(pizzaRef.Result);
		CheckReference(pizzaRef.Result, COLLECTION_NAME_PIZZA, pizzaId);

		var soupRef = Client.Data.Get(new() { Id = soupId, Collection = COLLECTION_NAME_SOUP });
		Assert.Equal(HttpStatusCode.OK, soupRef.HttpStatusCode);
		Assert.NotNull(soupRef.Result);
		CheckReference(soupRef.Result, COLLECTION_NAME_SOUP, soupId);
	}

	[Fact]
	public void CreateWithReferenceDelete()
	{
		CreateWeaviateTestSchemaFoodWithReferenceProperty(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var soupId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

		CreateAndVerifyReferences(pizzaId, soupId);

		var soupReference = Client.Reference.DeleteReference(new DeleteReferenceRequest(
			pizzaId,
			COLLECTION_NAME_PIZZA,
			"otherFoods",
			ReferenceApi.CreateReference(COLLECTION_NAME_SOUP, soupId))
		{
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.NoContent, soupReference.HttpStatusCode);

		var pizzaReference = Client.Reference.DeleteReference(new DeleteReferenceRequest(
			soupId,
			COLLECTION_NAME_SOUP,
			"otherFoods",
			ReferenceApi.CreateReference(COLLECTION_NAME_PIZZA, pizzaId))
		{
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.NoContent, pizzaReference.HttpStatusCode);

		var pizzaRef = Client.Data.Get(new() { Id = pizzaId, Collection = COLLECTION_NAME_PIZZA });
		Assert.Equal(HttpStatusCode.OK, pizzaRef.HttpStatusCode);
		Assert.NotNull(pizzaRef.Result);
		Assert.Empty(((JsonElement)pizzaRef.Result.Single().Properties!["otherFoods"])
			.EnumerateArray().Select(a => a).ToArray());

		var soupRef = Client.Data.Get(new() { Id = soupId, Collection = COLLECTION_NAME_SOUP });
		Assert.Equal(HttpStatusCode.OK, soupRef.HttpStatusCode);
		Assert.NotNull(soupRef.Result);
		Assert.Empty(((JsonElement)soupRef.Result.Single().Properties!["otherFoods"])
			.EnumerateArray().Select(a => a).ToArray());
	}

	[Fact]
	public void CreateWithAddReferenceUsingProperties()
	{
		CreateWeaviateTestSchemaFoodWithReferenceProperty(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var beaconId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba92";

		var beacon = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = beaconId,
			Properties = new()
			{
				{ "name", "Hawaii" },
				{ "description", "Universally accepted to be the best pizza ever created." }
			}
		});
		Assert.Equal(HttpStatusCode.OK, beacon.HttpStatusCode);

		var pizza = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = pizzaId,
			Properties = new()
			{
				{ "name", "Hawaii" },
				{ "description", "Universally accepted to be the best pizza ever created." },
				{ "otherFoods", new[] { new SingleRef(COLLECTION_NAME_PIZZA, beaconId) } }
			}
		});
		Assert.Equal(HttpStatusCode.OK, pizza.HttpStatusCode);

		var pizzaRef = Client.Data.Get(new() { Id = pizzaId, Collection = COLLECTION_NAME_PIZZA });
		Assert.Equal(HttpStatusCode.OK, pizzaRef.HttpStatusCode);
		Assert.NotNull(pizzaRef.Result);
		CheckReference(pizzaRef.Result, COLLECTION_NAME_PIZZA, beaconId);
	}

	private void CreateAndVerifyReferences(string pizzaId, string soupId)
	{
		var hawaiian = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = pizzaId,
			Properties = new()
			{
				{ "name", "Hawaii" },
				{ "description", "Universally accepted to be the best pizza ever created." }
			}
		});
		Assert.Equal(HttpStatusCode.OK, hawaiian.HttpStatusCode);

		var chickensoup = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Id = soupId,
			Properties = new()
			{
				{ "name", "ChickenSoup" },
				{
					"description",
					"Used by humans when their inferior genetics are attacked by microscopic organisms."
				}
			}
		});
		Assert.Equal(HttpStatusCode.OK, chickensoup.HttpStatusCode);

		var soupReference = Client.Reference.Create(new CreateReferenceRequest(
			pizzaId,
			COLLECTION_NAME_PIZZA,
			"otherFoods",
			new SingleRef(COLLECTION_NAME_SOUP, soupId)));
		Assert.Equal(HttpStatusCode.OK, soupReference.HttpStatusCode);

		var pizzaReference = Client.Reference.Create(new CreateReferenceRequest(
			soupId,
			COLLECTION_NAME_SOUP,
			"otherFoods",
			new SingleRef(COLLECTION_NAME_PIZZA, pizzaId)));
		Assert.Equal(HttpStatusCode.OK, pizzaReference.HttpStatusCode);

		var pizzaRef = Client.Data.Get(new() { Id = pizzaId, Collection = COLLECTION_NAME_PIZZA });
		Assert.Equal(HttpStatusCode.OK, pizzaRef.HttpStatusCode);
		Assert.NotNull(pizzaRef.Result);
		CheckReference(pizzaRef.Result, COLLECTION_NAME_SOUP, soupId);

		var soupRef = Client.Data.Get(new() { Id = soupId, Collection = COLLECTION_NAME_SOUP });
		Assert.Equal(HttpStatusCode.OK, soupRef.HttpStatusCode);
		Assert.NotNull(soupRef.Result);
		CheckReference(soupRef.Result, COLLECTION_NAME_PIZZA, pizzaId);
	}

	private static void CheckReference(IEnumerable<WeaviateObject> result, string className, string refId)
	{
		Assert.NotNull(result);
		var weaviateObject = result.Single();
		Assert.NotNull(weaviateObject);
		Assert.NotNull(weaviateObject.Properties);
		Assert.True(weaviateObject.Properties.ContainsKey("otherFoods"), "Properties should contain 'otherFoods'");
		var jsonElement = (JsonElement)weaviateObject.Properties["otherFoods"];
		var otherFoods = jsonElement.EnumerateArray().Select(a => a).ToArray();
		Assert.NotEmpty(otherFoods);
		var otherFood = otherFoods.Single();
		var beacon = otherFood.GetProperty("beacon").GetString();
		var href = otherFood.GetProperty("href").GetString();
		Assert.NotNull(beacon);
		Assert.NotNull(href);
		Assert.Equal("weaviate://localhost/" + className + "/" + refId, beacon);
		Assert.Equal("/v1/objects/" + className + "/" + refId, href);
	}
}
