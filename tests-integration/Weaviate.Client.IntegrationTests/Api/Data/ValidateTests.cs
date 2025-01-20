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
public class ValidateTests : TestBase
{
	[Fact]
	public void Validate()
	{
		CreateWeaviateTestSchemaFood(Client);

		var pizzaId = "abefd256-8574-442b-9293-9205193737ee";
		var soupId = "565da3b6-60b3-40e5-ba21-e6bfe5dbba91";

		var pizza = Client.Data.Validate(new(pizzaId, COLLECTION_NAME_PIZZA)
		{
			Properties = new()
			{
				{ "name", "Hawaii" },
				{ "description", "Universally accepted to be the best pizza ever created." }
			}
		});
		Assert.Equal(HttpStatusCode.OK, pizza.HttpStatusCode);

		var soup = Client.Data.Validate(new(soupId, COLLECTION_NAME_SOUP)
		{
			Properties = new()
			{
				{ "name", "ChickenSoup" },
				{
					"description",
					"Used by humans when their inferior genetics are attacked by microscopic organisms."
				}
			}
		});
		Assert.Equal(HttpStatusCode.OK, soup.HttpStatusCode);

		var pizza2 = Client.Data.Validate(new(pizzaId, COLLECTION_NAME_PIZZA)
		{
			Properties = new()
			{
				{ "name", "test" },
				{ "description", "Universally accepted to be the best pizza ever created." },
				{ "test", "not existing property" }
			}
		});
		Assert.Equal(HttpStatusCode.UnprocessableEntity, pizza2.HttpStatusCode);
		Assert.Contains(
			"invalid object: no such prop with name 'test' found in collection 'Pizza' in the schema. Check your schema files for which properties in this collection are available",
			pizza2.Error!.Error!.Select(e => e.Message));

		var soup2 = Client.Data.Validate(new(soupId, COLLECTION_NAME_SOUP)
		{
			Properties = new()
			{
				{ "name", "ChickenSoup" },
				{
					"description",
					"Used by humans when their inferior genetics are attacked by microscopic organisms."
				},
				{ "test", "not existing property" }
			}
		});
		Assert.Equal(HttpStatusCode.UnprocessableEntity, soup2.HttpStatusCode);
		Assert.Contains(
			"invalid object: no such prop with name 'test' found in collection 'Soup' in the schema. Check your schema files for which properties in this collection are available",
			soup2.Error!.Error!.Select(e => e.Message));
	}
}
