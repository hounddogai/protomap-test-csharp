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
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentTemplateExportEndpoint
{
    /// <summary>
    /// Cancels a document template.
    /// </summary>
    public override async Task<ApiDocuments.CancelDocumentTemplateExportResponse> CancelDocumentTemplateExport(ApiDocuments.CancelDocumentTemplateExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        _logger.LogInformation("Handling CancelDocumentTemplateExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "CancelDocumentTemplateExport", "accepted", token);
        var input0 = new ApiVenues.BrowseVenuesRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiVenues.BrowseVenuesRequest.Types.Filter
        {
            CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
        };
        using var rpc0 = _venuesVenueQueryService.BrowseVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.CancelDocumentTemplateExportResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Outcome = new ApiDocuments.CancelDocumentTemplateExportResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "CancelDocumentTemplateExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document template.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentTemplateExportResponse> GetDocumentTemplateExport(ApiDocuments.GetDocumentTemplateExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        _logger.LogInformation("Handling GetDocumentTemplateExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "GetDocumentTemplateExport", "accepted", token);
        var response = new ApiDocuments.GetDocumentTemplateExportResponse
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
    public override async Task<ApiDocuments.ListDocumentTemplateExportsResponse> ListDocumentTemplateExports(ApiDocuments.ListDocumentTemplateExportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentTemplateExports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", "batch",
            "ListDocumentTemplateExports", "accepted", token);
        var response = new ApiDocuments.ListDocumentTemplateExportsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ListDocumentTemplateExports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a document template.
    /// </summary>
    public override async Task<ApiDocuments.RetryDocumentTemplateExportResponse> RetryDocumentTemplateExport(ApiDocuments.RetryDocumentTemplateExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryDocumentTemplateExport for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RetryDocumentTemplateExportResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = (request.DocumentTemplate?.Counterparty ?? new ApiDocuments.DocumentFolderRef()),
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "RetryDocumentTemplateExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a document template.
    /// </summary>
    public override async Task<ApiDocuments.ScheduleDocumentTemplateExportResponse> ScheduleDocumentTemplateExport(ApiDocuments.ScheduleDocumentTemplateExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleDocumentTemplateExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "ScheduleDocumentTemplateExport", "accepted", token);
        var response = new ApiDocuments.ScheduleDocumentTemplateExportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ScheduleDocumentTemplateExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a document template.
    /// </summary>
    public override async Task<ApiDocuments.ShareDocumentTemplateExportResponse> ShareDocumentTemplateExport(ApiDocuments.ShareDocumentTemplateExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareDocumentTemplateExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "ShareDocumentTemplateExport", "accepted", token);
        var response = new ApiDocuments.ShareDocumentTemplateExportResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ShareDocumentTemplateExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching document template.
    /// </summary>
    public override async Task StreamDocumentTemplateExport(ApiDocuments.StreamDocumentTemplateExportRequest request, IServerStreamWriter<ApiDocuments.StreamDocumentTemplateExportResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamDocumentTemplateExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", "batch",
            "StreamDocumentTemplateExport", "accepted", token);
        var response = new ApiDocuments.StreamDocumentTemplateExportResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "StreamDocumentTemplateExport", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
