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

namespace Weaviate.Client.IntegrationTests.Api.Data;

[Collection("Sequential")]
public class CreateTests : TestBase
{
	[Fact]
	public void Create()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var id1 = "abefd256-8574-442b-9293-9205193737ee";
		var id2 = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

		var newPizza = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = id1,
			Properties = new()
			{
				{ "name", "Hawaii" },
				{ "description", "Universally accepted to be the best pizza ever created." }
			},
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.OK, newPizza.HttpStatusCode);

		var newSoup = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Id = id2,
			Properties = new()
			{
				{ "name", "ChickenSoup" },
				{
					"description",
					"Used by humans when their inferior genetics are attacked by microscopic organisms."
				}
			},
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.OK, newSoup.HttpStatusCode);

		var pizza = Client.Data.Get(new() { Collection = COLLECTION_NAME_PIZZA, Id = id1 });
		Assert.Equal(HttpStatusCode.OK, pizza.HttpStatusCode);
		Assert.NotNull(pizza.Result);
		var pizzaResult = pizza.Result.Single();
		Assert.NotNull(pizzaResult.Properties);
		Assert.Equal("Hawaii", ((JsonElement)pizzaResult.Properties["name"]).GetString());

		var soup = Client.Data.Get(new() { Collection = COLLECTION_NAME_SOUP, Id = id2 });
		Assert.Equal(HttpStatusCode.OK, soup.HttpStatusCode);
		Assert.NotNull(soup.Result);
		var soupResult = soup.Result.Single();
		Assert.NotNull(soupResult.Properties);
		Assert.Equal("ChickenSoup", ((JsonElement)soupResult.Properties["name"]).GetString());
	}

	[Fact]
	public void CreateWithSpecialCharacters()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var id1 = "abefd256-8574-442b-9293-9205193737ee";

		var name = "Zażółć gęślą jaźń";
		var description = "test äüëö";

		var newPizza = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = id1, Properties = new() { { "name", name }, { "description", description } }
		});
		Assert.Equal(HttpStatusCode.OK, newPizza.HttpStatusCode);

		var pizza = Client.Data.Get(new() { Collection = COLLECTION_NAME_PIZZA, Id = id1 });
		Assert.Equal(HttpStatusCode.OK, pizza.HttpStatusCode);
		Assert.NotNull(pizza.Result);
		var pizzaResult = pizza.Result.Single();
		Assert.NotNull(pizzaResult.Properties);
		Assert.Equal(name, ((JsonElement)pizzaResult.Properties["name"]).GetString());
		Assert.Equal(description, ((JsonElement)pizzaResult.Properties["description"]).GetString());
	}

	[Fact]
	public void CreateWithArrayType()
	{
		Client.Collections.DeleteAllCollections();

		const string @class = "CollectionArrays";
		var collection = Client.Collections.CreateCollection(new CreateCollectionRequest(@class)
		{
			Description = "Collection which properties are all array properties",
			VectorIndexType = VectorIndexType.HNSW,
			Vectorizer = Vectorizer.Text2VecContextionary.ToString(),
			Properties = new Property[]
			{
				new() { Name = "stringArray", DataType = new[] { DataType.StringArray } },
				new() { Name = "textArray", DataType = new[] { DataType.TextArray } },
				new() { Name = "intArray", DataType = new[] { DataType.IntArray } },
				new() { Name = "numberArray", DataType = new[] { DataType.NumberArray } },
				new() { Name = "booleanArray", DataType = new[] { DataType.BooleanArray } }
			}
		});
		Assert.Equal(HttpStatusCode.OK, collection.HttpStatusCode);

		var id = "abefd256-8574-442b-9293-9205193737ee";

		var create = Client.Data.Create(new(@class)
		{
			Id = id,
			Properties = new()
			{
				{ "stringArray", new[] { "a", "b" } },
				{ "textArray", new[] { "c", "d" } },
				{ "intArray", new[] { 1, 2 } },
				{ "numberArray", new[] { 3.3f, 4.4f } },
				{ "booleanArray", new[] { true, false } }
			}
		});
		Assert.Equal(HttpStatusCode.OK, create.HttpStatusCode);

		var get = Client.Data.Get(new() { Id = id, Collection = @class });
		Assert.Equal(HttpStatusCode.OK, get.HttpStatusCode);
		Assert.NotNull(get.Result);
		var getResult = get.Result.Single();
		Assert.NotNull(getResult.Properties);
		// TODO: we should have extension methods for this.
		Assert.Equal(new[] { "a", "b" },
			((JsonElement)getResult.Properties["stringArray"]).EnumerateArray().Select(a => a.GetString())
			.ToArray());
		Assert.Equal(new[] { "c", "d" },
			((JsonElement)get.Result.Single().Properties!["textArray"]).EnumerateArray().Select(a => a.GetString())
			.ToArray());
		Assert.Equal(new[] { 1, 2 },
			((JsonElement)get.Result.Single().Properties!["intArray"]).EnumerateArray().Select(a => a.GetInt32())
			.ToArray());
		Assert.Equal(new[] { 3.3f, 4.4f },
			((JsonElement)get.Result.Single().Properties!["numberArray"]).EnumerateArray().Select(a => a.GetSingle())
			.ToArray());
		Assert.Equal(new[] { true, false },
			((JsonElement)get.Result.Single().Properties!["booleanArray"]).EnumerateArray().Select(a => a.GetBoolean())
			.ToArray());
	}

	[Fact]
	public void CreateWithIdInNotUuidFormat()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var newPizza = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = "not-uuid", Properties = new() { { "name", "name" }, { "description", "description" } }
		});
		Assert.Equal(HttpStatusCode.UnprocessableEntity, newPizza.HttpStatusCode);
		Assert.NotNull(newPizza.Error);
		Assert.Contains(
			"id in body must be of type uuid",
			newPizza.Error?.Error?.FirstOrDefault()?.Message ?? string.Empty);
	}
}
