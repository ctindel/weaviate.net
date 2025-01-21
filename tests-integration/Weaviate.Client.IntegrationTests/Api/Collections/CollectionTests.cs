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

namespace Weaviate.Client.IntegrationTests.Api.Collections;

[Collection("Sequential")]
public class CollectionTests : TestBase
{
	[Fact]
	public void CreateBandCollection()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			Properties = new Property[]
			{
				new()
				{
					Name = "name",
					Description = "Name of the band",
					DataType = new[] { DataType.Text },
					IndexFilterable = true,
					IndexSearchable = true
				},
				new()
				{
					Name = "members",
					Description = "Band members",
					DataType = new[] { DataType.Object },
					IndexFilterable = true,
					IndexSearchable = false,
					NestedProperties = new[]
					{
						new Property
						{
							Name = "name",
							Description = "Name of the band member",
							DataType = new[] { DataType.Text },
							IndexFilterable = true,
							IndexSearchable = false
						},
						new Property
						{
							Name = "instrument",
							Description = "Instrument played by the member",
							DataType = new[] { DataType.Text },
							IndexFilterable = true,
							IndexSearchable = false,
							VectorizerConfig = new PropertyVectorizerConfig
							{
								SkipVectorization = true,
								Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString()
							},
							VectorIndexConfig = new PropertyVectorIndexConfig
							{
								Distance = VectorDistance.Cosine.ToString(),
								MaxConnections = 64,
								Skip = false
							}
						}
					}
				}
			}
		});
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);

		var collections = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collections.HttpStatusCode);
		Assert.NotNull(collections.Result);
		Assert.NotNull(collections.Result.Collections);
		Assert.Single(collections.Result.Collections);

		// Verify property configuration
		var bandCollection = collections.Result.Collections.First();
		Assert.NotNull(bandCollection);
		Assert.NotNull(bandCollection.Properties);
		Assert.Equal(2, bandCollection.Properties.Length);

		// Verify name property
		var nameProperty = bandCollection.Properties.First(p => p.Name == "name");
		Assert.NotNull(nameProperty);
		Assert.Equal("Name of the band", nameProperty.Description);
		Assert.True(nameProperty.IndexFilterable);
		Assert.True(nameProperty.IndexSearchable);

		// Verify members property and its nested properties
		var membersProperty = bandCollection.Properties.First(p => p.Name == "members");
		Assert.NotNull(membersProperty);
		Assert.Equal("Band members", membersProperty.Description);
		Assert.NotNull(membersProperty.NestedProperties);
		Assert.Equal(2, membersProperty.NestedProperties.Length);

		// Verify instrument property configuration
		var instrumentProperty = membersProperty.NestedProperties.First(p => p.Name == "instrument");
		Assert.NotNull(instrumentProperty);
		Assert.Equal("Instrument played by the member", instrumentProperty.Description);
		Assert.NotNull(instrumentProperty.VectorizerConfig);
		Assert.Equal(Vectorizer.Text2VecOllama.ToString(), instrumentProperty.VectorizerConfig.Vectorizer);
		Assert.True(instrumentProperty.VectorizerConfig.SkipVectorization);

		Assert.NotNull(instrumentProperty.VectorIndexConfig);
		Assert.Equal(VectorDistance.Cosine.ToString(), instrumentProperty.VectorIndexConfig.Distance);
		Assert.Equal(64, instrumentProperty.VectorIndexConfig.MaxConnections);
		Assert.False(instrumentProperty.VectorIndexConfig.Skip);

		// Verify member name property
		var memberNameProperty = membersProperty.NestedProperties.First(p => p.Name == "name");
		Assert.NotNull(memberNameProperty);
		Assert.Equal("Name of the band member", memberNameProperty.Description);

		var deleteStatus = Client.Collections.DeleteCollection(new DeleteCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, deleteStatus.HttpStatusCode);
	}

	[Fact]
	public void CreateRunCollection()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Run")
		{
			Description = "Running from the fuzz",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString()
		});
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);

		var collections = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collections.HttpStatusCode);
		Assert.NotNull(collections.Result);
		Assert.NotNull(collections.Result.Collections);
		Assert.Single(collections.Result.Collections);

		Client.Collections.DeleteAllCollections();

		var collectionsAfterDelete = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collectionsAfterDelete.HttpStatusCode);
		Assert.NotNull(collectionsAfterDelete.Result);
		Assert.NotNull(collectionsAfterDelete.Result.Collections);
		Assert.Empty(collectionsAfterDelete.Result.Collections);
	}

	[Fact]
	public void DeleteCollections()
	{
		CreateTestCollectionsAndData(Client);

		var collectionsAfterCreate = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collectionsAfterCreate.HttpStatusCode);
		Assert.NotNull(collectionsAfterCreate.Result);
		Assert.NotNull(collectionsAfterCreate.Result.Collections);
		Assert.Equal(2, collectionsAfterCreate.Result.Collections.Count);

		var deletePizzas = Client.Collections.DeleteCollection(new DeleteCollectionRequest("Pizza"));
		Assert.Equal(HttpStatusCode.OK, deletePizzas.HttpStatusCode);

		var deleteSoups = Client.Collections.DeleteCollection(new DeleteCollectionRequest("Soup"));
		Assert.Equal(HttpStatusCode.OK, deleteSoups.HttpStatusCode);

		var collectionsAfterDelete = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collectionsAfterDelete.HttpStatusCode);
		Assert.NotNull(collectionsAfterDelete.Result);
		Assert.NotNull(collectionsAfterDelete.Result.Collections);
		Assert.Empty(collectionsAfterDelete.Result.Collections);
	}

	[Fact]
	public void DeleteAllCollections()
	{
		CreateTestCollectionsAndData(Client);

		var collectionsAfterCreate = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collectionsAfterCreate.HttpStatusCode);
		Assert.NotNull(collectionsAfterCreate.Result);
		Assert.NotNull(collectionsAfterCreate.Result.Collections);
		Assert.Equal(2, collectionsAfterCreate.Result.Collections.Count);

		Client.Collections.DeleteAllCollections();

		var collectionsAfterDelete = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collectionsAfterDelete.HttpStatusCode);
		Assert.NotNull(collectionsAfterDelete.Result);
		Assert.NotNull(collectionsAfterDelete.Result.Collections);
		Assert.Empty(collectionsAfterDelete.Result.Collections);
	}

	[Fact]
	public void CreateCollectionsAddProperties()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var property = new Property
		{
			Name = "Additional", Description = "Additional property", DataType = new[] { DataType.String }
		};

		var pizzaProperty = Client.Collections.CreateProperty(new("Pizza") { Property = property });
		Assert.Equal(HttpStatusCode.OK, pizzaProperty.HttpStatusCode);

		var soupProperty = Client.Collections.CreateProperty(new("Soup") { Property = property });
		Assert.Equal(HttpStatusCode.OK, soupProperty.HttpStatusCode);

		var collectionsAfterCreate = Client.Collections.GetCollections();
		Assert.Equal(HttpStatusCode.OK, collectionsAfterCreate.HttpStatusCode);
		Assert.NotNull(collectionsAfterCreate.Result);
		Assert.NotNull(collectionsAfterCreate.Result.Collections);
		foreach (var collection in collectionsAfterCreate.Result.Collections)
		{
			Assert.NotNull(collection);
			Assert.NotNull(collection.Properties);
			Assert.Equal(5, collection.Properties.Length);
			var lastProperty = collection.Properties.Last();
			Assert.NotNull(lastProperty);
			Assert.Equal(Tokenization.Word, lastProperty.Tokenization);
		}
	}

	[Fact]
	public void CreateCollectionExplicitVectorizerWithProperties()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Article")
		{
			Description = "A written text, for example a news article or blog post",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			Properties = new Property[]
			{
				new()
				{
					Name = "title",
					Description = "Title of the article",
					Tokenization = Tokenization.Field,
					DataType = new[] { DataType.String }
				},
				new()
				{
					Name = "content",
					Description = "The content of the article",
					Tokenization = Tokenization.Word,
					DataType = new[] { DataType.Text }
				}
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		var collections = Client.Collections.GetCollections();
		Assert.Equal(System.Net.HttpStatusCode.OK, collections.HttpStatusCode);
		Assert.NotNull(collections.Result);
		Assert.NotNull(collections.Result.Collections);
		Assert.Single(collections.Result.Collections);

		var collection = collections.Result.Collections.First();
		Assert.NotNull(collection);
		Assert.NotNull(collection.Properties);

		var titleProperty = collection.Properties.Single(p => p.Name == "title");
		Assert.NotNull(titleProperty);
		Assert.Equal(Tokenization.Field, titleProperty.Tokenization);

		var contentProperty = collection.Properties.Single(p => p.Name == "content");
		Assert.NotNull(contentProperty);
		Assert.Equal(Tokenization.Word, contentProperty.Tokenization);

		var deleteStatus = Client.Collections.DeleteCollection(new DeleteCollectionRequest("Article"));
		Assert.Equal(System.Net.HttpStatusCode.OK, deleteStatus.HttpStatusCode);
	}

	[Fact]
	public void CreateCollectionExplicitVectorizerWithArrayProperties()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("CollectionArrays")
		{
			Description = "Collection which properties are all array properties",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			Properties = new Property[]
			{
				new()
				{
					Name = "stringArray",
					Tokenization = Tokenization.Field,
					DataType = new[] { DataType.StringArray }
				},
				new()
				{
					Name = "textArray",
					Tokenization = Tokenization.Word,
					DataType = new[] { DataType.TextArray }
				},
				new() { Name = "intArray", DataType = new[] { DataType.IntArray } },
				new() { Name = "numberArray", DataType = new[] { DataType.NumberArray } },
				new() { Name = "booleanArray", DataType = new[] { DataType.BooleanArray } },
				new() { Name = "dateArray", DataType = new[] { DataType.DateArray } }
			}
		});
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);
		Assert.NotNull(createStatus.Result.Properties);
		Assert.Equal(6, createStatus.Result.Properties.Length);
	}

	[Fact]
	public void CreateCollectionWithProperties()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Article")
		{
			Description = "A written text, for example a news article or blog post",
			Properties = new Property[]
			{
				new()
				{
					Name = "title",
					Description = "Title of the article",
					DataType = new[] { DataType.String }
				},
				new()
				{
					Name = "content",
					Description = "The content of the article",
					DataType = new[] { DataType.Text }
				}
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		Assert.Equal(Tokenization.Word, createStatus.Result!.Properties![0].Tokenization);
		Assert.Equal(Tokenization.Word, createStatus.Result!.Properties![1].Tokenization);
	}

	[Fact]
	public void CreateCollectionWithInvalidTokenizationProperty()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus1 = Client.Collections.CreateCollection(new CreateCollectionRequest("Pizza")
		{
			Description = "A delicious religion like food and arguably the best export of Italy.",
			Properties = new Property[]
			{
				new()
				{
					Name = "someText",
					Description = "someText",
					DataType = new[] { DataType.Text },
					Tokenization = Tokenization.Field
				}
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, createStatus1.HttpStatusCode);
		Assert.Equal("Tokenization 'field' is not allowed for data type 'text'", createStatus1.Error!.Error![0].Message!);

		var createStatus2 = Client.Collections.CreateCollection(new CreateCollectionRequest("Pizza")
		{
			Description = "A delicious religion like food and arguably the best export of Italy.",
			Properties = new Property[]
			{
				new()
				{
					Name = "someInt",
					Description = "someInt",
					DataType = new[] { DataType.Int },
					Tokenization = Tokenization.Word
				}
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, createStatus2.HttpStatusCode);
		Assert.Equal("Tokenization 'word' is not allowed for data type 'int'", createStatus2.Error!.Error![0].Message!);
	}

	[Fact]
	public void CreateCollectionWithBM25Config()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			InvertedIndexConfig = new()
			{
				Bm25 = new() { B = 0.777f, K1 = 1.777f },
				Stopwords = new()
				{
					Preset = "en",
					Additions = new[] { "star", "nebula" },
					Removals = new[] { "a", "the" }
				},
				CleanupIntervalSeconds = 300
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		void Verify(WeaviateCollection collection)
		{
			Assert.Equal(0.777f, collection.InvertedIndexConfig!.Bm25!.B);
			Assert.Equal(1.777f, collection.InvertedIndexConfig.Bm25.K1);
			Assert.Equal("en", collection.InvertedIndexConfig.Stopwords!.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords!.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords!.Removals);
			Assert.Equal(300, collection.InvertedIndexConfig.CleanupIntervalSeconds);
		}

		Verify(createStatus.Result!);

		var collectionAfterCreate = Client.Collections.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(System.Net.HttpStatusCode.OK, collectionAfterCreate.HttpStatusCode);

		Verify(collectionAfterCreate.Result!);
	}

	[Fact]
	public void CreateCollectionWithStopwordsConfig()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			InvertedIndexConfig =  new()
				{
					Stopwords = new()
					{
						Preset = "en",
						Additions = new[] { "star", "nebula" },
						Removals = new[] { "a", "the" }
					}
				}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		void Verify(WeaviateCollection collection)
		{
			Assert.Equal("en", collection.InvertedIndexConfig!.Stopwords!.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords!.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords!.Removals);
		}

		Verify(createStatus.Result!);

		var collectionAfterCreate = Client.Collections.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(System.Net.HttpStatusCode.OK, collectionAfterCreate.HttpStatusCode);

		Verify(collectionAfterCreate.Result!);
	}

	[Fact]
	public void CreateCollectionWithBM25ConfigAndWithStopwordsConfig()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			InvertedIndexConfig = new()
			{
				Bm25 = new() { B = 0.777f, K1 = 1.777f },
				Stopwords = new()
				{
					Preset = "en",
					Additions = new[] { "star", "nebula" },
					Removals = new[] { "a", "the" }
				},
				CleanupIntervalSeconds = 300
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		void Verify(WeaviateCollection collection)
		{
			Assert.Equal(0.777f, collection.InvertedIndexConfig!.Bm25!.B);
			Assert.Equal(1.777f, collection.InvertedIndexConfig.Bm25.K1);
			Assert.Equal("en", collection.InvertedIndexConfig.Stopwords!.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords!.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords!.Removals);
			Assert.Equal(300, collection.InvertedIndexConfig.CleanupIntervalSeconds);
		}

		Verify(createStatus.Result!);

		var collectionAfterCreate = Client.Collections.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, collectionAfterCreate.HttpStatusCode);

		Verify(collectionAfterCreate.Result!);
	}

	[Fact]
	public void GetShards()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			}
		});
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);

		var shards = Client.Collections.GetShards(new GetShardsRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, shards.HttpStatusCode);
		Assert.NotNull(shards.Result);
		Assert.NotEmpty(shards.Result);
		Assert.NotNull(shards.Result[0]);
		Assert.NotNull(shards.Result[0].Name);
		Assert.Equal(ShardStatus.Ready, shards.Result[0].Status);
	}

	[Fact]
	public void UpdateShard()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			}
		});
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);

		var shards = Client.Collections.GetShards(new GetShardsRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, shards.HttpStatusCode);
		Assert.NotNull(shards.Result);
		Assert.NotEmpty(shards.Result);
		Assert.NotNull(shards.Result[0]);
		Assert.NotNull(shards.Result[0].Name);
		Assert.Equal(ShardStatus.Ready, shards.Result[0].Status);
		Assert.NotNull(shards.Result[0]);
		Assert.NotNull(shards.Result[0].Name);
		var updateShard = Client.Collections.UpdateShard(new UpdateShardRequest("Band", shards.Result[0].Name ?? throw new InvalidOperationException("Shard name should not be null"), ShardStatus.ReadOnly));
		Assert.Equal(HttpStatusCode.OK, updateShard.HttpStatusCode);
		Assert.Equal(ShardStatus.ReadOnly, updateShard.Result);

		var readyShard = Client.Collections.UpdateShard(new UpdateShardRequest("Band", shards.Result[0].Name ?? throw new InvalidOperationException("Shard name should not be null"), ShardStatus.Ready));
		Assert.Equal(HttpStatusCode.OK, readyShard.HttpStatusCode);
		Assert.Equal(ShardStatus.Ready, readyShard.Result);
	}

	[Fact]
	public void UpdateShards()
	{
		const int shardCount = 3;

		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			ShardingConfig = new()
			{
				ActualCount = shardCount,
				ActualVirtualCount = 128,
				DesiredCount = shardCount,
				DesiredVirtualCount = 128,
				Function = "murmur3",
				Key = "_id",
				Strategy = "hash",
				VirtualPerPhysical = 128
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		var shards = Client.Collections.GetShards(new("Band"));
		Assert.Equal(System.Net.HttpStatusCode.OK, shards.HttpStatusCode);
		Assert.NotNull(shards.Result);
		Assert.Equal(3, shards.Result.Length);

		foreach (var shard in shards.Result)
		{
			Assert.NotNull(shard);
			Assert.NotNull(shard.Name);
			var updateShard = Client.Collections.UpdateShard(new("Band", shard.Name ?? throw new InvalidOperationException("Shard name should not be null"), ShardStatus.ReadOnly));
			Assert.Equal(System.Net.HttpStatusCode.OK, updateShard.HttpStatusCode);
			Assert.Equal(ShardStatus.ReadOnly, updateShard.Result);
		}

		foreach (var shard in shards.Result)
		{
			var shardName = shard.Name ?? throw new InvalidOperationException("Shard name should not be null");
			var updateShard = Client.Collections.UpdateShard(new("Band", shardName, ShardStatus.Ready));
			Assert.Equal(System.Net.HttpStatusCode.OK, updateShard.HttpStatusCode);
			Assert.Equal(ShardStatus.Ready, updateShard.Result);
		}
	}

	[Fact]
	public void CreateCollectionWithExplicitReplicationFactor()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			ReplicationConfig = new()
			{
				Factor = 2
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		void Verify(WeaviateCollection collection)
		{
			Assert.Equal(2, collection.ReplicationConfig!.Factor);
		}

		Verify(createStatus.Result!);

		var collectionAfterCreate = Client.Collections.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(System.Net.HttpStatusCode.OK, collectionAfterCreate.HttpStatusCode);

		Verify(collectionAfterCreate.Result!);
	}

	[Fact]
	public void CreateCollectionWithImplicitReplicationFactor()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			ReplicationConfig = new()
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		void Verify(WeaviateCollection collection)
		{
			Assert.Equal(1, collection.ReplicationConfig!.Factor);
		}

		Verify(createStatus.Result!);

		var collectionAfterCreate = Client.Collections.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(System.Net.HttpStatusCode.OK, collectionAfterCreate.HttpStatusCode);

		Verify(collectionAfterCreate.Result!);
	}

	[Fact]
	public void CreateCollectionWithInvertedIndexConfigAndVectorIndexConfigAndShardConfig()
	{
		Client.Collections.DeleteAllCollections();

		var createStatus = Client.Collections.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			InvertedIndexConfig =
				new()
				{
					Bm25 = new() { B = 0.777f, K1 = 1.777f },
					Stopwords = new()
					{
						Preset = "en",
						Additions = new[] { "star", "nebula" },
						Removals = new[] { "a", "the" }
					},
					CleanupIntervalSeconds = 300
				},
			VectorIndexConfig = new()
			{
				CleanupIntervalSeconds = 300,
				EfConstruction = 128,
				MaxConnections = 64,
				VectorCacheMaxObjects = 500000,
				Ef = -1,
				Skip = false,
				DynamicEfFactor = 8,
				DynamicEfMin = 100,
				DynamicEfMax = 500,
				FlatSearchCutoff = 40000,
				Distance = Distance.DotProduct,
				ProductQuantization = new()
				{
					Enabled = true,
					BitCompression = true,
					Segments = 4,
					Centroids = 8,
					Encoder = new()
					{
						Type = EncoderType.Tile.ToString(),
						Distribution = DistributionType.Normal.ToString()
					}
				}
			},
			ShardingConfig = new()
			{
				ActualCount = 1,
				ActualVirtualCount = 128,
				DesiredCount = 1,
				DesiredVirtualCount = 128,
				Function = "murmur3",
				Key = "_id",
				Strategy = "hash",
				VirtualPerPhysical = 128
			}
		});
		Assert.Equal(System.Net.HttpStatusCode.OK, createStatus.HttpStatusCode);

		void Verify(WeaviateCollection collection)
		{
			Assert.Equal(0.777f, collection.InvertedIndexConfig!.Bm25!.B);
			Assert.Equal(1.777f, collection.InvertedIndexConfig.Bm25.K1);
			Assert.Equal("en", collection.InvertedIndexConfig.Stopwords!.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords!.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords!.Removals);
			Assert.Equal(300, collection.InvertedIndexConfig.CleanupIntervalSeconds);

			Assert.Equal(300, collection.VectorIndexConfig!.CleanupIntervalSeconds);
			Assert.Equal(128, collection.VectorIndexConfig.EfConstruction);
			Assert.Equal(64, collection.VectorIndexConfig.MaxConnections);
			Assert.Equal(500000, collection.VectorIndexConfig.VectorCacheMaxObjects);
			Assert.Equal(-1, collection.VectorIndexConfig.Ef);
			Assert.False(collection.VectorIndexConfig.Skip);
			Assert.Equal(8, collection.VectorIndexConfig.DynamicEfFactor);
			Assert.Equal(100, collection.VectorIndexConfig.DynamicEfMin);
			Assert.Equal(500, collection.VectorIndexConfig.DynamicEfMax);
			Assert.Equal(40000, collection.VectorIndexConfig.FlatSearchCutoff);
			Assert.Equal(Distance.DotProduct, collection.VectorIndexConfig.Distance);

			Assert.True(collection.VectorIndexConfig.ProductQuantization!.Enabled);
			Assert.True(collection.VectorIndexConfig.ProductQuantization!.BitCompression);
			Assert.Equal(4, collection.VectorIndexConfig.ProductQuantization!.Segments);
			Assert.Equal(8, collection.VectorIndexConfig.ProductQuantization!.Centroids);
			Assert.NotNull(collection.VectorIndexConfig);
			Assert.NotNull(collection.VectorIndexConfig.ProductQuantization);
			Assert.NotNull(collection.VectorIndexConfig.ProductQuantization.Encoder);
			Assert.Equal(EncoderType.Tile.ToString(), collection.VectorIndexConfig.ProductQuantization.Encoder.Type);
			Assert.Equal(DistributionType.Normal.ToString(), collection.VectorIndexConfig.ProductQuantization.Encoder.Distribution);

			Assert.Equal(1, collection.ShardingConfig!.ActualCount);
			Assert.Equal(128, collection.ShardingConfig.ActualVirtualCount);
			Assert.Equal(1, collection.ShardingConfig.DesiredCount);
			Assert.Equal(128, collection.ShardingConfig.DesiredVirtualCount);
			Assert.Equal("murmur3", collection.ShardingConfig.Function);
			Assert.Equal("_id", collection.ShardingConfig.Key);
			Assert.Equal("hash", collection.ShardingConfig.Strategy);
			Assert.Equal(128, collection.ShardingConfig.VirtualPerPhysical);
		}

		Verify(createStatus.Result!);

		var collectionAfterCreate = Client.Collections.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(System.Net.HttpStatusCode.OK, collectionAfterCreate.HttpStatusCode);

		Verify(collectionAfterCreate.Result!);
	}
}
