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

using Xunit;

namespace Weaviate.Client.IntegrationTests.Api.Graph;

[Collection("Sequential")]
public class ExploreTests : TestBase
{
	[Fact]
	public void ExploreWithCertainty()
	{
		CreateTestCollectionsAndData(Client);

		var result = Client.Graph.Explore(new()
		{
			NearText = new()
			{
				Concepts = new[] { "pineapple slices", "ham" },
				MoveTo = new() { Concepts = new[] { COLLECTION_NAME_PIZZA }, Force = 0.3f },
				MoveAwayFrom = new() { Concepts = new[] { "toast", "bread" }, Force = 0.4f },
				Certainty = 0.4f
			},
			Fields = new[] { ExploreFields.Certainty, ExploreFields.Beacon, ExploreFields.ClassName }
		});

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var exploreData = result.Result.Data["Explore"];
		Assert.NotNull(exploreData);
		Assert.Equal(6, exploreData.AsArray().Count);
	}

	[Fact]
	public void ExploreWithDistance()
	{
		CreateTestCollectionsAndData(Client);

		var result = Client.Graph.Explore(new()
		{
			NearText = new()
			{
				Concepts = new[] { "pineapple slices", "ham" },
				MoveTo = new() { Concepts = new[] { COLLECTION_NAME_PIZZA }, Force = 0.3f },
				MoveAwayFrom = new() { Concepts = new[] { "toast", "bread" }, Force = 0.4f },
				Distance = 0.8f
			},
			Fields = new[] { ExploreFields.Certainty, ExploreFields.Beacon, ExploreFields.ClassName }
		});

		Assert.NotNull(result.Result);
		Assert.NotNull(result.Result.Data);
		var exploreData = result.Result.Data["Explore"];
		Assert.NotNull(exploreData);
		Assert.Equal(6, exploreData.AsArray().Count);
	}
}
