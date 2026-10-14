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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Answers read-only questions about change requests, for screens and reports.
/// </summary>
public sealed class ChangeRequestQueryEndpoint : ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDeliveries.DeliveryService.DeliveryServiceClient _deliveriesDeliveryService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<ChangeRequestQueryEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly IRecordRepository _repository;

    public ChangeRequestQueryEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ApiDeliveries.DeliveryService.DeliveryServiceClient deliveriesDeliveryService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<ChangeRequestQueryEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        IRecordRepository repository)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _deliveriesDeliveryService = deliveriesDeliveryService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentService = documentsDocumentService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _repository = repository;
    }
    /// <summary>
    /// Lists change requests a page at a time.
    /// </summary>
    public override async Task<ApiItsm.BrowseChangeRequestsResponse> BrowseChangeRequests(ApiItsm.BrowseChangeRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseChangeRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "BrowseChangeRequests", "accepted", token);
        var response = new ApiItsm.BrowseChangeRequestsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "BrowseChangeRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts change requests by status.
    /// </summary>
    public override async Task<ApiItsm.CountChangeRequestsResponse> CountChangeRequests(ApiItsm.CountChangeRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountChangeRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "CountChangeRequests", "accepted", token);
        var profile0 = new MappingProfile<ApiItsm.CountChangeRequestsRequest, ApiItsm.NormalizeChangeRequestRequest>(value => new ApiItsm.NormalizeChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _itsmChangeRequestValidationService.NormalizeChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ExplainProblemCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainProblemCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Problem = new ApiItsm.Problem
            {
                Sla = new ApiItsm.ProblemSla
                {
                    PausedFor = (reply0.ChangeRequest?.Sla?.PausedFor ?? new Duration()),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmProblemCalculationService.ExplainProblemCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CountChangeRequestsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "CountChangeRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching change request.
    /// </summary>
    public override async Task ExportChangeRequests(ApiItsm.ExportChangeRequestsRequest request, IServerStreamWriter<ApiItsm.ExportChangeRequestsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportChangeRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "ExportChangeRequests", "accepted", token);
        var input0 = new ApiDocuments.PreviewSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PreviewSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.PreviewSignatureAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var rpc1 = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDeliveries.WatchDeliveriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDeliveries.WatchDeliveriesRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var stream2 = _deliveriesDeliveryService.WatchDeliveries(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchDeliveries frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiLicensing.ResolveSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiLicensing.ResolveSoftwareLicensesRequest>();
        using var rpc3 = _licensingSoftwareLicenseQueryService.ResolveSoftwareLicensesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.ResolveSoftwareLicensesRequest>("acme.itsm.v1.ChangeRequestQueryService/ExportChangeRequests/3", input3.Clone()), token);
        var response = new ApiItsm.ExportChangeRequestsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ExportChangeRequests", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists change requests a page at a time.
    /// </summary>
    public override async Task<ApiItsm.FindChangeRequestsResponse> FindChangeRequests(ApiItsm.FindChangeRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindChangeRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "FindChangeRequests", "accepted", token);
        var input0 = new ApiDevices.ExplainManagedDeviceDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainManagedDeviceDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiDevices.ExplainManagedDeviceDecisionRequest>(json0);
        using var rpc0 = _devicesManagedDeviceValidationService.ExplainManagedDeviceDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.FindChangeRequestsResponse
        {
            ChangeRequests = { new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ManagedDevice?.Audit?.Etag ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "FindChangeRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.LookupChangeRequestResponse> LookupChangeRequest(ApiItsm.LookupChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupChangeRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "LookupChangeRequest", "accepted", token);
        var input0 = new ApiDocuments.CreateDocumentRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Document = new ApiDocuments.Document
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
            },
            Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        using var rpc0 = _documentsDocumentService.CreateDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.LookupChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Comments = { new ApiItsm.ChangeRequestComment
                {
                    CreatedAt = (reply0.Document?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.PeekChangeRequestResponse> PeekChangeRequest(ApiItsm.PeekChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekChangeRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "PeekChangeRequest", "accepted", token);
        var response = new ApiItsm.PeekChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists change requests a page at a time.
    /// </summary>
    public override async Task<ApiItsm.SuggestChangeRequestsResponse> SuggestChangeRequests(ApiItsm.SuggestChangeRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestChangeRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "SuggestChangeRequests", "accepted", token);
        var input0 = new ApiItsm.ScoreServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScoreServiceAssetRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetValidationService.ScoreServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.LintEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ServiceAsset?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingEntitlementValidationService.LintEntitlementsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.SuggestChangeRequestsResponse
        {
            ChangeRequests = { new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "SuggestChangeRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a change request.
    /// </summary>
    public override async Task<ApiItsm.SummarizeChangeRequestsResponse> SummarizeChangeRequests(ApiItsm.SummarizeChangeRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeChangeRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "SummarizeChangeRequests", "accepted", token);
        var input0 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.CheckAuditEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.SummarizeChangeRequestsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "SummarizeChangeRequests", (request.Details?.Format ?? ""), token);
        return response;
    }
}
