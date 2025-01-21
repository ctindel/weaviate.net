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


using System.Linq;
using System.Net;
using Xunit;

namespace Weaviate.Client.IntegrationTests.Api.Cluster;

[Collection("Sequential")]
public class ClusterTests : TestBase
{
	[Fact]
	public void NodeStatusWithoutData()
	{
		Client.Collections.DeleteAllCollections();
		var nodeStatus = Client.Cluster.NodeStatus();
		Assert.Equal(HttpStatusCode.OK, nodeStatus.HttpStatusCode);
		Assert.NotNull(nodeStatus.Result);

		var result = nodeStatus.Result;
		Assert.NotNull(result);
		Assert.NotNull(result.Version);
		Assert.NotNull(result.Status);
		Assert.Equal(ExpectedVersion, result.Version ?? string.Empty);
		Assert.Equal("HEALTHY", result.Status ?? string.Empty);

		// Stats and Shards may be null in newer API versions
		if (result.Shards != null)
		{
			var objectCount = result.Shards
				.Where(s => s != null)
				.Sum(s => s?.ObjectCount ?? 0);
			Assert.Equal(0, objectCount);
		}
	}

	[Fact]
	public void NodeStatusWithData()
	{
		CreateTestCollectionsAndData(Client);
		var nodeStatus = Client.Cluster.NodeStatus();
		Assert.Equal(HttpStatusCode.OK, nodeStatus.HttpStatusCode);
		Assert.NotNull(nodeStatus.Result);
		Assert.NotNull(nodeStatus.Result.Version);
		Assert.NotNull(nodeStatus.Result.Status);
		Assert.Equal(ExpectedVersion, nodeStatus.Result.Version ?? string.Empty);
		Assert.Equal("HEALTHY", nodeStatus.Result.Status ?? string.Empty);
		// Stats and Shards may be null in newer API versions
		if (nodeStatus.Result.Shards != null)
		{
			var shards = nodeStatus.Result.Shards
				.Where(s => s != null && s.Collection != null)
				.ToList();
			var collections = shards.Select(s => s.Collection ?? string.Empty);
			Assert.Contains(COLLECTION_NAME_PIZZA, collections);
			Assert.Contains(COLLECTION_NAME_SOUP, collections);
			Assert.Equal(4, shards.Where(s => (s.Collection ?? string.Empty).Equals(COLLECTION_NAME_PIZZA)).Sum(s => s?.ObjectCount ?? 0));
			Assert.Equal(2, shards.Where(s => (s.Collection ?? string.Empty).Equals(COLLECTION_NAME_SOUP)).Sum(s => s?.ObjectCount ?? 0));
		}
	}
}
