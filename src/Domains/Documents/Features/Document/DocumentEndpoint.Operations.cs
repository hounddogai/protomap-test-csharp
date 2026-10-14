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

public sealed partial class DocumentEndpoint
{
    /// <summary>
    /// Takes a stream of documents and answers once at the end.
    /// </summary>
    public override async Task<ApiDocuments.BatchCreateDocumentsResponse> BatchCreateDocuments(IAsyncStreamReader<ApiDocuments.BatchCreateDocumentsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiDocuments.BatchCreateDocumentsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling BatchCreateDocuments for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var response = new ApiDocuments.BatchCreateDocumentsResponse();
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.VoucherCode ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "document",
                "audit", "BatchCreateDocuments", (request.Record?.VoucherCode ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several documents at once.
    /// </summary>
    public override async Task<ApiDocuments.BatchGetDocumentsResponse> BatchGetDocuments(ApiDocuments.BatchGetDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BatchGetDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "BatchGetDocuments", "accepted", token);
        var response = new ApiDocuments.BatchGetDocumentsResponse();
        return response;
    }
    /// <summary>
    /// Creates a document.
    /// </summary>
    public override async Task<ApiDocuments.CreateDocumentResponse> CreateDocument(ApiDocuments.CreateDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateDocument for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CreateDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.Document?.DocumentId ?? ""),
                Status = (request.Document?.Status ?? (ApiDocuments.DocumentStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "CreateDocument", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a document.
    /// </summary>
    public override async Task<ApiDocuments.DeleteDocumentResponse> DeleteDocument(ApiDocuments.DeleteDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling DeleteDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "DeleteDocument", "accepted", token);
        var response = new ApiDocuments.DeleteDocumentResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "DeleteDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentResponse> GetDocument(ApiDocuments.GetDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling GetDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "GetDocument", "accepted", token);
        var response = new ApiDocuments.GetDocumentResponse
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
    public override async Task<ApiDocuments.ListDocumentsResponse> ListDocuments(ApiDocuments.ListDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "ListDocuments", "accepted", token);
        var input0 = new ApiDocuments.ScheduleDocumentExportRequest
        {
            Annotations = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var command0 = new DispatchCommand<ApiDocuments.ScheduleDocumentExportRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentExportService.ScheduleDocumentExportAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListDocumentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ListDocuments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of documents as they happen.
    /// </summary>
    public override async Task WatchDocuments(ApiDocuments.WatchDocumentsRequest request, IServerStreamWriter<ApiDocuments.WatchDocumentsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling WatchDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "WatchDocuments", "accepted", token);
        var response = new ApiDocuments.WatchDocumentsResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "WatchDocuments", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
