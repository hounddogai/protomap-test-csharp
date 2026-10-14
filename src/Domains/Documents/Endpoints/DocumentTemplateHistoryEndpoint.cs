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

/// <summary>
/// Keeps every version of a document template and restores earlier ones.
/// </summary>
public sealed class DocumentTemplateHistoryEndpoint : ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceBase
{
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ILogger<DocumentTemplateHistoryEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentTemplateHistoryEndpoint(
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ILogger<DocumentTemplateHistoryEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Annotates a document template.
    /// </summary>
    public override async Task<ApiDocuments.AnnotateDocumentTemplateVersionResponse> AnnotateDocumentTemplateVersion(ApiDocuments.AnnotateDocumentTemplateVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AnnotateDocumentTemplateVersion for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.AnnotateDocumentTemplateVersionResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = (request.DocumentTemplate?.Counterparty ?? new ApiDocuments.DocumentFolderRef()),
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "AnnotateDocumentTemplateVersion", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document template.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentTemplateVersionResponse> GetDocumentTemplateVersion(ApiDocuments.GetDocumentTemplateVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetDocumentTemplateVersion for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "GetDocumentTemplateVersion", "accepted", token);
        var response = new ApiDocuments.GetDocumentTemplateVersionResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists document templates a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentTemplateVersionsResponse> ListDocumentTemplateVersions(ApiDocuments.ListDocumentTemplateVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListDocumentTemplateVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", "batch",
            "ListDocumentTemplateVersions", "accepted", token);
        var response = new ApiDocuments.ListDocumentTemplateVersionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ListDocumentTemplateVersions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a document template.
    /// </summary>
    public override async Task<ApiDocuments.PruneDocumentTemplateVersionsResponse> PruneDocumentTemplateVersions(ApiDocuments.PruneDocumentTemplateVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PruneDocumentTemplateVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "PruneDocumentTemplateVersions", "accepted", token);
        var input0 = new ApiDocuments.StreamDocumentNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentNoticesRequest.Types.Filter
            {
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.DocumentTemplateId ?? ""),
            },
            Since = (request.Context?.Deadline ?? new Timestamp()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _documentsDocumentNotificationService.StreamDocumentNotices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentNotices frame {Type}", change0.GetType().Name);
        }
        var response = new ApiDocuments.PruneDocumentTemplateVersionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "PruneDocumentTemplateVersions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Records a document template.
    /// </summary>
    public override async Task<ApiDocuments.RecordDocumentTemplateVersionResponse> RecordDocumentTemplateVersion(ApiDocuments.RecordDocumentTemplateVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecordDocumentTemplateVersion for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RecordDocumentTemplateVersionResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Metadata = { (request.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "RecordDocumentTemplateVersion", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reverts a document template.
    /// </summary>
    public override async Task<ApiDocuments.RevertDocumentTemplateResponse> RevertDocumentTemplate(ApiDocuments.RevertDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RevertDocumentTemplate for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RevertDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Number = (request.DocumentTemplate?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "RevertDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
}
