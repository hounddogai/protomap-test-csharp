using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
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
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentFolderHistoryEndpoint
{
    /// <summary>
    /// Annotates a document folder.
    /// </summary>
    public override async Task<ApiDocuments.AnnotateDocumentFolderVersionResponse> AnnotateDocumentFolderVersion(ApiDocuments.AnnotateDocumentFolderVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AnnotateDocumentFolderVersion for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.DocumentFolder?.Manager?.Email ?? ""),
                },
            },
            DocumentFolderId = (request.DocumentFolder?.DocumentFolderId ?? ""),
        };
        using var rpc0 = _documentsDocumentFolderNotificationService.UnsubscribeDocumentFolderUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.AnnotateDocumentFolderVersionResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Status = (reply0.DocumentFolder?.Status ?? (ApiDocuments.DocumentFolderStatus)0),
                    } },
                } },
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Location = (request.DocumentFolder?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "AnnotateDocumentFolderVersion", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a document folder.
    /// </summary>
    public override async Task<ApiDocuments.CompareDocumentFolderVersionsResponse> CompareDocumentFolderVersions(ApiDocuments.CompareDocumentFolderVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareDocumentFolderVersions for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CompareDocumentFolderVersionsResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = (request.DocumentFolder?.Manager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "CompareDocumentFolderVersions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentFolderVersionResponse> GetDocumentFolderVersion(ApiDocuments.GetDocumentFolderVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        _logger.LogInformation("Handling GetDocumentFolderVersion for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "GetDocumentFolderVersion", "accepted", token);
        var input0 = new ApiNotifications.ResolveNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _notificationsNotificationQueryService.ResolveNotificationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.GetDocumentFolderVersionResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists document folders a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentFolderVersionsResponse> ListDocumentFolderVersions(ApiDocuments.ListDocumentFolderVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentFolderVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "ListDocumentFolderVersions", "accepted", token);
        var response = new ApiDocuments.ListDocumentFolderVersionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ListDocumentFolderVersions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a document folder.
    /// </summary>
    public override async Task<ApiDocuments.PruneDocumentFolderVersionsResponse> PruneDocumentFolderVersions(ApiDocuments.PruneDocumentFolderVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        _logger.LogInformation("Handling PruneDocumentFolderVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "PruneDocumentFolderVersions", "accepted", token);
        var response = new ApiDocuments.PruneDocumentFolderVersionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "PruneDocumentFolderVersions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Records a document folder.
    /// </summary>
    public override async Task<ApiDocuments.RecordDocumentFolderVersionResponse> RecordDocumentFolderVersion(ApiDocuments.RecordDocumentFolderVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecordDocumentFolderVersion for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RecordDocumentFolderVersionResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = (request.DocumentFolder?.Manager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "RecordDocumentFolderVersion", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reverts a document folder.
    /// </summary>
    public override async Task<ApiDocuments.RevertDocumentFolderResponse> RevertDocumentFolder(ApiDocuments.RevertDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RevertDocumentFolder for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RevertDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Location = (request.DocumentFolder?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "RevertDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
}
