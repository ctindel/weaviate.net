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
using Xunit;

namespace Weaviate.Client.IntegrationTests.Api.Data;

[Collection("Sequential")]
public class GetTests : TestBase
{
	[Fact]
	public void GetActionsThings()
	{
		CreateWeaviateTestSchemaFood(Client);

		var margherita = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Properties = new() { { "name", "Margherita" }, { "description", "plain" } }
		});
		Assert.Equal(HttpStatusCode.OK, margherita.HttpStatusCode);

		var pepperoni = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Properties = new() { { "name", "Pepperoni" }, { "description", "meat" } }
		});
		Assert.Equal(HttpStatusCode.OK, pepperoni.HttpStatusCode);

		var chicken = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Properties = new() { { "name", "Chicken" }, { "description", "plain" } }
		});
		Assert.Equal(HttpStatusCode.OK, chicken.HttpStatusCode);

		var tofu = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Properties = new() { { "name", "Tofu" }, { "description", "vegetarian" } }
		});
		Assert.Equal(HttpStatusCode.OK, tofu.HttpStatusCode);

		var allResults = Client.Data.Get(new());
		var oneResult = Client.Data.Get(new() { Collection = "Pizza", Limit = 1 });
		Assert.NotNull(oneResult.Result);
		var firstPizzaId = oneResult.Result[0].Id ?? throw new InvalidOperationException("Pizza ID should not be null");
		var afterFirstPizzaObjects = Client.Data.Get(new() { Collection = "Pizza", Limit = 1, After = firstPizzaId });

		var allResultsConvenience = Client.Data.GetAll();

		var singleResult = Client.Data.Get(new() { Limit = 1 });

		Assert.NotNull(allResults.Result);
		Assert.NotNull(allResultsConvenience.Result);
		Assert.NotNull(singleResult.Result);
		Assert.NotNull(afterFirstPizzaObjects.Result);
		Assert.Equal(4, allResults.Result.Length);
		Assert.Equal(4, allResultsConvenience.Result.Length);
		Assert.Single(singleResult.Result);
		Assert.Single(afterFirstPizzaObjects.Result);
	}

	[Fact]
	public void GetWithAdditional()
	{
		CreateWeaviateTestSchemaFood(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var soupId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

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

		var pizza = Client.Data.Get(new()
		{
			Id = pizzaId,
			Collection = COLLECTION_NAME_PIZZA,
			Additional = new()
			{
				"classification", // TODO! can these be strings somewhere?
				"nearestNeighbors",
				"vector"
			}
		});
		Assert.Equal(HttpStatusCode.OK, pizza.HttpStatusCode);
		Assert.NotNull(pizza.Result);
		var pizzaResult = pizza.Result.Single();
		Assert.NotNull(pizzaResult);
		Assert.NotNull(pizzaResult.Additional);
		Assert.Equal(pizzaId, pizzaResult.Id);
		Assert.Null(pizzaResult.Additional["classification"]);
		Assert.NotNull(pizzaResult.Additional["nearestNeighbors"]);
		Assert.NotNull(pizzaResult.Vector);

		var soup = Client.Data.Get(new()
		{
			Id = soupId,
			Collection = COLLECTION_NAME_SOUP,
			Additional = new() { "classification", "nearestNeighbors", "interpretation", "vector" }
		});
		Assert.Equal(HttpStatusCode.OK, soup.HttpStatusCode);
		Assert.NotNull(soup.Result);
		var soupResult = soup.Result.Single();
		Assert.NotNull(soupResult);
		Assert.NotNull(soupResult.Additional);
		Assert.Equal(soupId, soupResult.Id);
		Assert.Null(soupResult.Additional["classification"]);
		Assert.NotNull(soupResult.Additional["nearestNeighbors"]);
		Assert.NotNull(soupResult.Additional["interpretation"]);
		Assert.NotNull(soupResult.Vector);
	}

	[Fact]
	public void GetWithAdditionalError()
	{
		CreateWeaviateTestSchemaFood(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var hawaiian = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = pizzaId,
			Properties = new()
			{
				{ "name", "Hawaii" },
				{ "description", "Universally accepted to be the best pizza ever created." }
			}
		});
		Assert.True(hawaiian.HttpStatusCode == HttpStatusCode.OK);

		var get = Client.Data.Get(new()
		{
			Id = pizzaId, Collection = COLLECTION_NAME_PIZZA, Additional = new() { "featureProjection" }
		});

		Assert.True(get.HttpStatusCode == HttpStatusCode.InternalServerError);
		Assert.NotNull(get.Error);
		Assert.NotNull(get.Error.Error);
		Assert.Contains(
			"get extend: unknown capability: featureProjection",
			get.Error.Error.Select(e => e.Message));
	}

	[Fact]
	public void GetWithVector()
	{
		Client.Schema.DeleteAllCollections();

		const string @class = "ClassCustomVector";
		var schema = Client.Schema.CreateCollection(new CreateCollectionRequest(@class)
		{
			Description = "Collection with custom vector",
			Vectorizer = nameof(Vectorizer.None),
			Properties = new Property[] { new() { Name = "foo", DataType = new[] { DataType.String } } }
		});
		Assert.Equal(HttpStatusCode.OK, schema.HttpStatusCode);

		var id = "abefd256-8574-442b-9293-9205193737ee";
		var floats = new[]
		{
			-0.26736435f, -0.112380296f, 0.29648793f, 0.39212644f, 0.0033650293f, -0.07112332f, 0.07513781f,
			0.22459874f
		};

		var create = Client.Data.Create(new(@class)
		{
			Id = id, Properties = new() { { "foo", "bar" } }, Vector = floats
		});

		var get = Client.Data.Get(new() { Id = id, Collection = @class, Additional = new() { "vector" } });

		Assert.Equal(HttpStatusCode.OK, get.HttpStatusCode);
		Assert.NotNull(get.Result);
		var getResult = get.Result.Single();
		Assert.NotNull(getResult);
		Assert.NotNull(getResult.Vector);
		Assert.Equal(floats.Length, getResult.Vector.Length);
	}

	[Fact]
	public void GetUsingCollectionParameter()
	{
		CreateTestSchemaAndData(Client);

		var all = Client.Data.GetAll();
		Assert.True(all.HttpStatusCode == HttpStatusCode.OK);
		Assert.NotNull(all.Result);
		Assert.Equal(6, all.Result.Length);

		var pizzas = Client.Data.Get(new() { Collection = COLLECTION_NAME_PIZZA });
		Assert.True(pizzas.HttpStatusCode == HttpStatusCode.OK);
		Assert.NotNull(pizzas.Result);
		Assert.Equal(4, pizzas.Result.Length);

		var soups = Client.Data.Get(new() { Collection = COLLECTION_NAME_SOUP });
		Assert.True(soups.HttpStatusCode == HttpStatusCode.OK);
		Assert.NotNull(soups.Result);
		Assert.Equal(2, soups.Result.Length);
	}
}
