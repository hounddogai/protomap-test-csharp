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
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentAdminEndpoint
{
    /// <summary>
    /// Archives a document.
    /// </summary>
    public override async Task<ApiDocuments.ArchiveDocumentResponse> ArchiveDocument(ApiDocuments.ArchiveDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling ArchiveDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "ArchiveDocument", "accepted", token);
        var input0 = new ApiMetrics.ListMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.ListMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _sendArchiveDocument0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ArchiveDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
            Outcome = new ApiDocuments.ArchiveDocumentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ArchiveDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.InspectDocumentResponse> InspectDocument(ApiDocuments.InspectDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling InspectDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "InspectDocument", "accepted", token);
        var response = new ApiDocuments.InspectDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Locks a document.
    /// </summary>
    public override async Task<ApiDocuments.LockDocumentResponse> LockDocument(ApiDocuments.LockDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling LockDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "LockDocument", "accepted", token);
        var response = new ApiDocuments.LockDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
            Outcome = new ApiDocuments.LockDocumentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "LockDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a document.
    /// </summary>
    public override async Task<ApiDocuments.PurgeDocumentResponse> PurgeDocument(ApiDocuments.PurgeDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling PurgeDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "PurgeDocument", "accepted", token);
        var response = new ApiDocuments.PurgeDocumentResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "PurgeDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reassigns a document.
    /// </summary>
    public override async Task<ApiDocuments.ReassignDocumentResponse> ReassignDocument(ApiDocuments.ReassignDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling ReassignDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "ReassignDocument", "accepted", token);
        var response = new ApiDocuments.ReassignDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.Assignee?.DisplayName ?? ""),
                },
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ReassignDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Restores a document.
    /// </summary>
    public override async Task<ApiDocuments.RestoreDocumentResponse> RestoreDocument(ApiDocuments.RestoreDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling RestoreDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "RestoreDocument", "accepted", token);
        var response = new ApiDocuments.RestoreDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
            Outcome = new ApiDocuments.RestoreDocumentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "RestoreDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Transfers a document.
    /// </summary>
    public override async Task<ApiDocuments.TransferDocumentResponse> TransferDocument(ApiDocuments.TransferDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling TransferDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "TransferDocument", "accepted", token);
        var input0 = new ApiDocuments.SyncDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                        },
                    } },
                } },
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.SyncDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.TransferDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
            Outcome = new ApiDocuments.TransferDocumentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "TransferDocument", (request.Reason ?? ""), token);
        return response;
    }
}
