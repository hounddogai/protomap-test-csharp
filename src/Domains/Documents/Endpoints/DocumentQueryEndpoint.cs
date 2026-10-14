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
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Answers read-only questions about documents, for screens and reports.
/// </summary>
public sealed class DocumentQueryEndpoint : ApiDocuments.DocumentQueryService.DocumentQueryServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<DocumentQueryEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public DocumentQueryEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        CallInvoker invoker,
        ILogger<DocumentQueryEndpoint> logger,
        IRecordRepository repository,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _invoker = invoker;
        _logger = logger;
        _repository = repository;
        _venuesVenueValidationService = venuesVenueValidationService;
    }
    /// <summary>
    /// Lists documents a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.BrowseDocumentsResponse> BrowseDocuments(ApiDocuments.BrowseDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "BrowseDocuments", "accepted", token);
        var response = new ApiDocuments.BrowseDocumentsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "BrowseDocuments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts documents by status.
    /// </summary>
    public override async Task<ApiDocuments.CountDocumentsResponse> CountDocuments(ApiDocuments.CountDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "CountDocuments", "accepted", token);
        var response = new ApiDocuments.CountDocumentsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "CountDocuments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching document.
    /// </summary>
    public override async Task ExportDocuments(ApiDocuments.ExportDocumentsRequest request, IServerStreamWriter<ApiDocuments.ExportDocumentsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "ExportDocuments", "accepted", token);
        var input0 = new ApiVenues.NormalizeVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _venuesVenueValidationService.NormalizeVenueAsync);
        var response = new ApiDocuments.ExportDocumentsResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ExportDocuments", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists documents a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.FindDocumentsResponse> FindDocuments(ApiDocuments.FindDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "FindDocuments", "accepted", token);
        var response = new ApiDocuments.FindDocumentsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "FindDocuments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.LookupDocumentResponse> LookupDocument(ApiDocuments.LookupDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "LookupDocument", "accepted", token);
        var input0 = new ApiDocuments.ReconcileDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentFoldersRequest.Types.Details
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
                            Email = (request.Context?.Actor?.Email ?? ""),
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.ReconcileDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.LookupDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                DocumentId = (request.DocumentId ?? ""),
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (reply0.DocumentFolder?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.PeekDocumentResponse> PeekDocument(ApiDocuments.PeekDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekDocument for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "PeekDocument", "accepted", token);
        var input0 = new ApiDocuments.StreamDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateExportRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        using var stream0 = _documentsDocumentTemplateExportService.StreamDocumentTemplateExport(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateExport frame {Type}", change0.GetType().Name);
        }
        var response = new ApiDocuments.PeekDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several documents at once.
    /// </summary>
    public override async Task<ApiDocuments.ResolveDocumentsResponse> ResolveDocuments(ApiDocuments.ResolveDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "ResolveDocuments", "accepted", token);
        var response = new ApiDocuments.ResolveDocumentsResponse();
        return response;
    }
    /// <summary>
    /// Lists documents a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.SuggestDocumentsResponse> SuggestDocuments(ApiDocuments.SuggestDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "SuggestDocuments", "accepted", token);
        var input0 = new ApiAuthorization.BrowseAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.BrowseAccessPoliciesRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        var descriptor0 = new Method<ApiAuthorization.BrowseAccessPoliciesRequest, ApiAuthorization.BrowseAccessPoliciesResponse>(
            MethodType.Unary, "acme.authorization.v1" + ".AccessPolicyQueryService", "BrowseAccessPolicies",
            Wire.Marshaller<ApiAuthorization.BrowseAccessPoliciesRequest>(), Wire.Marshaller<ApiAuthorization.BrowseAccessPoliciesResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.SuggestDocumentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "SuggestDocuments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a document.
    /// </summary>
    public override async Task<ApiDocuments.SummarizeDocumentsResponse> SummarizeDocuments(ApiDocuments.SummarizeDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeDocuments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "SummarizeDocuments", "accepted", token);
        var response = new ApiDocuments.SummarizeDocumentsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "SummarizeDocuments", (request.Reason ?? ""), token);
        return response;
    }
}
