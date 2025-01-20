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

namespace Weaviate.Client.IntegrationTests.Api.Batch;

[Collection("Sequential")]
public class DeleteTests : TestBase
{
	[Fact]
	public void DeleteDryRunVerbose()
	{
		CreateTestSchemaAndData(Client);

		var getAllResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getAllResponse.HttpStatusCode);
		Assert.NotNull(getAllResponse.Result);
		var allWeaviateObjects = getAllResponse.Result.Length;

		var dryRun = Client.Batch.DeleteObjects(new()
		{
			Where = new() { Operator = Operator.Equal, Path = new[] { "name" }, ValueString = "Hawaii" },
			DryRun = true,
			Collection = COLLECTION_NAME_PIZZA,
			Output = BatchOutput.Verbose,
			ConsistencyLevel = ConsistencyLevel.Quorum
		});

		Assert.Equal(HttpStatusCode.OK, dryRun.HttpStatusCode);

		var getRemainingResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getRemainingResponse.HttpStatusCode);
		Assert.NotNull(getRemainingResponse.Result);
		var remainingWeaviateObjects = getRemainingResponse.Result.Length;

		Assert.Equal(remainingWeaviateObjects, allWeaviateObjects);

		Assert.NotNull(dryRun.Result);
		Assert.NotNull(dryRun.Result.Results);
		Assert.Equal(BatchOutput.Verbose, dryRun.Result.Results.Output);
		Assert.Equal(0, dryRun.Result.Results.Successful);
		Assert.Equal(0, dryRun.Result.Results.Failed);
		Assert.Equal(10000L, dryRun.Result.Results.Limit);
		Assert.Equal(1, dryRun.Result.Results.Matches);
		Assert.NotNull(dryRun.Result.Results.Objects);
		Assert.Single(dryRun.Result.Results.Objects);
		var firstObject = dryRun.Result.Results.Objects.First();
		Assert.Equal(PIZZA_HAWAII_ID, firstObject.Id);
		Assert.Equal(BatchResultStatus.DryRun.ToWeaviateString(), firstObject.Status);
		Assert.Null(firstObject.Errors);
	}

	[Fact]
	public void DeleteDryRunMinmal()
	{
		CreateTestSchemaAndData(Client);

		var getAllResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getAllResponse.HttpStatusCode);
		Assert.NotNull(getAllResponse.Result);
		var allWeaviateObjects = getAllResponse.Result.Length;

		var dryRun = Client.Batch.DeleteObjects(new()
		{
			Where = new() { Operator = Operator.Equal, Path = new[] { "name" }, ValueString = "Hawaii" },
			DryRun = true,
			Collection = COLLECTION_NAME_PIZZA,
			Output = BatchOutput.Minimal,
			ConsistencyLevel = ConsistencyLevel.Quorum
		});

		Assert.Equal(HttpStatusCode.OK, dryRun.HttpStatusCode);

		var getRemainingResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getRemainingResponse.HttpStatusCode);
		Assert.NotNull(getRemainingResponse.Result);
		var remainingWeaviateObjects = getRemainingResponse.Result.Length;

		Assert.Equal(remainingWeaviateObjects, allWeaviateObjects);

		Assert.NotNull(dryRun.Result);
		Assert.Equal(BatchOutput.Minimal, dryRun.Result.Output);
		Assert.NotNull(dryRun.Result.Results);
		Assert.Equal(0, dryRun.Result.Results.Successful);
		Assert.Equal(0, dryRun.Result.Results.Failed);
		Assert.Equal(10000L, dryRun.Result.Results.Limit);
		Assert.Equal(1, dryRun.Result.Results.Matches);
		Assert.Null(dryRun.Result.Results.Objects);
	}

	[Fact]
	public void DeleteNoMatchWithDefaultOutputAndDryRun()
	{
		CreateTestSchemaAndData(Client);

		var getAllResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getAllResponse.HttpStatusCode);
		Assert.NotNull(getAllResponse.Result);
		var allWeaviateObjects = getAllResponse.Result.Length;

		var batch = Client.Batch.DeleteObjects(new()
		{
			Where = new()
			{
				Operator = Operator.GreaterThan,
				Path = new[] { "_creationTimeUnix" },
				ValueString = DateTimeOffset.Now.AddSeconds(60).ToUnixTimeMilliseconds().ToString()
			},
			Collection = COLLECTION_NAME_PIZZA,
			ConsistencyLevel = ConsistencyLevel.Quorum
		});

		Assert.Equal(HttpStatusCode.OK, batch.HttpStatusCode);

		var getRemainingResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getRemainingResponse.HttpStatusCode);
		Assert.NotNull(getRemainingResponse.Result);
		var remainingWeaviateObjects = getRemainingResponse.Result.Length;

		Assert.Equal(remainingWeaviateObjects, allWeaviateObjects);

		Assert.NotNull(batch.Result);
		Assert.NotNull(batch.Result.Results);
		Assert.Equal(BatchOutput.Minimal, batch.Result.Results.Output);
		Assert.Equal(0, batch.Result.Results.Successful);
		Assert.Equal(0, batch.Result.Results.Failed);
		Assert.Equal(10000L, batch.Result.Results.Limit);
		Assert.Equal(0, batch.Result.Results.Matches);
		Assert.Null(batch.Result.Results.Objects);
	}

	[Fact]
	public void DeleteNoMatchWithVerboseOutput()
	{
		CreateTestSchemaAndData(Client);

		var getAllResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getAllResponse.HttpStatusCode);
		Assert.NotNull(getAllResponse.Result);
		var allWeaviateObjects = getAllResponse.Result.Length;

		var batch = Client.Batch.DeleteObjects(new()
		{
			Where = new()
			{
				Operator = Operator.LessThan,
				Path = new[] { "_creationTimeUnix" },
				ValueString = DateTimeOffset.Now.AddSeconds(60).ToUnixTimeMilliseconds().ToString()
			},
			Collection = COLLECTION_NAME_PIZZA,
			Output = BatchOutput.Verbose,
			ConsistencyLevel = ConsistencyLevel.Quorum
		});

		Assert.Equal(HttpStatusCode.OK, batch.HttpStatusCode);

		var getRemainingResponse = Client.Data.GetAll();
		Assert.Equal(HttpStatusCode.OK, getRemainingResponse.HttpStatusCode);
		Assert.NotNull(getRemainingResponse.Result);
		var remainingWeaviateObjects = getRemainingResponse.Result.Length;

		Assert.Equal(allWeaviateObjects - 4, remainingWeaviateObjects);

		Assert.NotNull(batch.Result);
		Assert.Equal(BatchOutput.Verbose, batch.Result.Output);
		Assert.NotNull(batch.Result.Results);
		Assert.Equal(4, batch.Result.Results.Successful);
		Assert.Equal(0, batch.Result.Results.Failed);
		Assert.Equal(10000L, batch.Result.Results.Limit);
		Assert.Equal(4, batch.Result.Results.Matches);
		Assert.NotNull(batch.Result.Results.Objects);
		Assert.Equal(4, batch.Result.Results.Objects.Length);

		var pizzas = batch.Result.Results.Objects.Select(o => o.Id).ToArray();
		Assert.Contains(PIZZA_HAWAII_ID, pizzas);
		Assert.Contains(PIZZA_DOENER_ID, pizzas);
		Assert.Contains(PIZZA_QUATTRO_FORMAGGI_ID, pizzas);
		Assert.Contains(PIZZA_FRUTTI_DI_MARE_ID, pizzas);
	}
}
