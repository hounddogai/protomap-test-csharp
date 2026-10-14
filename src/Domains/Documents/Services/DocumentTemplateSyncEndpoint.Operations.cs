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

namespace Acme.Workforce.Documents;

public sealed partial class DocumentTemplateSyncEndpoint
{
    /// <summary>
    /// Lists document templates a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentTemplateConflictsResponse> ListDocumentTemplateConflicts(ApiDocuments.ListDocumentTemplateConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentTemplateConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", "batch",
            "ListDocumentTemplateConflicts", "accepted", token);
        var response = new ApiDocuments.ListDocumentTemplateConflictsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ListDocumentTemplateConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a document template.
    /// </summary>
    public override async Task<ApiDocuments.PullDocumentTemplatesResponse> PullDocumentTemplates(ApiDocuments.PullDocumentTemplatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullDocumentTemplates for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PullDocumentTemplatesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                IssuedAt = (request.DocumentTemplate?.IssuedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "PullDocumentTemplates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a document template.
    /// </summary>
    public override async Task<ApiDocuments.PushDocumentTemplateResponse> PushDocumentTemplate(ApiDocuments.PushDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushDocumentTemplate for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PushDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = (request.DocumentTemplate?.BillingAddress ?? new ApiCommon.PostalAddress()),
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "PushDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a document template.
    /// </summary>
    public override async Task<ApiDocuments.ReconcileDocumentTemplatesResponse> ReconcileDocumentTemplates(ApiDocuments.ReconcileDocumentTemplatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileDocumentTemplates for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ReconcileDocumentTemplatesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                IssuedAt = (request.DocumentTemplate?.IssuedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ReconcileDocumentTemplates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a document template.
    /// </summary>
    public override async Task<ApiDocuments.ResolveDocumentTemplateConflictResponse> ResolveDocumentTemplateConflict(ApiDocuments.ResolveDocumentTemplateConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveDocumentTemplateConflict for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.DocumentTemplate?.Counterparty?.DocumentFolderId ?? ""),
                Region = (request.DocumentTemplate?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _sendResolveDocumentTemplateConflict0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ResolveDocumentTemplateConflictResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                IssuedAt = (request.DocumentTemplate?.IssuedAt ?? new Timestamp()),
            },
            Outcome = new ApiDocuments.ResolveDocumentTemplateConflictResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ResolveDocumentTemplateConflict", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of document templates with a peer.
    /// </summary>
    public override async Task StreamDocumentTemplateChanges(IAsyncStreamReader<ApiDocuments.StreamDocumentTemplateChangesRequest> requests, IServerStreamWriter<ApiDocuments.StreamDocumentTemplateChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling StreamDocumentTemplateChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var response = new ApiDocuments.StreamDocumentTemplateChangesResponse
            {
                DocumentTemplate = new ApiDocuments.DocumentTemplate
                {
                    BillingAddress = (request.Change?.BillingAddress ?? new ApiCommon.PostalAddress()),
                    Total = (request.Change?.Total ?? new ApiCommon.Money()),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "document_template",
                "audit", "StreamDocumentTemplateChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs a document template.
    /// </summary>
    public override async Task<ApiDocuments.SyncDocumentTemplatesResponse> SyncDocumentTemplates(ApiDocuments.SyncDocumentTemplatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncDocumentTemplates for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.SyncDocumentTemplatesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Metadata = { (request.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "SyncDocumentTemplates", (request.Reason ?? ""), token);
        return response;
    }
}
