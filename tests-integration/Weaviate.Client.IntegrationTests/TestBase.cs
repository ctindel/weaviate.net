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

using System.Globalization;
using System.Net;
using Flurl.Http;
using Xunit;

namespace Weaviate.Client.IntegrationTests;

public abstract class TestBase
{
	protected const string ExpectedVersion = "1.28.2";
	// GitHash verification removed as it's not a critical compatibility check
	[Obsolete("Use COLLECTION_NAME_PIZZA instead. This constant will be removed in v5.")]
	protected static readonly string CLASS_NAME_PIZZA = "Pizza";
	[Obsolete("Use COLLECTION_NAME_SOUP instead. This constant will be removed in v5.")]
	protected static readonly string CLASS_NAME_SOUP = "Soup";

	protected static readonly string COLLECTION_NAME_PIZZA = "Pizza";
	protected static readonly string COLLECTION_NAME_SOUP = "Soup";

	protected static readonly string PIZZA_QUATTRO_FORMAGGI_ID = "10523cdd-15a2-42f4-81fa-267fe92f7cd6";
	protected static readonly string PIZZA_FRUTTI_DI_MARE_ID = "927dd3ac-e012-4093-8007-7799cc7e81e4";
	protected static readonly string PIZZA_HAWAII_ID = "00000000-0000-0000-0000-000000000000";
	protected static readonly string PIZZA_DOENER_ID = "d2b393ff-4b26-48c7-b554-218d970a9e17";
	protected static readonly string SOUP_CHICKENSOUP_ID = "8c156d37-81aa-4ce9-a811-621e2702b825";
	protected static readonly string SOUP_BEAUTIFUL_ID = "27351361-2898-4d1a-aad7-1ca48253eb0b";
	protected readonly WeaviateClient Client = new(new("http", "host.docker.internal:8080") { DebugLoggingEnabled = true }, new FlurlClient());

	static TestBase()
	{
		// Enable debug logging for all tests
		Environment.SetEnvironmentVariable("WEAVIATE_CLIENT_DEBUG", "true");

		// Set environment variables for Docker networking
		Environment.SetEnvironmentVariable("DOCKER_HOST", "tcp://host.docker.internal:2375");
		Environment.SetEnvironmentVariable("DOCKER_CERT_PATH", "");
		Environment.SetEnvironmentVariable("DOCKER_TLS_VERIFY", "0");
	}

	// Uncomment to try alternate seperators
	// protected TestBase() =>
	// 	Thread.CurrentThread.CurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
	// 	{
	// 		NumberFormat =
	// 		{
	// 			NumberDecimalSeparator = ",",
	// 			NumberGroupSeparator = "."
	// 		}
	// 	};

	protected static void CreateWeaviateTestCollectionsFood(WeaviateClient client)
	{
		client.Collections.DeleteAllCollections();

		var properties = new Property[]
		{
			new()
			{
				Name = "name",
				Description = "name",
				Tokenization = Tokenization.Field,
				IndexFilterable = true,
				IndexSearchable = false,  // Not a text type
				DataType = new[] { DataType.Text }
			},
			new()
			{
				Name = "description",
				Description = "description",
				Tokenization = Tokenization.Word,
				IndexFilterable = true,
				IndexSearchable = true,  // Text type
				DataType = new[] { DataType.Text }
			},
			new()
			{
				Name = "bestBefore",
				Description = "best before",
				IndexFilterable = true,
				IndexSearchable = false,  // Not a text type
				DataType = new[] { DataType.Date }
			},
			new()
			{
				Name = "price",
				Description = "price",
				DataType = new[] { DataType.Number },
				IndexFilterable = true,
				IndexSearchable = false  // Not a text type
			}
		};

		var pizzaCreateStatus = client.Collections.CreateCollection(new CreateCollectionRequest(COLLECTION_NAME_PIZZA)
		{
			Description = "A delicious religion like food and arguably the best export of Italy.",
			Properties = properties,
			VectorIndexType = VectorIndexType.HNSW,
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			ModuleConfig = new Dictionary<string, Dictionary<string, object>>
			{
				["text2vec-ollama"] = new Dictionary<string, object>
				{
					{ "model", "mxbai-embed-large" },
					{ "apiEndpoint", "http://host.docker.internal:11434" },
					{ "skip", false },
					{ "vectorizePropertyName", true },
					{ "vectorizeClassName", false }
				}
			}
		});

		Assert.Equal(HttpStatusCode.OK, pizzaCreateStatus.HttpStatusCode);

		var soupCreateStatus = client.Collections.CreateCollection(new CreateCollectionRequest(COLLECTION_NAME_SOUP)
		{
			Description = "Mostly water based brew of sustenance for humans.",
			Properties = properties,
			VectorIndexType = VectorIndexType.HNSW,
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			ModuleConfig = new Dictionary<string, Dictionary<string, object>>
			{
				["text2vec-ollama"] = new Dictionary<string, object>
				{
					{ "model", "mxbai-embed-large" },
					{ "apiEndpoint", "http://host.docker.internal:11434" },
					{ "skip", false },
					{ "vectorizePropertyName", true },
					{ "vectorizeClassName", false }
				}
			}
		});
		Assert.Equal(HttpStatusCode.OK, soupCreateStatus.HttpStatusCode);
	}

