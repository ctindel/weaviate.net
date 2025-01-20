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
public class CheckTests : TestBase
{
	[Fact]
	public void Check()
	{
		CreateTestSchemaAndData(Client);

		var nonExistent = Client.Data.Check(new("11111111-1111-1111-aaaa-aaaaaaaaaaaa", COLLECTION_NAME_PIZZA));
		Assert.Equal(HttpStatusCode.NotFound, nonExistent.HttpStatusCode);

		var pizza = Client.Data.Check(new(PIZZA_HAWAII_ID, COLLECTION_NAME_PIZZA));
		Assert.Equal(HttpStatusCode.NoContent, pizza.HttpStatusCode);

		var soup = Client.Data.Check(new(SOUP_CHICKENSOUP_ID, COLLECTION_NAME_SOUP));
		Assert.Equal(HttpStatusCode.NoContent, soup.HttpStatusCode);

		Client.Schema.DeleteAllCollections();

		var pizzaNone = Client.Data.Check(new(PIZZA_HAWAII_ID, COLLECTION_NAME_PIZZA));
		Assert.Equal(HttpStatusCode.NotFound, pizzaNone.HttpStatusCode);

		var soupNone = Client.Data.Check(new(SOUP_CHICKENSOUP_ID, COLLECTION_NAME_SOUP));
		Assert.Equal(HttpStatusCode.NotFound, soupNone.HttpStatusCode);
	}
}
