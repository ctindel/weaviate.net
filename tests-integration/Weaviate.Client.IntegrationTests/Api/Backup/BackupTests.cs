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

namespace Weaviate.Client.IntegrationTests.Api.Backup;

[Collection("Sequential")]
public class BackupTests : TestBase
{
	private const string BackupsDirectory = "/tmp/backups";

	[Fact]
	public void CreateAndRestoreBackupWithWaiting()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.OK, backup.HttpStatusCode);
		Assert.NotNull(backup.Result);
		Assert.NotNull(backup.Result.Collections);
		Assert.Contains(COLLECTION_NAME_PIZZA, backup.Result.Collections ?? Array.Empty<string>());
		Assert.Equal(backupId, backup.Result.Id);
		Assert.StartsWith(BackupsDirectory, backup.Result.Path);
		Assert.Equal(BackupStatus.Success, backup.Result.Status);
		Assert.Equal(Backend.Filesystem, backup.Result.Backend);

		AssertAllPizzasExist();

		var backupStatus = Client.Backup.Status(new(backupId, Backend.Filesystem));
		Assert.Equal(HttpStatusCode.OK, backupStatus.HttpStatusCode);
		Assert.Equal(backupId, backupStatus.Result!.Id);
		Assert.StartsWith(BackupsDirectory, backupStatus.Result.Path);
		Assert.Equal(BackupStatus.Success, backupStatus.Result.Status);
		Assert.Equal(Backend.Filesystem, backupStatus.Result.Backend);

		DeleteAllPizzas();

		var restore = Client.Backup.Restore(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.OK, restore.HttpStatusCode);
		Assert.Equal(backupId, restore.Result!.Id);
		Assert.StartsWith(BackupsDirectory, restore.Result.Path);
		Assert.Equal(BackupStatus.Success, restore.Result.Status);
		Assert.Equal(Backend.Filesystem, restore.Result.Backend);

		AssertAllPizzasExist();

		var restoreStatus = Client.Backup.RestoreStatus(new(backupId, Backend.Filesystem));
		Assert.Equal(HttpStatusCode.OK, restoreStatus.HttpStatusCode);
		Assert.Equal(backupId, restoreStatus.Result!.Id);
		Assert.StartsWith(BackupsDirectory, restoreStatus.Result.Path);
		Assert.Equal(BackupStatus.Success, restoreStatus.Result.Status);
		Assert.Equal(Backend.Filesystem, restoreStatus.Result.Backend);
	}

	[Fact]
	public void CreateAndRestoreBackupWithoutWaiting()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }
		});

		Assert.Equal(HttpStatusCode.OK, backup.HttpStatusCode);
		Assert.Contains(COLLECTION_NAME_PIZZA, backup.Result!.Collections!);
		Assert.Equal(backupId, backup.Result.Id);
		Assert.StartsWith(BackupsDirectory, backup.Result.Path);
		Assert.Equal(BackupStatus.Started, backup.Result.Status);
		Assert.Equal(Backend.Filesystem, backup.Result.Backend);

		while (true)
		{
			var backupStatus = Client.Backup.Status(new(backupId, Backend.Filesystem));
			Assert.Equal(HttpStatusCode.OK, backupStatus.HttpStatusCode);
			Assert.Equal(backupId, backupStatus.Result!.Id);
			Assert.StartsWith(BackupsDirectory, backupStatus.Result.Path);
			Assert.Equal(Backend.Filesystem, backupStatus.Result.Backend);

			if (backupStatus.Result.Status == BackupStatus.Success)
				break;

			Thread.Sleep(100);
		}

		AssertAllPizzasExist();

		DeleteAllPizzas();

		var restore = Client.Backup.Restore(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }
		});

		Assert.Equal(HttpStatusCode.OK, restore.HttpStatusCode);
		Assert.Equal(backupId, restore.Result!.Id);
		Assert.StartsWith(BackupsDirectory, restore.Result.Path);
		Assert.Equal(BackupStatus.Started, restore.Result.Status);
		Assert.Equal(Backend.Filesystem, restore.Result.Backend);

		while (true)
		{
			var restoreStatus = Client.Backup.RestoreStatus(new(backupId, Backend.Filesystem));
			Assert.Equal(HttpStatusCode.OK, restoreStatus.HttpStatusCode);
			Assert.Equal(backupId, restoreStatus.Result!.Id);
			Assert.StartsWith(BackupsDirectory, restoreStatus.Result.Path);
			Assert.Equal(Backend.Filesystem, restoreStatus.Result.Backend);

			if (restoreStatus.Result.Status == BackupStatus.Success)
				break;

			Thread.Sleep(100);
		}

		AssertAllPizzasExist();
	}

	[Fact]
	public void CreateAndRestore1Of2Classes()
	{
		CreateWeaviateTestCollectionsFood(Client);

		AssertAllPizzasExist();
		AssertAllSoupsExist();

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem) { WaitForCompletion = true });

		Assert.Equal(HttpStatusCode.OK, backup.HttpStatusCode);
		Assert.Contains(COLLECTION_NAME_PIZZA, backup.Result!.Collections!);
		Assert.Equal(backupId, backup.Result.Id);
		Assert.StartsWith(BackupsDirectory, backup.Result.Path);
		Assert.Equal(BackupStatus.Success, backup.Result.Status);
		Assert.Equal(Backend.Filesystem, backup.Result.Backend);

		var backupStatus = Client.Backup.Status(new(backupId, Backend.Filesystem));
		Assert.Equal(HttpStatusCode.OK, backupStatus.HttpStatusCode);
		Assert.Equal(backupId, backupStatus.Result!.Id);
		Assert.StartsWith(BackupsDirectory, backupStatus.Result.Path);
		Assert.Equal(BackupStatus.Success, backupStatus.Result.Status);
		Assert.Equal(Backend.Filesystem, backupStatus.Result.Backend);

		DeleteAllPizzas();

		var restore = Client.Backup.Restore(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.OK, restore.HttpStatusCode);
		Assert.Equal(backupId, restore.Result!.Id);
		Assert.StartsWith(BackupsDirectory, restore.Result.Path);
		Assert.Equal(BackupStatus.Success, restore.Result.Status);
		Assert.Equal(Backend.Filesystem, restore.Result.Backend);

		AssertAllPizzasExist();
		AssertAllSoupsExist();

		var restoreStatus = Client.Backup.RestoreStatus(new(backupId, Backend.Filesystem));
		Assert.Equal(HttpStatusCode.OK, restoreStatus.HttpStatusCode);
		Assert.Equal(backupId, restoreStatus.Result!.Id);
		Assert.StartsWith(BackupsDirectory, restoreStatus.Result.Path);
		Assert.Equal(BackupStatus.Success, restoreStatus.Result.Status);
		Assert.Equal(Backend.Filesystem, restoreStatus.Result.Backend);
	}

	[Fact]
	public void FailOnCreateBackupForNotExistingClass()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { "not-exist" }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.UnprocessableEntity, backup.HttpStatusCode);
		Assert.Contains("not-exist", backup.Error!.Error!.First().Message);
	}

	[Fact]
	public void FailOnRestoreBackupForExistingClass()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.OK, backup.HttpStatusCode);
		Assert.Contains(COLLECTION_NAME_PIZZA, backup.Result!.Collections!);
		Assert.Equal(backupId, backup.Result.Id);
		Assert.StartsWith(BackupsDirectory, backup.Result.Path);
		Assert.Equal(BackupStatus.Success, backup.Result.Status);
		Assert.Equal(Backend.Filesystem, backup.Result.Backend);

		var restore = Client.Backup.Restore(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(backupId, restore.Result!.Id);
		Assert.StartsWith(BackupsDirectory, restore.Result.Path);
		Assert.Equal(Backend.Filesystem, restore.Result.Backend);
		Assert.Equal(BackupStatus.Failed, restore.Result.Status);
		Assert.Contains("restore collection Pizza: already exists", restore.Result.Error);
	}

	[Fact]
	public void FailOnCreateOfExistingBackup()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.OK, backup.HttpStatusCode);
		Assert.Contains(COLLECTION_NAME_PIZZA, backup.Result!.Collections!);
		Assert.Equal(backupId, backup.Result.Id);
		Assert.StartsWith(BackupsDirectory, backup.Result.Path);
		Assert.Equal(BackupStatus.Success, backup.Result.Status);
		Assert.Equal(Backend.Filesystem, backup.Result.Backend);

		var backup2 = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.UnprocessableEntity, backup2.HttpStatusCode);
		Assert.Contains(backupId, backup2.Error!.Error!.First().Message);
	}

	[Fact]
	public void FailOnCreateStatusOfNotExistingBackup()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backupStatus = Client.Backup.Status(new(backupId, Backend.Filesystem));
		Assert.Equal(HttpStatusCode.NotFound, backupStatus.HttpStatusCode);
		Assert.Contains(backupId, backupStatus.Error!.Error!.First().Message);
	}

	[Fact]
	public void FailOnRestoreOfNotExistingBackup()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "not-existing-backup-id";
		var restore = Client.Backup.Restore(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.NotFound, restore.HttpStatusCode);
		Assert.Contains(backupId, restore.Error!.Error!.First().Message);
	}

	[Fact]
	public void FailOnCreateBackupForBothIncludeAndExcludeCollections()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA },
			ExcludeCollections = new[] { COLLECTION_NAME_SOUP },
			WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.UnprocessableEntity, backup.HttpStatusCode);
		Assert.Contains("include", backup.Error!.Error!.First().Message);
		Assert.Contains("exclude", backup.Error!.Error!.First().Message);
	}

	[Fact]
	public void FailOnRestoreBackupForBothIncludeAndExcludeCollections()
	{
		CreateWeaviateTestCollectionsFood(Client);

		var backupId = "backup-" + new Random().Next(int.MaxValue);

		var backup = Client.Backup.Create(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA, COLLECTION_NAME_SOUP }, WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.OK, backup.HttpStatusCode);
		var collections = backup.Result!.Collections ?? Array.Empty<string>();
		Assert.Contains(COLLECTION_NAME_PIZZA, collections);
		Assert.Contains(COLLECTION_NAME_SOUP, collections);
		Assert.Equal(backupId, backup.Result.Id);
		Assert.StartsWith(BackupsDirectory, backup.Result.Path);
		Assert.Equal(BackupStatus.Success, backup.Result.Status);
		Assert.Equal(Backend.Filesystem, backup.Result.Backend);

		DeleteAllPizzas();

		var restore = Client.Backup.Restore(new(backupId, Backend.Filesystem)
		{
			IncludeCollections = new[] { COLLECTION_NAME_PIZZA },
			ExcludeCollections = new[] { COLLECTION_NAME_SOUP },
			WaitForCompletion = true
		});

		Assert.Equal(HttpStatusCode.UnprocessableEntity, restore.HttpStatusCode);
		Assert.Contains("include", restore.Error!.Error!.First().Message);
		Assert.Contains("exclude", restore.Error!.Error!.First().Message);
	}

	private void DeleteAllPizzas()
	{
		var delete = Client.Collections.DeleteCollection(new DeleteCollectionRequest(COLLECTION_NAME_PIZZA));
		Assert.Equal(HttpStatusCode.OK, delete.HttpStatusCode);
	}

	private void AssertAllPizzasExist()
	{
		var pizzas = Client.Data.Get(new() { Collection = COLLECTION_NAME_PIZZA });
		Assert.Equal(HttpStatusCode.OK, pizzas.HttpStatusCode);
		Assert.NotNull(pizzas.Result);
		Assert.Equal(4, pizzas.Result.Length);
	}

	private void AssertAllSoupsExist()
	{
		var soups = Client.Data.Get(new() { Collection = COLLECTION_NAME_SOUP });
		Assert.Equal(HttpStatusCode.OK, soups.HttpStatusCode);
		Assert.NotNull(soups.Result);
		Assert.Equal(2, soups.Result.Length);
	}
}