	protected static void CreateWeaviateTestCollectionsFoodWithReferenceProperty(WeaviateClient client)
	{
		CreateWeaviateTestCollectionsFood(client);

		var referenceProperty = new Property
		{
			Name = "otherFoods",
			Description = "reference to other foods",
			IndexFilterable = true,
			IndexSearchable = false,  // Not a text type
			DataType = new[] { COLLECTION_NAME_PIZZA, COLLECTION_NAME_SOUP } // TODO! make it easy to reference collections
		};

		var pizzaRefAdd = client.Collections.CreateProperty(new(COLLECTION_NAME_PIZZA) { Property = referenceProperty });
		Assert.Equal(HttpStatusCode.OK, pizzaRefAdd.HttpStatusCode);

		var soupRefAdd = client.Collections.CreateProperty(new(COLLECTION_NAME_SOUP) { Property = referenceProperty });
		Assert.Equal(HttpStatusCode.OK, soupRefAdd.HttpStatusCode);
	}

	protected static void CreateTestCollectionsAndData(WeaviateClient client)
	{
		CreateWeaviateTestCollectionsFood(client);

		var menuPizza = new[]
		{
			CreateObject(PIZZA_QUATTRO_FORMAGGI_ID, COLLECTION_NAME_PIZZA, "Quattro Formaggi",
				"Pizza quattro formaggi Italian: [ˈkwattro forˈmaddʒi] (four cheese pizza) is a variety of pizza in Italian cuisine that is topped with a combination of four kinds of cheese, usually melted together, with (rossa, red) or without (bianca, white) tomato sauce. It is popular worldwide, including in Italy,[1] and is one of the iconic items from pizzerias's menus.",
				1.4f, "2022-01-02T03:04:05+01:00"),
			CreateObject(PIZZA_FRUTTI_DI_MARE_ID, COLLECTION_NAME_PIZZA, "Frutti di Mare",
				"Frutti di Mare is an Italian type of pizza that may be served with scampi, mussels or squid. It typically lacks cheese, with the seafood being served atop a tomato sauce.",
				2.5f, "2022-02-03T04:05:06+02:00"),
			CreateObject(PIZZA_HAWAII_ID, COLLECTION_NAME_PIZZA, "Hawaii",
				"Universally accepted to be the best pizza ever created.",
				1.1f, "2022-03-04T05:06:07+03:00"),
			CreateObject(PIZZA_DOENER_ID, COLLECTION_NAME_PIZZA, "Doener",
				"A innovation, some say revolution, in the pizza industry.",
				1.2f, "2022-04-05T06:07:08+04:00")
		};
		var menuSoup = new[]
		{
			CreateObject(SOUP_CHICKENSOUP_ID, COLLECTION_NAME_SOUP, "ChickenSoup",
				"Used by humans when their inferior genetics are attacked by microscopic organisms.",
				2.0f, "2022-05-06T07:08:09+05:00"),
			CreateObject(SOUP_BEAUTIFUL_ID, COLLECTION_NAME_SOUP, "Beautiful",
				"Putting the game of letter soups to a whole new level.",
				3f, "2022-06-07T08:09:10+06:00")
		};
		var menu = menuPizza.Concat(menuSoup);

		var response = client.Batch.CreateObjects(new(menu.ToArray()));
		Assert.Equal(HttpStatusCode.OK, response.HttpStatusCode);
		Assert.NotNull(response.Result);
		Assert.Equal(6, response.Result.Successful);
	}

	private static WeaviateObject CreateObject(string id, string className, string name, string description,
		float price, string bestBeforeRfc3339)
	{
		var obj = new WeaviateObject
		{
			Id = id,
			Collection = className,
			Properties = new()
			{
				{ "name", name },
				{ "description", description },
				{ "price", price },
				{ "bestBefore", bestBeforeRfc3339 }
			}
		};

		obj.ModuleConfig = new Dictionary<string, Dictionary<string, object>>
		{
			["text2vec-ollama"] = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "apiEndpoint", "http://host.docker.internal:11434" },
				{ "skip", false },
				{ "vectorizePropertyName", true },
				{ "vectorizeClassName", false }
			}
		};

		return obj;
	}
}
