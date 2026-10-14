using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentFolderSyncEndpoint
{
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentFolderSyncStateResponse> GetDocumentFolderSyncState(ApiDocuments.GetDocumentFolderSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        _logger.LogInformation("Handling GetDocumentFolderSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "GetDocumentFolderSyncState", "accepted", token);
        var response = new ApiDocuments.GetDocumentFolderSyncStateResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists document folders a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentFolderConflictsResponse> ListDocumentFolderConflicts(ApiDocuments.ListDocumentFolderConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentFolderConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "ListDocumentFolderConflicts", "accepted", token);
        var response = new ApiDocuments.ListDocumentFolderConflictsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ListDocumentFolderConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a document folder.
    /// </summary>
    public override async Task<ApiDocuments.PullDocumentFoldersResponse> PullDocumentFolders(ApiDocuments.PullDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullDocumentFolders for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PullDocumentFoldersResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Status = (request.DocumentFolder?.Status ?? (ApiDocuments.DocumentFolderStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "PullDocumentFolders", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a document folder.
    /// </summary>
    public override async Task<ApiDocuments.PushDocumentFolderResponse> PushDocumentFolder(ApiDocuments.PushDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushDocumentFolder for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PushDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Attributes = { (request.DocumentFolder?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "PushDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ReconcileDocumentFoldersResponse> ReconcileDocumentFolders(ApiDocuments.ReconcileDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileDocumentFolders for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ResolveDocumentConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.DocumentFolder?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.DocumentFolder?.Location?.PostalCode ?? ""),
                    Region = (request.DocumentFolder?.Location?.Region ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (request.DocumentFolder?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.ResolveDocumentConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ReconcileDocumentFoldersResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.Document?.Audit?.Etag ?? ""),
                        },
                    } },
                } },
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                ParentId = (request.DocumentFolder?.ParentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ReconcileDocumentFolders", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ResolveDocumentFolderConflictResponse> ResolveDocumentFolderConflict(ApiDocuments.ResolveDocumentFolderConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveDocumentFolderConflict for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ResolveDocumentFolderConflictResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Location = (request.DocumentFolder?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ResolveDocumentFolderConflict", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of document folders with a peer.
    /// </summary>
    public override async Task StreamDocumentFolderChanges(IAsyncStreamReader<ApiDocuments.StreamDocumentFolderChangesRequest> requests, IServerStreamWriter<ApiDocuments.StreamDocumentFolderChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling StreamDocumentFolderChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var response = new ApiDocuments.StreamDocumentFolderChangesResponse
            {
                DocumentFolder = new ApiDocuments.DocumentFolder
                {
                    DocumentFolderId = (request.Change?.DocumentFolderId ?? ""),
                    Location = (request.Change?.Location ?? new ApiCommon.PostalAddress()),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "document_folder",
                "audit", "StreamDocumentFolderChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs a document folder.
    /// </summary>
    public override async Task<ApiDocuments.SyncDocumentFoldersResponse> SyncDocumentFolders(ApiDocuments.SyncDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncDocumentFolders for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.SyncDocumentFoldersResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Location = (request.DocumentFolder?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "SyncDocumentFolders", (request.Reason ?? ""), token);
        return response;
    }
}
