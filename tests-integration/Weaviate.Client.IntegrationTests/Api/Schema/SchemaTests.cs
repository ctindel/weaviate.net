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

namespace Weaviate.Client.IntegrationTests.Api.Schema;

[Collection("Sequential")]
public class SchemaTests : TestBase
{
	[Fact]
	public void CreateBandCollection()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
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

		var schema = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schema.HttpStatusCode);
		Assert.NotNull(schema.Result);
		Assert.NotNull(schema.Result.Collections);
		Assert.Single(schema.Result.Collections);

		var deleteStatus = Client.Schema.DeleteCollection(new DeleteCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, deleteStatus.HttpStatusCode);
	}

	[Fact]
	public void CreateRunCollection()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Run")
		{
			Description = "Running from the fuzz",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString()
		});
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);

		var schema = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schema.HttpStatusCode);
		Assert.NotNull(schema.Result);
		Assert.NotNull(schema.Result.Collections);
		Assert.Single(schema.Result.Collections);

		Client.Schema.DeleteAllCollections();

		var schemaAfterDelete = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schemaAfterDelete.HttpStatusCode);
		Assert.NotNull(schemaAfterDelete.Result);
		Assert.NotNull(schemaAfterDelete.Result.Collections);
		Assert.Empty(schemaAfterDelete.Result.Collections);
	}

	[Fact]
	public void DeleteClasses()
	{
		CreateTestSchemaAndData(Client);

		var schemaAfterCreate = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);
		Assert.NotNull(schemaAfterCreate.Result.Collections);
		Assert.Equal(2, schemaAfterCreate.Result.Collections.Count);

		var deletePizzas = Client.Schema.DeleteCollection(new DeleteCollectionRequest(COLLECTION_NAME_PIZZA));
		Assert.Equal(HttpStatusCode.OK, deletePizzas.HttpStatusCode);

		var deleteSoups = Client.Schema.DeleteCollection(new DeleteCollectionRequest(COLLECTION_NAME_SOUP));
		Assert.Equal(HttpStatusCode.OK, deleteSoups.HttpStatusCode);

		var schemaAfterDelete = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schemaAfterDelete.HttpStatusCode);
		Assert.NotNull(schemaAfterDelete.Result);
		Assert.NotNull(schemaAfterDelete.Result.Collections);
		Assert.Empty(schemaAfterDelete.Result.Collections);
	}

	[Fact]
	public void DeleteAllClasses()
	{
		CreateTestSchemaAndData(Client);

		var schemaAfterCreate = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);
		Assert.NotNull(schemaAfterCreate.Result.Collections);
		Assert.Equal(2, schemaAfterCreate.Result.Collections.Count);

		Client.Schema.DeleteAllCollections();

		var schemaAfterDelete = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schemaAfterDelete.HttpStatusCode);
		Assert.NotNull(schemaAfterDelete.Result);
		Assert.NotNull(schemaAfterDelete.Result.Collections);
		Assert.Empty(schemaAfterDelete.Result.Collections);
	}

	[Fact]
	public void CreateCollectionsAddProperties()
	{
		CreateWeaviateTestSchemaFood(Client);

		var property = new Property
		{
			Name = "Additional", Description = "Additional property", DataType = new[] { DataType.String }
		};

		var pizzaProperty = Client.Schema.CreateProperty(new(COLLECTION_NAME_PIZZA) { Property = property });
		Assert.Equal(HttpStatusCode.OK, pizzaProperty.HttpStatusCode);
		Assert.NotNull(pizzaProperty.Result);

		var soupProperty = Client.Schema.CreateProperty(new(COLLECTION_NAME_SOUP) { Property = property });
		Assert.Equal(HttpStatusCode.OK, soupProperty.HttpStatusCode);
		Assert.NotNull(soupProperty.Result);

		var schemaAfterCreate = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);
		Assert.NotNull(schemaAfterCreate.Result.Collections);
		foreach (var collection in schemaAfterCreate.Result.Collections)
		{
			Assert.NotNull(collection);
			Assert.NotNull(collection.Properties);
			Assert.Equal(5, collection.Properties.Length);
			Assert.Equal(Tokenization.Word, collection.Properties.Last().Tokenization);
		}
	}

	[Fact]
	public void CreateClassExplicitVectorizerWithProperties()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Article")
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);

		var schema = Client.Schema.GetSchema();
		Assert.Equal(HttpStatusCode.OK, schema.HttpStatusCode);
		Assert.NotNull(schema.Result);
		Assert.NotNull(schema.Result.Collections);
		Assert.Single(schema.Result.Collections);

		Assert.NotNull(schema.Result);
		Assert.NotNull(schema.Result.Collections);
		var collection = schema.Result.Collections.First();
		Assert.NotNull(collection);
		Assert.NotNull(collection.Properties);

		var titleProperty = collection.Properties.Single(p => p.Name == "title");
		Assert.NotNull(titleProperty);
		Assert.Equal(Tokenization.Field, titleProperty.Tokenization);

		var contentProperty = collection.Properties.Single(p => p.Name == "content");
		Assert.NotNull(contentProperty);
		Assert.Equal(Tokenization.Word, contentProperty.Tokenization);

		var deleteStatus = Client.Schema.DeleteCollection(new DeleteCollectionRequest("Article"));
		Assert.Equal(HttpStatusCode.OK, deleteStatus.HttpStatusCode);
	}

	[Fact]
	public void CreateCollectionExplicitVectorizerWithArrayProperties()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("CollectionArrays")
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
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Article")
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);
		Assert.NotNull(createStatus.Result.Properties);
		Assert.Equal(2, createStatus.Result.Properties.Length);
		Assert.Equal(Tokenization.Word, createStatus.Result.Properties[0].Tokenization);
		Assert.Equal(Tokenization.Word, createStatus.Result.Properties[1].Tokenization);
	}

	[Fact]
	public void CreateCollectionWithInvalidTokenizationProperty()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus1 = Client.Schema.CreateCollection(new CreateCollectionRequest("Pizza")
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
		Assert.Equal(HttpStatusCode.UnprocessableEntity, createStatus1.HttpStatusCode);
		Assert.NotNull(createStatus1.Error);
		Assert.NotNull(createStatus1.Error.Error);
		Assert.NotEmpty(createStatus1.Error.Error);
		Assert.Equal("Tokenization 'field' is not allowed for data type 'text'", createStatus1.Error.Error[0].Message);

		var createStatus2 = Client.Schema.CreateCollection(new CreateCollectionRequest("Pizza")
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
		Assert.Equal(HttpStatusCode.UnprocessableEntity, createStatus2.HttpStatusCode);
		Assert.NotNull(createStatus2.Error);
		Assert.NotNull(createStatus2.Error.Error);
		Assert.NotEmpty(createStatus2.Error.Error);
		Assert.Equal("Tokenization 'word' is not allowed for data type 'int'", createStatus2.Error.Error[0].Message);
	}

	[Fact]
	public void CreateCollectionWithBM25Config()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://localhost:11434" }
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);
		Assert.NotNull(createStatus.Result.InvertedIndexConfig);
		Assert.NotNull(createStatus.Result.InvertedIndexConfig.Bm25);
		Assert.NotNull(createStatus.Result.InvertedIndexConfig.Stopwords);

		void Verify(WeaviateCollection collection)
		{
			Assert.NotNull(collection.InvertedIndexConfig);
			Assert.NotNull(collection.InvertedIndexConfig.Bm25);
			Assert.Equal(0.777f, collection.InvertedIndexConfig.Bm25.B);
			Assert.Equal(1.777f, collection.InvertedIndexConfig.Bm25.K1);
			Assert.NotNull(collection.InvertedIndexConfig.Stopwords);
			Assert.Equal("en", collection.InvertedIndexConfig.Stopwords.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords.Removals);
			Assert.Equal(300, collection.InvertedIndexConfig.CleanupIntervalSeconds);
		}

		Verify(createStatus.Result);

		var schemaAfterCreate = Client.Schema.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);

		Verify(schemaAfterCreate.Result);
	}

	[Fact]
	public void CreateCollectionWithStopwordsConfig()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToWeaviateString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://localhost:11434" }
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);

		void Verify(WeaviateCollection collection)
		{
			Assert.NotNull(collection.InvertedIndexConfig);
			Assert.NotNull(collection.InvertedIndexConfig.Stopwords);
			Assert.Equal("en", collection.InvertedIndexConfig.Stopwords.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords.Removals);
		}

		Verify(createStatus.Result);

		var schemaAfterCreate = Client.Schema.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);

		Verify(schemaAfterCreate.Result);
	}

	[Fact]
	public void CreateCollectionWithBM25ConfigAndWithStopwordsConfig()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://localhost:11434" }
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);
		Assert.NotNull(createStatus.Result.InvertedIndexConfig);
		Assert.NotNull(createStatus.Result.InvertedIndexConfig.Bm25);
		Assert.NotNull(createStatus.Result.InvertedIndexConfig.Stopwords);

		void Verify(WeaviateCollection collection)
		{
			Assert.NotNull(collection.InvertedIndexConfig);
			Assert.NotNull(collection.InvertedIndexConfig.Bm25);
			Assert.Equal(0.777f, collection.InvertedIndexConfig.Bm25.B);
			Assert.Equal(1.777f, collection.InvertedIndexConfig.Bm25.K1);
			Assert.NotNull(collection.InvertedIndexConfig.Stopwords);
			Assert.Equal("en", collection.InvertedIndexConfig.Stopwords.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords.Removals);
			Assert.Equal(300, collection.InvertedIndexConfig.CleanupIntervalSeconds);
		}

		Verify(createStatus.Result);

		var schemaAfterCreate = Client.Schema.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);
		Assert.NotNull(schemaAfterCreate.Result.InvertedIndexConfig);
		Assert.NotNull(schemaAfterCreate.Result.InvertedIndexConfig.Bm25);
		Assert.NotNull(schemaAfterCreate.Result.InvertedIndexConfig.Stopwords);

		Verify(schemaAfterCreate.Result);
	}

	[Fact]
	public void GetShards()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
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

		var shards = Client.Schema.GetShards(new GetShardsRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, shards.HttpStatusCode);

		Assert.NotNull(shards.Result);
		Assert.NotEmpty(shards.Result);
		Assert.NotNull(shards.Result[0].Name);
		Assert.Equal(ShardStatus.Ready, shards.Result[0].Status);
	}

	[Fact]
	public void UpdateShard()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
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

		var shards = Client.Schema.GetShards(new GetShardsRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, shards.HttpStatusCode);
		Assert.NotNull(shards.Result);
		Assert.NotEmpty(shards.Result);
		Assert.NotNull(shards.Result[0].Name);
		Assert.Equal(ShardStatus.Ready, shards.Result[0].Status);

		var updateShard = Client.Schema.UpdateShard(new UpdateShardRequest("Band", shards.Result[0].Name ?? throw new InvalidOperationException("Shard name should not be null"), ShardStatus.ReadOnly));
		Assert.Equal(HttpStatusCode.OK, updateShard.HttpStatusCode);
		Assert.Equal(ShardStatus.ReadOnly, updateShard.Result);

		var readyShard = Client.Schema.UpdateShard(new UpdateShardRequest("Band", shards.Result[0].Name ?? throw new InvalidOperationException("Shard name should not be null"), ShardStatus.Ready));
		Assert.Equal(HttpStatusCode.OK, readyShard.HttpStatusCode);
		Assert.Equal(ShardStatus.Ready, readyShard.Result);
	}

	[Fact]
	public void UpdateShards()
	{
		const int shardCount = 3;

		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);

		var shards = Client.Schema.GetShards(new GetShardsRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, shards.HttpStatusCode);
		Assert.NotNull(shards.Result);
		Assert.Equal(3, shards.Result.Length);

		foreach (var shard in shards.Result)
		{
			Assert.NotNull(shard.Name);
			var updateShard = Client.Schema.UpdateShard(new UpdateShardRequest("Band", shard.Name, ShardStatus.ReadOnly));
			Assert.Equal(HttpStatusCode.OK, updateShard.HttpStatusCode);
			Assert.Equal(ShardStatus.ReadOnly, updateShard.Result);
		}

		foreach (var shard in shards.Result)
		{
			Assert.NotNull(shard.Name);
			var updateShard = Client.Schema.UpdateShard(new UpdateShardRequest("Band", shard.Name, ShardStatus.Ready));
			Assert.Equal(HttpStatusCode.OK, updateShard.HttpStatusCode);
			Assert.Equal(ShardStatus.Ready, updateShard.Result);
		}
	}

	[Fact]
	public void CreateCollectionWithExplicitReplicationFactor()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToString(),
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);

		void Verify(WeaviateCollection collection)
		{
			Assert.NotNull(collection.ReplicationConfig);
			Assert.Equal(2, collection.ReplicationConfig.Factor);
		}

		Verify(createStatus.Result);

		var schemaAfterCreate = Client.Schema.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);

		Verify(schemaAfterCreate.Result);
	}

	[Fact]
	public void CreateCollectionWithImplicitReplicationFactor()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new CreateCollectionRequest("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = VectorIndexType.HNSW.ToString(),
			Vectorizer = Vectorizer.Text2VecOllama.ToString(),
			VectorizerConfig = new Dictionary<string, object>
			{
				{ "model", "mxbai-embed-large" },
				{ "api_endpoint", "http://host.docker.internal:11434" }
			},
			ReplicationConfig = new()
		});
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);
		Assert.NotNull(createStatus.Result);

		void Verify(WeaviateCollection collection)
		{
			Assert.NotNull(collection.ReplicationConfig);
			Assert.Equal(1, collection.ReplicationConfig.Factor);
		}

		Verify(createStatus.Result);

		var schemaAfterCreate = Client.Schema.GetCollection(new GetCollectionRequest("Band"));
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);
		Assert.NotNull(schemaAfterCreate.Result);

		Verify(schemaAfterCreate.Result);
	}

	[Fact]
	public void CreateClassWithInvertedIndexConfigAndVectorIndexConfigAndShardConfig()
	{
		Client.Schema.DeleteAllCollections();

		var createStatus = Client.Schema.CreateCollection(new("Band")
		{
			Description = "Band that plays and produces music",
			VectorIndexType = nameof(VectorIndexType.HNSW),
			Vectorizer = nameof(Vectorizer.Text2VecOllama),
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
						Type = EncoderType.Tile.ToString().ToLowerInvariant(),
						Distribution = DistributionType.Normal.ToString().ToLowerInvariant()
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
		Assert.Equal(HttpStatusCode.OK, createStatus.HttpStatusCode);

		void Verify(WeaviateCollection collection)
		{
			Assert.NotNull(collection.InvertedIndexConfig);
			Assert.NotNull(collection.InvertedIndexConfig.Bm25);
			Assert.Equal(0.777f, collection.InvertedIndexConfig.Bm25.B);
			Assert.Equal(1.777f, collection.InvertedIndexConfig.Bm25.K1);
			Assert.NotNull(collection.InvertedIndexConfig.Stopwords);
			Assert.Equal("en", collection.InvertedIndexConfig.Stopwords.Preset);
			Assert.Equal(new[] { "star", "nebula" }, collection.InvertedIndexConfig.Stopwords.Additions);
			Assert.Equal(new[] { "a", "the" }, collection.InvertedIndexConfig.Stopwords.Removals);
			Assert.Equal(300, collection.InvertedIndexConfig.CleanupIntervalSeconds);

			Assert.NotNull(collection.VectorIndexConfig);
			Assert.Equal(300, collection.VectorIndexConfig.CleanupIntervalSeconds);
			Assert.Equal(128, collection.VectorIndexConfig.EfConstruction);
			Assert.Equal(64, collection.VectorIndexConfig.MaxConnections);
			Assert.Equal(500000, collection.VectorIndexConfig.VectorCacheMaxObjects);
			Assert.Equal(-1, collection.VectorIndexConfig.Ef);
			Assert.False(collection.VectorIndexConfig.Skip);
			Assert.Equal(8, collection.VectorIndexConfig.DynamicEfFactor);
			Assert.Equal(100, collection.VectorIndexConfig.DynamicEfMin);
			Assert.Equal(500, collection.VectorIndexConfig.DynamicEfMax);
			Assert.Equal(40000, collection.VectorIndexConfig.FlatSearchCutoff);
			Assert.NotNull(collection.VectorIndexConfig.Distance);
			Assert.Equal(Distance.DotProduct.ToString(), collection.VectorIndexConfig.Distance?.ToString(), StringComparer.InvariantCultureIgnoreCase);

			Assert.NotNull(collection.VectorIndexConfig);
			Assert.NotNull(collection.VectorIndexConfig.ProductQuantization);
			var pq = collection.VectorIndexConfig.ProductQuantization;
			Assert.True(pq.Enabled);
			Assert.True(pq.BitCompression);
			Assert.Equal(4, pq.Segments);
			Assert.Equal(8, pq.Centroids);
			Assert.NotNull(pq.Encoder);
			Assert.NotNull(pq.Encoder.Type);
			Assert.NotNull(pq.Encoder.Distribution);
			Assert.Equal(EncoderType.Tile.ToString(), pq.Encoder.Type, StringComparer.InvariantCultureIgnoreCase);
			Assert.Equal(DistributionType.Normal.ToString(), pq.Encoder.Distribution, StringComparer.InvariantCultureIgnoreCase);

			Assert.NotNull(collection.ShardingConfig);
			var shardingConfig = collection.ShardingConfig;
			Assert.Equal(1, shardingConfig.ActualCount);
			Assert.Equal(128, shardingConfig.ActualVirtualCount);
			Assert.Equal(1, shardingConfig.DesiredCount);
			Assert.Equal(128, shardingConfig.DesiredVirtualCount);
			Assert.NotNull(shardingConfig.Function);
			Assert.Equal("murmur3", shardingConfig.Function);
			Assert.NotNull(shardingConfig.Key);
			Assert.Equal("_id", shardingConfig.Key);
			Assert.NotNull(shardingConfig.Strategy);
			Assert.Equal("hash", shardingConfig.Strategy);
			Assert.Equal(128, shardingConfig.VirtualPerPhysical);
		}

		Verify(createStatus.Result!);

		var schemaAfterCreate = Client.Schema.GetCollection(new("Band"));
		Assert.Equal(HttpStatusCode.OK, schemaAfterCreate.HttpStatusCode);

		Verify(schemaAfterCreate.Result!);
	}
}
