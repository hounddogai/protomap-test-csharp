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
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentHistoryEndpoint
{
    /// <summary>
    /// Annotates a document.
    /// </summary>
    public override async Task<ApiDocuments.AnnotateDocumentVersionResponse> AnnotateDocumentVersion(ApiDocuments.AnnotateDocumentVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AnnotateDocumentVersion for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.AnnotateDocumentVersionResponse
        {
            Document = new ApiDocuments.Document
            {
                Counterparty = (request.Document?.Counterparty ?? new ApiDocuments.DocumentFolderRef()),
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "AnnotateDocumentVersion", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a document.
    /// </summary>
    public override async Task<ApiDocuments.CompareDocumentVersionsResponse> CompareDocumentVersions(ApiDocuments.CompareDocumentVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareDocumentVersions for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CompareDocumentVersionsResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Notes = (request.Document?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "CompareDocumentVersions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentVersionResponse> GetDocumentVersion(ApiDocuments.GetDocumentVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling GetDocumentVersion for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "GetDocumentVersion", "accepted", token);
        var response = new ApiDocuments.GetDocumentVersionResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists documents a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentVersionsResponse> ListDocumentVersions(ApiDocuments.ListDocumentVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "ListDocumentVersions", "accepted", token);
        var response = new ApiDocuments.ListDocumentVersionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ListDocumentVersions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a document.
    /// </summary>
    public override async Task<ApiDocuments.PruneDocumentVersionsResponse> PruneDocumentVersions(ApiDocuments.PruneDocumentVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling PruneDocumentVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "PruneDocumentVersions", "accepted", token);
        var response = new ApiDocuments.PruneDocumentVersionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "PruneDocumentVersions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Records a document.
    /// </summary>
    public override async Task<ApiDocuments.RecordDocumentVersionResponse> RecordDocumentVersion(ApiDocuments.RecordDocumentVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecordDocumentVersion for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RecordDocumentVersionResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Status = (request.Document?.Status ?? (ApiDocuments.DocumentStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "RecordDocumentVersion", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reverts a document.
    /// </summary>
    public override async Task<ApiDocuments.RevertDocumentResponse> RevertDocument(ApiDocuments.RevertDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RevertDocument for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RevertDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                DueAt = (request.Document?.DueAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "RevertDocument", (request.Reason ?? ""), token);
        return response;
    }
}
