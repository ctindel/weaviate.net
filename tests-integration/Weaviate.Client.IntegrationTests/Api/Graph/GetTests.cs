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

namespace Weaviate.Client.IntegrationTests.Api.Graph;

[Collection("Sequential")]
public class GetTests : TestBase
{
	[Fact]
	public void Get()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new() { Collection = COLLECTION_NAME_PIZZA, Fields = "name".AsFields() });
		Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);
		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Equal(4, pizzaArray.Count);
	}

	[Fact]
	public void NearObjectAndCertainty()
	{
		CreateTestSchemaAndData(Client);

		const string id = "6baed48e-2afe-4be4-a09d-b00a955d962b";

		var newSoup = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Id = id, Properties = new() { { "name", "JustSoup" }, { "description", "soup with id" } }
		});
		Assert.True(newSoup.HttpStatusCode == HttpStatusCode.OK);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_SOUP,
			NearObject = new() { Id = id, Certainty = 0.99f },
			Fields = new[] { "name".AsField(), AdditionalField.Certainty.AsAdditionalField() }
		});

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var soupData = getData[COLLECTION_NAME_SOUP];
		Assert.NotNull(soupData);
		var soupArray = soupData.AsArray();
		Assert.Single(soupArray);
	}

	[Fact]
	public void NearObjectAndDistance()
	{
		CreateTestSchemaAndData(Client);

		const string id = "6baed48e-2afe-4be4-a09d-b00a955d962b";

		var newSoup = Client.Data.Create(new(COLLECTION_NAME_SOUP)
		{
			Id = id, Properties = new() { { "name", "JustSoup" }, { "description", "soup with id" } }
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, newSoup.HttpStatusCode);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_SOUP,
			NearObject = new() { Id = id, Distance = 0.01f },
			Fields = new[] { "name".AsField(), AdditionalField.Certainty.AsAdditionalField() }
		});

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var soupData = getData[COLLECTION_NAME_SOUP];
		Assert.NotNull(soupData);
		var soupArray = soupData.AsArray();
		Assert.Single(soupArray);
	}

	[Fact]
	public void BM25()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			BM25 = new() { Query = "innovation", Properties = new[] { "description" } },
			Fields = new[] { "description".AsField(), AdditionalField.Id.AsAdditionalField() }
		});
		Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);
		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
		var firstPizza = pizzaArray.First();
		Assert.NotNull(firstPizza);
		var description = firstPizza["description"];
		Assert.NotNull(description);
		Assert.Contains("innovation", description.GetValue<string>());
	}

	[Fact]
	public void Hybrid()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			Hybrid = new() { Query = "some say revolution", Alpha = 0.8f },
			Fields = new[] { "description".AsField(), AdditionalField.Id.AsAdditionalField() }
		});
		Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);
		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.NotNull(pizzaArray);
		Assert.Equal(4, pizzaArray.Count);
	}

	[Fact]
	public void NearTextAndCertainty()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new()
			{
				Concepts = new[] { "some say revolution" },
				MoveAwayFrom = new() { Concepts = new[] { "Universally" }, Force = 0.8f },
				Certainty = 0.8f
			},
			Fields = new[] { "name".AsField(), AdditionalField.Certainty.AsAdditionalField() }
		});
		Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);

		Assert.NotNull(result.Result?.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
	}

	[Fact]
	public void NearTextAndDistance()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new()
			{
				Concepts = new[] { "some say revolution" },
				MoveAwayFrom = new() { Concepts = new[] { "Universally" }, Force = 0.8f },
				Distance = 0.4f
			},
			Fields = new[] { "name".AsField(), AdditionalField.Certainty.AsAdditionalField() }
		});
		Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);

		Assert.NotNull(result.Result?.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
	}

	[Fact]
	public void NearTextAndMoveParamsAndCertainty()
	{
		CreateTestSchemaAndData(Client);

		const string id1 = "6baed48e-2afe-4be4-a09d-b00a955d962b";
		const string id2 = "6baed48e-2afe-4be4-a09d-b00a955d962a";

		var batch = Client.Batch.CreateObjects(new(
			new WeaviateObject
			{
				Id = id1,
				Collection = COLLECTION_NAME_PIZZA,
				Properties = new() { { "name", "JustPizza1" }, { "description", "Universally pizza with id" } }
			},
			new WeaviateObject
			{
				Id = id2,
				Collection = COLLECTION_NAME_PIZZA,
				Properties = new()
				{
					{ "name", "JustPizza2" }, { "description", "Universally pizza with some other id" }
				}
			}
		));
		Assert.Equal(System.Net.HttpStatusCode.OK, batch.HttpStatusCode);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new()
			{
				Concepts = new[] { "Universally pizza with id" },
				MoveAwayFrom = new() { Objects = new ObjectMove[] { new() { Id = id1 } }, Force = 0.9f },
				MoveTo = new() { Objects = new ObjectMove[] { new() { Id = id2 } }, Force = 0.9f },
				Certainty = 0.4f
			},
			Fields = new[] { "name".AsField(), AdditionalField.Certainty.AsAdditionalField() }
		});

		Assert.NotNull(result.Result?.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Equal(6, pizzaArray.Count);
	}

	[Fact]
	public void NearTextAndLimitAndCertainty()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new() { Concepts = new[] { "some say revolution" }, Certainty = 0.8f },
			Limit = 1,
			Fields = new[] { "name".AsField(), AdditionalField.Certainty.AsAdditionalField() }
		});
		Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);

		Assert.NotNull(result.Result?.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
	}

	[Fact]
	public void NearTextAndLimitAndDistance()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new() { Concepts = new[] { "some say revolution" }, Distance = 0.4f },
			Limit = 1,
			Fields = new[] { "name".AsField(), AdditionalField.Certainty.AsAdditionalField() }
		});
		Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);

		Assert.NotNull(result.Result?.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
	}

	[Fact]
	public void WhereByFieldTokenizedProperty()
	{
		CreateTestSchemaAndData(Client);

		void AssertCount(int count, ApiResponse<GraphResponse> result)
		{
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Data);
			var getData = result.Result.Data["Get"];
			Assert.NotNull(getData);
			var pizzaData = getData[COLLECTION_NAME_PIZZA];
			Assert.NotNull(pizzaData);
			var pizzaArray = pizzaData.AsArray();
			Assert.Equal(count, pizzaArray.Count);
		}

		AssertCount(1,
			Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Where = new()
				{
					Path = new[] { "name" }, Operator = Operator.Equal, ValueString = "Frutti di Mare"
				},
				Fields = new[] { "name".AsField() }
			}));

		AssertCount(0,
			Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Where = new() { Path = new[] { "name" }, Operator = Operator.Equal, ValueString = "Frutti" },
				Fields = new[] { "name".AsField() }
			}));

		AssertCount(1,
			Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Where = new()
				{
					Path = new[] { "description" },
					Operator = Operator.Equal,
					ValueText = "Universally accepted to be the best pizza ever created."
				},
				Fields = new[] { "name".AsField() }
			}));

		AssertCount(1,
			Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Where = new()
				{
					Path = new[] { "description" }, Operator = Operator.Equal, ValueText = "Universally"
				},
				Fields = new[] { "name".AsField() }
			}));
	}

	[Fact]
	public void WhereByDate()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			Where = new()
			{
				Path = new[] { "bestBefore" },
				Operator = Operator.GreaterThan,
				ValueDate = DateTime.Parse("2022-02-01T00:00:00+0100")
			},
			Fields = new[] { "name".AsField() }
		});

		Assert.NotNull(result.Result?.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Equal(3, pizzaArray.Count);
	}

	[Fact]
	public void Group()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_SOUP,
			Group = new() { Type = GroupType.Merge, Force = 1.0f },
			Limit = 7,
			Fields = "name".AsFields()
		});

		Assert.NotNull(result.Result?.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var soupData = getData[COLLECTION_NAME_SOUP];
		Assert.NotNull(soupData);
		var soupArray = soupData.AsArray();
		Assert.Single(soupArray);
	}

	[Fact]
	public void Sort()
	{
		CreateTestSchemaAndData(Client);

		void AssertOrder(IReadOnlyList<string> order, ApiResponse<GraphResponse> result)
		{
			Assert.NotNull(result.Result);
			Assert.NotNull(result.Result.Data);
			var getData = result.Result.Data["Get"];
			Assert.NotNull(getData);
			var pizzaData = getData[COLLECTION_NAME_PIZZA];
			Assert.NotNull(pizzaData);
			var pizzas = pizzaData.AsArray();
			Assert.Equal(order.Count, pizzas.Count);
			for (var index = 0; index < pizzas.Count; index++)
			{
				var pizza = pizzas[(Index)index];
				Assert.NotNull(pizza);
				var name = pizza["name"];
				Assert.NotNull(name);
				Assert.Equal(order[index], name.GetValue<string>());
			}
		}

		AssertOrder(new[] { "Quattro Formaggi", "Hawaii", "Frutti di Mare", "Doener" },
			Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Sorts = new[] { new Sort { Path = new[] { "name" }, Order = SortOrder.Desc } },
				Fields = "name".AsFields()
			}));

		AssertOrder(new[] { "Hawaii", "Doener", "Quattro Formaggi", "Frutti di Mare" },
			Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Sorts = new[] { new Sort { Path = new[] { "price" }, Order = SortOrder.Asc } },
				Fields = "name".AsFields()
			}));

		AssertOrder(new[] { "Quattro Formaggi", "Hawaii", "Frutti di Mare", "Doener" },
			Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Sorts = new[]
				{
					new Sort { Path = new[] { "name" }, Order = SortOrder.Desc },
					new Sort { Path = new[] { "price" }, Order = SortOrder.Asc }
				},
				Fields = "name".AsFields()
			}));
	}

	[Fact]
	public void GetWithTimestampFilters()
	{
		CreateTestSchemaAndData(Client);

		var fields = new[]
		{
			AdditionalField.Id.AsAdditionalField(),
			AdditionalField.CreationTime.AsAdditionalField(),
			AdditionalField.LastUpdateTime.AsAdditionalField()
		};

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			Fields = fields
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, result.HttpStatusCode);

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var jsonArray = pizzaData.AsArray();
		Assert.Equal(4, jsonArray.Count);

		foreach (var json in jsonArray)
		{
			Assert.NotNull(json);
			var additional = json["_additional"];
			Assert.NotNull(additional);
			var creationTimeUnix = additional["creationTimeUnix"];
			Assert.NotNull(creationTimeUnix);
			var lastUpdateTimeUnix = additional["lastUpdateTimeUnix"];
			Assert.NotNull(lastUpdateTimeUnix);

			var createdResult = Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Where = new()
				{
					Path = new[] { "_creationTimeUnix" },
					Operator = Operator.Equal,
					ValueString = creationTimeUnix.ToString()
				},
				Fields = fields
			});
			Assert.Equal(System.Net.HttpStatusCode.OK, createdResult.HttpStatusCode);

			Assert.NotNull(createdResult.Result);
			Assert.NotNull(createdResult.Result.Data);
			var createdGetData = createdResult.Result.Data["Get"];
			Assert.NotNull(createdGetData);
			var createdPizzaData = createdGetData[COLLECTION_NAME_PIZZA];
			Assert.NotNull(createdPizzaData);
			var createdArray = createdPizzaData.AsArray();
			foreach (var item in createdArray)
			{
				Assert.NotNull(item);
				var itemAdditional = item["_additional"];
				Assert.NotNull(itemAdditional);
				var itemCreationTime = itemAdditional["creationTimeUnix"];
				Assert.NotNull(itemCreationTime);
				Assert.Equal(creationTimeUnix.ToString(), itemCreationTime.ToString());
			}

			var updatedResult = Client.Graph.Get(new()
			{
				Collection = COLLECTION_NAME_PIZZA,
				Where = new()
				{
					Path = new[] { "_lastUpdateTimeUnix" },
					Operator = Operator.Equal,
					ValueString = lastUpdateTimeUnix.ToString()
				},
				Fields = fields
			});
			Assert.Equal(System.Net.HttpStatusCode.OK, updatedResult.HttpStatusCode);

			Assert.NotNull(updatedResult.Result);
			Assert.NotNull(updatedResult.Result.Data);
			var updatedGetData = updatedResult.Result.Data["Get"];
			Assert.NotNull(updatedGetData);
			var updatedPizzaData = updatedGetData[COLLECTION_NAME_PIZZA];
			Assert.NotNull(updatedPizzaData);
			var updatedArray = updatedPizzaData.AsArray();
			foreach (var item in updatedArray)
			{
				Assert.NotNull(item);
				var itemAdditional = item["_additional"];
				Assert.NotNull(itemAdditional);
				var itemUpdateTime = itemAdditional["lastUpdateTimeUnix"];
				Assert.NotNull(itemUpdateTime);
				Assert.Equal(lastUpdateTimeUnix.ToString(), itemUpdateTime.ToString());
			}
		}
	}

	[Fact]
	public void GetUsingCursorAPI()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			After = "00000000-0000-0000-0000-000000000000",
			Limit = 10,
			Fields = "name".AsFields()
		});

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Equal(3, pizzaArray.Count);
	}

	[Fact]
	public void GetUsingLimitAndOffset()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			Offset = 3,
			Limit = 4,
			Fields = "name".AsFields()
		});

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var getData = result.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
	}
}
