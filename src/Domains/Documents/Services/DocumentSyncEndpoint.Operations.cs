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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentSyncEndpoint
{
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentSyncStateResponse> GetDocumentSyncState(ApiDocuments.GetDocumentSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling GetDocumentSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "GetDocumentSyncState", "accepted", token);
        var response = new ApiDocuments.GetDocumentSyncStateResponse
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
    public override async Task<ApiDocuments.ListDocumentConflictsResponse> ListDocumentConflicts(ApiDocuments.ListDocumentConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "ListDocumentConflicts", "accepted", token);
        var profile0 = new MappingProfile<ApiDocuments.ListDocumentConflictsRequest, ApiAuthorization.ScoreAccessPolicyRequest>(value => new ApiAuthorization.ScoreAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.ScoreAccessPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _authorizationAccessPolicyValidationService.ScoreAccessPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListDocumentConflictsResponse
        {
            Documents = { new ApiDocuments.Document
            {
                Lines = { new ApiDocuments.DocumentLine
                {
                    Amount = (reply0.AccessPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ListDocumentConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a document.
    /// </summary>
    public override async Task<ApiDocuments.PullDocumentsResponse> PullDocuments(ApiDocuments.PullDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullDocuments for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PullDocumentsResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                DueAt = (request.Document?.DueAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "PullDocuments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a document.
    /// </summary>
    public override async Task<ApiDocuments.PushDocumentResponse> PushDocument(ApiDocuments.PushDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushDocument for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PushDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Notes = (request.Document?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "PushDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a document.
    /// </summary>
    public override async Task<ApiDocuments.ReconcileDocumentsResponse> ReconcileDocuments(ApiDocuments.ReconcileDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileDocuments for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ReconcileDocumentsResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Status = (request.Document?.Status ?? (ApiDocuments.DocumentStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ReconcileDocuments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a document.
    /// </summary>
    public override async Task<ApiDocuments.ResolveDocumentConflictResponse> ResolveDocumentConflict(ApiDocuments.ResolveDocumentConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveDocumentConflict for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.SummarizeMetricSeriesesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.SummarizeMetricSeriesesRequest.Types.Details
            {
                Filters = { (request.Document?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _metricsMetricSeriesQueryService.SummarizeMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ResolveDocumentConflictResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = (request.Document?.BillingAddress ?? new ApiCommon.PostalAddress()),
                DocumentId = (request.DocumentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Document?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ResolveDocumentConflict", (request.Document?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of documents with a peer.
    /// </summary>
    public override async Task StreamDocumentChanges(IAsyncStreamReader<ApiDocuments.StreamDocumentChangesRequest> requests, IServerStreamWriter<ApiDocuments.StreamDocumentChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling StreamDocumentChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var response = new ApiDocuments.StreamDocumentChangesResponse
            {
                Document = new ApiDocuments.Document
                {
                    Number = (request.Change?.Number ?? ""),
                    Total = (request.Change?.Total ?? new ApiCommon.Money()),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "document",
                "audit", "StreamDocumentChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs a document.
    /// </summary>
    public override async Task<ApiDocuments.SyncDocumentsResponse> SyncDocuments(ApiDocuments.SyncDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncDocuments for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.SyncDocumentsResponse
        {
            Document = new ApiDocuments.Document
            {
                Counterparty = (request.Document?.Counterparty ?? new ApiDocuments.DocumentFolderRef()),
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "SyncDocuments", (request.Reason ?? ""), token);
        return response;
    }
}
