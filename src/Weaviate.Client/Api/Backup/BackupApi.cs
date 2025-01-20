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

// ReSharper disable once CheckNamespace
namespace Weaviate.Client;

public class BackupApi
{
    private readonly Transport _transport;

    public BackupApi(Transport transport)
    {
        _transport = transport;
    }

    public ApiResponse<BackupResponse> Create(CreateBackupRequest request)
    {
        var path = $"/backups/{request.Backend}/{request.Id}";
        if (request.WaitForCompletion.HasValue)
            path += $"?wait={request.WaitForCompletion.Value.ToString().ToLowerInvariant()}";

        return _transport.Post<BackupResponse, BackupRequestDto>(path, new BackupRequestDto
        {
            Include = request.IncludeCollections,
            Exclude = request.ExcludeCollections
        });
    }

    public ApiResponse<BackupResponse> Restore(RestoreBackupRequest request)
    {
        var path = $"/backups/{request.Backend}/{request.Id}/restore";
        if (request.WaitForCompletion.HasValue)
            path += $"?wait={request.WaitForCompletion.Value.ToString().ToLowerInvariant()}";

        return _transport.Post<BackupResponse, BackupRequestDto>(path, new BackupRequestDto
        {
            Include = request.IncludeCollections,
            Exclude = request.ExcludeCollections
        });
    }

    public ApiResponse<BackupResponse> Status(BackupStatusRequest request)
    {
        return _transport.Get<BackupResponse>($"/backups/{request.Backend}/{request.Id}");
    }

    public ApiResponse<BackupResponse> RestoreStatus(BackupStatusRequest request)
    {
        return _transport.Get<BackupResponse>($"/backups/{request.Backend}/{request.Id}/restore");
    }
}
