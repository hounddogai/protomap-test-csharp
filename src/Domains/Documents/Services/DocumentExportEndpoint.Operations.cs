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

public sealed partial class DocumentExportEndpoint
{
    /// <summary>
    /// Cancels a document.
    /// </summary>
    public override async Task<ApiDocuments.CancelDocumentExportResponse> CancelDocumentExport(ApiDocuments.CancelDocumentExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling CancelDocumentExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "CancelDocumentExport", "accepted", token);
        var response = new ApiDocuments.CancelDocumentExportResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
            Outcome = new ApiDocuments.CancelDocumentExportResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "CancelDocumentExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentExportResponse> GetDocumentExport(ApiDocuments.GetDocumentExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling GetDocumentExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "GetDocumentExport", "accepted", token);
        var response = new ApiDocuments.GetDocumentExportResponse
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
    public override async Task<ApiDocuments.ListDocumentExportsResponse> ListDocumentExports(ApiDocuments.ListDocumentExportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentExports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "ListDocumentExports", "accepted", token);
        var input0 = new ApiDocuments.PushDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                DueAt = (request.Context?.Deadline ?? new Timestamp()),
                Metadata = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.PushDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListDocumentExportsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ListDocumentExports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a document.
    /// </summary>
    public override async Task<ApiDocuments.RetryDocumentExportResponse> RetryDocumentExport(ApiDocuments.RetryDocumentExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryDocumentExport for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RetryDocumentExportResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Notes = (request.Document?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "RetryDocumentExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a document.
    /// </summary>
    public override async Task<ApiDocuments.ScheduleDocumentExportResponse> ScheduleDocumentExport(ApiDocuments.ScheduleDocumentExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleDocumentExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "ScheduleDocumentExport", "accepted", token);
        var input0 = new ApiDocuments.PullDocumentsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentId = (request.DocumentId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentSyncService.PullDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ScheduleDocumentExportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ScheduleDocumentExport", (request.Details?.Format ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a document.
    /// </summary>
    public override async Task<ApiDocuments.ShareDocumentExportResponse> ShareDocumentExport(ApiDocuments.ShareDocumentExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareDocumentExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "ShareDocumentExport", "accepted", token);
        var response = new ApiDocuments.ShareDocumentExportResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ShareDocumentExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching document.
    /// </summary>
    public override async Task StreamDocumentExport(ApiDocuments.StreamDocumentExportRequest request, IServerStreamWriter<ApiDocuments.StreamDocumentExportResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamDocumentExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "StreamDocumentExport", "accepted", token);
        var response = new ApiDocuments.StreamDocumentExportResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "StreamDocumentExport", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
