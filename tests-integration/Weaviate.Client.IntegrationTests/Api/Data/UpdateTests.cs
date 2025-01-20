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
public class UpdateTests : TestBase
{
	[Fact]
	public void Update()
	{
		CreateWeaviateTestSchemaFood(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var soupId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

		var badPizza = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = pizzaId,
			Properties = new() { { "name", "Hawaii" }, { "description", "Missing description" } },
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.OK, badPizza.HttpStatusCode);

		var badSoup = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Id = soupId,
			Properties = new() { { "name", "Water" }, { "description", "Missing description" } },
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.OK, badSoup.HttpStatusCode);

		var updatePizza = Client.Data.Update(new(pizzaId, COLLECTION_NAME_PIZZA)
		{
			Properties = new()
			{
				{ "name", "Hawaii" },
				{ "description", "Universally accepted to be the best pizza ever created." }
			},
			ConsistencyLevel = ConsistencyLevel.Quorum
		});
		Assert.Equal(HttpStatusCode.OK, updatePizza.HttpStatusCode);

		var updateSoup = Client.Data.Update(new(soupId, COLLECTION_NAME_SOUP)
		{
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
		Assert.Equal(HttpStatusCode.OK, updateSoup.HttpStatusCode);

		var pizza = Client.Data.Get(new() { Collection = COLLECTION_NAME_PIZZA, Id = pizzaId });
		Assert.True(pizza.HttpStatusCode == HttpStatusCode.OK);
		Assert.NotNull(pizza.Result);
		var pizzaResult = pizza.Result.Single();
		Assert.NotNull(pizzaResult.Properties);
		Assert.Equal("Hawaii", ((JsonElement)pizzaResult.Properties["name"]).GetString());

		var soup = Client.Data.Get(new() { Collection = COLLECTION_NAME_SOUP, Id = soupId });
		Assert.True(soup.HttpStatusCode == HttpStatusCode.OK);
		Assert.NotNull(soup.Result);
		var soupResult = soup.Result.Single();
		Assert.NotNull(soupResult.Properties);
		Assert.Equal("ChickenSoup", ((JsonElement)soupResult.Properties["name"]).GetString());
	}

	[Fact]
	public void Merge()
	{
		CreateWeaviateTestSchemaFood(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var soupId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

		var badPizza = Client.Data.Create(new(COLLECTION_NAME_PIZZA)
		{
			Id = pizzaId, Properties = new() { { "name", "Hawaii" }, { "description", "Missing description" } }
		});
		Assert.Equal(HttpStatusCode.OK, badPizza.HttpStatusCode);

		var badSoup = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Id = soupId, Properties = new() { { "name", "ChickenSoup" }, { "description", "Missing description" } }
		});
		Assert.Equal(HttpStatusCode.OK, badSoup.HttpStatusCode);

		var updatePizza = Client.Data.Update(new(pizzaId, COLLECTION_NAME_PIZZA)
		{
			Properties = new() { { "description", "Universally accepted to be the best pizza ever created." } },
			WithMerge = true
		});
		Assert.Equal(HttpStatusCode.NoContent, updatePizza.HttpStatusCode);

		var updateSoup = Client.Data.Update(new(soupId, COLLECTION_NAME_SOUP)
		{
			Properties = new()
			{
				{
					"description",
					"Used by humans when their inferior genetics are attacked by microscopic organisms."
				}
			},
			WithMerge = true
		});
		Assert.Equal(HttpStatusCode.NoContent, updateSoup.HttpStatusCode);

		var pizza = Client.Data.Get(new() { Collection = COLLECTION_NAME_PIZZA, Id = pizzaId });
		Assert.Equal(HttpStatusCode.OK, pizza.HttpStatusCode);
		Assert.NotNull(pizza.Result);
		var pizzaResult = pizza.Result.Single();
		Assert.NotNull(pizzaResult.Properties);
		Assert.Equal("Hawaii", ((JsonElement)pizzaResult.Properties["name"]).GetString());
		Assert.Equal("Universally accepted to be the best pizza ever created.",
			((JsonElement)pizzaResult.Properties["description"]).GetString());

		var soup = Client.Data.Get(new() { Collection = COLLECTION_NAME_SOUP, Id = soupId });
		Assert.Equal(HttpStatusCode.OK, soup.HttpStatusCode);
		Assert.NotNull(soup.Result);
		var soupResult = soup.Result.Single();
		Assert.NotNull(soupResult.Properties);
		Assert.Equal("ChickenSoup", ((JsonElement)soupResult.Properties["name"]).GetString());
		Assert.Equal("Used by humans when their inferior genetics are attacked by microscopic organisms.",
			((JsonElement)soupResult.Properties["description"]).GetString());
	}
}
