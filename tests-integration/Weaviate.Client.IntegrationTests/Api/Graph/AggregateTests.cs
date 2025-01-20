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
public class AggregateTests : TestBase
{
	[Fact]
	public void Aggregate()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var pizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
		var firstResult = pizzaArray.First();
		Assert.NotNull(firstResult);
		var meta = firstResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(4, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithWhereFilter()
	{
		CreateTestSchemaAndData(Client);

		const string id = "6baed48e-2afe-4be4-a09d-b00a955d96ee";

		var batch = Client.Batch.CreateObjects(new(new WeaviateObject
			{
				Id = id,
				Collection = COLLECTION_NAME_PIZZA,
				Properties = new() { { "name", "JustPizza" }, { "description", "pizza with id" } }
			}
		));
		Assert.Equal(System.Net.HttpStatusCode.OK, batch.HttpStatusCode);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			Where = new() { Path = new[] { "id" }, Operator = Operator.Equal, ValueString = id },
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var pizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
		var firstResult = pizzaArray.First();
		Assert.NotNull(firstResult);
		var meta = firstResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(1, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithGroupedByAndWhere()
	{
		CreateTestSchemaAndData(Client);

		const string id = "6baed48e-2afe-4be4-a09d-b00a955d96ee";

		var batch = Client.Batch.CreateObjects(new(new WeaviateObject
			{
				Id = id,
				Collection = COLLECTION_NAME_PIZZA,
				Properties = new() { { "name", "JustPizza" }, { "description", "pizza with id" } }
			}
		));
		Assert.Equal(System.Net.HttpStatusCode.OK, batch.HttpStatusCode);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			Where = new() { Path = new[] { "id" }, Operator = Operator.Equal, ValueString = id },
			GroupBy = "name",
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var pizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Single(pizzaArray);
		var firstResult = pizzaArray.First();
		Assert.NotNull(firstResult);
		var meta = firstResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(1, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithGroupedBy()
	{
		CreateTestSchemaAndData(Client);

		const string id = "6baed48e-2afe-4be4-a09d-b00a955d96ee";

		var batch = Client.Batch.CreateObjects(new(new WeaviateObject
			{
				Id = id,
				Collection = COLLECTION_NAME_PIZZA,
				Properties = new() { { "name", "JustPizza" }, { "description", "pizza with id" } }
			}
		));
		Assert.Equal(System.Net.HttpStatusCode.OK, batch.HttpStatusCode);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			GroupBy = "name",
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var pizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.Equal(5, pizzaArray.Count);
		var firstResult = pizzaArray.First();
		Assert.NotNull(firstResult);
		var meta = firstResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(1, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithNearVector()
	{
		CreateTestSchemaAndData(Client);

		var vectorResult = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA, Fields = new[] { AdditionalField.Vector.AsAdditionalField() }
		});

		Assert.NotNull(vectorResult.Result?.Data);
		var getData = vectorResult.Result.Data["Get"];
		Assert.NotNull(getData);
		var pizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(pizzaData);
		var pizzaArray = pizzaData.AsArray();
		Assert.NotEmpty(pizzaArray);
		var firstPizza = pizzaArray.First();
		Assert.NotNull(firstPizza);
		var additional = firstPizza["_additional"];
		Assert.NotNull(additional);
		var vectorData = additional["vector"];
		Assert.NotNull(vectorData);
		var vector = vectorData.AsArray().Select(v => v!.GetValue<float>()).ToArray();

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearVector = new() { Vector = vector, Certainty = 0.7f },
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var resultPizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(resultPizzaData);
		var resultArray = resultPizzaData.AsArray();
		Assert.Single(resultArray);
		var firstResult = resultArray.First();
		Assert.NotNull(firstResult);
		var meta = firstResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(4, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithNearObjectAndCertainty()
	{
		CreateTestSchemaAndData(Client);

		var vectorResult = Client.Graph.Get(new()
		{
			Collection = COLLECTION_NAME_PIZZA, Fields = new[] { AdditionalField.Id.AsAdditionalField() }
		});

		Assert.NotNull(vectorResult.Result?.Data);
		var getData = vectorResult.Result.Data["Get"];
		Assert.NotNull(getData);
		var sourcePizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(sourcePizzaData);
		var sourcePizzaArray = sourcePizzaData.AsArray();
		Assert.NotEmpty(sourcePizzaArray);
		var firstPizza = sourcePizzaArray.First();
		Assert.NotNull(firstPizza);
		var additional = firstPizza["_additional"];
		Assert.NotNull(additional);
		var idData = additional["id"];
		Assert.NotNull(idData);
		var id = idData.GetValue<string>();

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearObject = new() { Id = id, Certainty = 0.7f },
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var resultPizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(resultPizzaData);
		var resultPizzaArray = resultPizzaData.AsArray();
		Assert.Single(resultPizzaArray);
		var firstResult = resultPizzaArray.First();
		Assert.NotNull(firstResult);
		var meta = firstResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(4, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithNearObjectAndDistance()
	{
		CreateTestSchemaAndData(Client);

		var vectorResult = Client.Graph.Get(new() { Collection = COLLECTION_NAME_PIZZA, Fields = new[] { "id".AsAdditional() } });

		Assert.NotNull(vectorResult.Result?.Data);
		var getData = vectorResult.Result.Data["Get"];
		Assert.NotNull(getData);
		var distSourcePizzaData = getData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(distSourcePizzaData);
		var distSourcePizzaArray = distSourcePizzaData.AsArray();
		Assert.NotEmpty(distSourcePizzaArray);
		var firstDistPizza = distSourcePizzaArray.First();
		Assert.NotNull(firstDistPizza);
		var additional = firstDistPizza["_additional"];
		Assert.NotNull(additional);
		var idData = additional["id"];
		Assert.NotNull(idData);
		var id = idData.GetValue<string>();

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearObject = new() { Id = id, Distance = 0.3f },
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var distResultPizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(distResultPizzaData);
		var distResultArray = distResultPizzaData.AsArray();
		Assert.Single(distResultArray);
		var firstDistResult = distResultArray.First();
		Assert.NotNull(firstDistResult);
		var meta = firstDistResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(4, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithNearTextAndCertainty()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new() { Concepts = new[] { COLLECTION_NAME_PIZZA }, Certainty = 0.7f },
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var nearTextPizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(nearTextPizzaData);
		var nearTextArray = nearTextPizzaData.AsArray();
		Assert.Single(nearTextArray);
		var firstNearTextResult = nearTextArray.First();
		Assert.NotNull(firstNearTextResult);
		var meta = firstNearTextResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(4, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithNearTextAndDistance()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new() { Concepts = new[] { COLLECTION_NAME_PIZZA }, Distance = 0.6f },
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var nearTextDistPizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(nearTextDistPizzaData);
		var nearTextDistArray = nearTextDistPizzaData.AsArray();
		Assert.Single(nearTextDistArray);
		var firstNearTextDistResult = nearTextDistArray.First();
		Assert.NotNull(firstNearTextDistResult);
		var meta = firstNearTextDistResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(4, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithObjectLimitAndCertainty()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new() { Concepts = new[] { COLLECTION_NAME_PIZZA }, Certainty = 0.7f },
			ObjectLimit = 1,
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var limitCertPizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(limitCertPizzaData);
		var limitCertArray = limitCertPizzaData.AsArray();
		Assert.Single(limitCertArray);
		var firstLimitCertResult = limitCertArray.First();
		Assert.NotNull(firstLimitCertResult);
		var meta = firstLimitCertResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(1, meta["count"]!.GetValue<int>());
	}

	[Fact]
	public void AggregateWithObjectLimitAndDistance()
	{
		CreateTestSchemaAndData(Client);

		var result = Client.Graph.Aggregate(new()
		{
			Collection = COLLECTION_NAME_PIZZA,
			NearText = new() { Concepts = new[] { COLLECTION_NAME_PIZZA }, Distance = 0.3f },
			ObjectLimit = 1,
			Fields = new[] { new Field { Name = "meta", Fields = new[] { new Field { Name = "count" } } } }
		});

		Assert.NotNull(result.Result?.Data);
		var aggregateData = result.Result.Data["Aggregate"];
		Assert.NotNull(aggregateData);
		var limitDistPizzaData = aggregateData[COLLECTION_NAME_PIZZA];
		Assert.NotNull(limitDistPizzaData);
		var limitDistArray = limitDistPizzaData.AsArray();
		Assert.Single(limitDistArray);
		var firstLimitDistResult = limitDistArray.First();
		Assert.NotNull(firstLimitDistResult);
		var meta = firstLimitDistResult["meta"];
		Assert.NotNull(meta);
		Assert.Equal(1, meta["count"]!.GetValue<int>());
	}
}
