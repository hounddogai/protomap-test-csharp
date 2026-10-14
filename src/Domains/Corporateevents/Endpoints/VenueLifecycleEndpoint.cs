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
using ApiCompliance = Acme.Compliance.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Moves a venue between the states of its lifecycle.
/// </summary>
public sealed class VenueLifecycleEndpoint : ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceBase
{
    private readonly ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient _complianceComplianceCaseQueryService;
    private readonly ApiConcessions.ConcessionStandService.ConcessionStandServiceClient _concessionsConcessionStandService;
    private readonly ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient _corporateeventsCorporateEventLifecycleService;
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient _itsmChangeRequestWorkflowService;
    private readonly ApiItsm.ProblemReportingService.ProblemReportingServiceClient _itsmProblemReportingService;
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _itsmServiceAssetWorkflowService;
    private readonly ILogger<VenueLifecycleEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly IVenueLifecycleEndpointSendRenewVenue0Port _venueLifecycleEndpointSendRenewVenue0Port;

    public VenueLifecycleEndpoint(
        ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient complianceComplianceCaseQueryService,
        ApiConcessions.ConcessionStandService.ConcessionStandServiceClient concessionsConcessionStandService,
        ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient corporateeventsCorporateEventLifecycleService,
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient itsmChangeRequestWorkflowService,
        ApiItsm.ProblemReportingService.ProblemReportingServiceClient itsmProblemReportingService,
        ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient itsmServiceAssetWorkflowService,
        ILogger<VenueLifecycleEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        IVenueLifecycleEndpointSendRenewVenue0Port venueLifecycleEndpointSendRenewVenue0PortAdapter)
    {
        _complianceComplianceCaseQueryService = complianceComplianceCaseQueryService;
        _concessionsConcessionStandService = concessionsConcessionStandService;
        _corporateeventsCorporateEventLifecycleService = corporateeventsCorporateEventLifecycleService;
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _itsmChangeRequestWorkflowService = itsmChangeRequestWorkflowService;
        _itsmProblemReportingService = itsmProblemReportingService;
        _itsmServiceAssetWorkflowService = itsmServiceAssetWorkflowService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _venueLifecycleEndpointSendRenewVenue0Port = venueLifecycleEndpointSendRenewVenue0PortAdapter;
    }
    /// <summary>
    /// Activates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ActivateVenueResponse> ActivateVenue(ApiCorporateevents.ActivateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateVenue for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "ActivateVenue", "accepted", token);
        var input0 = new ApiCorporateevents.ScheduleCorporateEventExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsCorporateEventLifecycleService.ScheduleCorporateEventExpiryAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.SendSignatureTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendSignatureTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsSignatureNotificationService.SendSignatureTestNoticeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ActivateVenueResponse
        {
            Outcome = new ApiCorporateevents.ActivateVenueResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ActivateVenue", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ExpireVenueResponse> ExpireVenue(ApiCorporateevents.ExpireVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireVenue for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "ExpireVenue", "accepted", token);
        var input0 = new ApiHelpdesk.StartTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _helpdeskTicketWorkflowService.StartTicketWorkflowAsync);
        var input1 = new ApiCorporateevents.ResolveVenueConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ResolveVenueConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Ticket?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsVenueSyncService.ResolveVenueConflictAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiConcessions.UpdateConcessionStandRequest
        {
            ConcessionStand = new ApiConcessions.ConcessionStand
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Ticket?.Assignee?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named2 = _registry.Get<ApiConcessions.ConcessionStandService.ConcessionStandServiceClient>("acme.concessions.v1.ConcessionStandService");
        using var rpc2 = named2.UpdateConcessionStandAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.ShareProblemReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ShareProblemReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply0.Ticket?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc3 = _itsmProblemReportingService.ShareProblemReportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.ExpireVenueResponse
        {
            Outcome = new ApiCorporateevents.ExpireVenueResponse.Types.Outcome
            {
                EffectiveAt = (reply3.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply3.Result?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Dimensions = new ApiCorporateevents.VenueDimensions
                {
                    Width = (reply1.Venue?.Dimensions?.Width ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ExpireVenue", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.GetVenueLifecycleResponse> GetVenueLifecycle(ApiCorporateevents.GetVenueLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetVenueLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "GetVenueLifecycle", "accepted", token);
        var response = new ApiCorporateevents.GetVenueLifecycleResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ReinstateVenueResponse> ReinstateVenue(ApiCorporateevents.ReinstateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateVenue for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "ReinstateVenue", "accepted", token);
        var input0 = new ApiCorporateevents.BrowseRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.BrowseRegistrationsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.BrowseRegistrationsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsRegistrationQueryService.BrowseRegistrationsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiCorporateevents.ReinstateVenueRequest, ApiCorporateevents.ScheduleRegistrationReportRequest>(value => new ApiCorporateevents.ScheduleRegistrationReportRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _corporateeventsRegistrationReportingService.ScheduleRegistrationReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMetrics.LintMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.LintMetricSeriesesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _metricsMetricSeriesValidationService.LintMetricSeriesesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.ReconcileDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.MetricSeries?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Description = (reply2.MetricSeries?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                    Quantity = (reply2.MetricSeries?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc3 = _documentsDocumentSyncService.ReconcileDocumentsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.ReinstateVenueResponse
        {
            Outcome = new ApiCorporateevents.ReinstateVenueResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ReinstateVenue", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.RenewVenueResponse> RenewVenue(ApiCorporateevents.RenewVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewVenue for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "RenewVenue", "accepted", token);
        var input0 = new ApiDocuments.CalculateDocumentTemplateRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await SendRenewVenue0Async(input0, token);
        var response = new ApiCorporateevents.RenewVenueResponse
        {
            Outcome = new ApiCorporateevents.RenewVenueResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "RenewVenue", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ScheduleVenueExpiryResponse> ScheduleVenueExpiry(ApiCorporateevents.ScheduleVenueExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleVenueExpiry for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCompliance.ResolveComplianceCasesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _complianceComplianceCaseQueryService.ResolveComplianceCasesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ResumeTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ResumeTicketWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Venue?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiHelpdesk.ResumeTicketWorkflowRequest>();
        using var rpc1 = _helpdeskTicketWorkflowService.ResumeTicketWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ScheduleVenueExpiryResponse
        {
            Outcome = new ApiCorporateevents.ScheduleVenueExpiryResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Capacity = (request.Venue?.Capacity ?? 0L),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Ticket?.Assignee?.Email ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ScheduleVenueExpiry", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.SuspendVenueResponse> SuspendVenue(ApiCorporateevents.SuspendVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendVenue for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "SuspendVenue", "accepted", token);
        var input0 = new ApiDevices.RetryComplianceScanWorkflowRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RetryComplianceScanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanWorkflowService.RetryComplianceScanWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ScheduleRegistrationReportRequest();
        input1.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiCorporateevents.ScheduleRegistrationReportRequest.Types.Details
        {
            Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        input1.Reason = (request.Reason ?? "");
        using var rpc1 = _corporateeventsRegistrationReportingService.ScheduleRegistrationReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.PushDocumentRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply0.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (reply0.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc2 = _documentsDocumentSyncService.PushDocumentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.SuspendVenueResponse
        {
            Outcome = new ApiCorporateevents.SuspendVenueResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Document?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "SuspendVenue", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.TerminateVenueResponse> TerminateVenue(ApiCorporateevents.TerminateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateVenue for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "TerminateVenue", "accepted", token);
        var input0 = new ApiItsm.CancelServiceAssetWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CancelServiceAssetWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmServiceAssetWorkflowService.CancelServiceAssetWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiItsm.CancelChangeRequestWorkflowRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _itsmChangeRequestWorkflowService.CancelChangeRequestWorkflowAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.PreviewSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PreviewSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _documentsSignatureValidationService.PreviewSignatureAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiCorporateevents.TerminateVenueResponse
        {
            Outcome = new ApiCorporateevents.TerminateVenueResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Dimensions = new ApiCorporateevents.VenueDimensions
                {
                    Length = (reply0.ServiceAsset?.Dimensions?.Length ?? 0d),
                    Weight = (reply0.ServiceAsset?.Dimensions?.Weight ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "TerminateVenue", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.CalculateDocumentTemplateResponse> SendRenewVenue0Async(ApiDocuments.CalculateDocumentTemplateRequest request, CancellationToken token)
    {
        return await _venueLifecycleEndpointSendRenewVenue0Port.SendAsync(request, token);
    }
}
