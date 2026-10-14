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
using ApiCertificates = Acme.Certificates.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Moves a corporate event between the states of its lifecycle.
/// </summary>
public sealed class CorporateEventLifecycleEndpoint : ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly NotificationBus _bus;
    private readonly ApiCertificates.CertificateQueryService.CertificateQueryServiceClient _certificatesCertificateQueryService;
    private readonly ICorporateEventLifecycleEndpointSendExpireCorporateEvent2Port _corporateEventLifecycleEndpointSendExpireCorporateEvent2Port;
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient _itsmServiceAssetLifecycleService;
    private readonly ILogger<CorporateEventLifecycleEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;
    private readonly ApiWarehousing.WarehouseService.WarehouseServiceClient _warehousingWarehouseService;

    public CorporateEventLifecycleEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        NotificationBus bus,
        ApiCertificates.CertificateQueryService.CertificateQueryServiceClient certificatesCertificateQueryService,
        ICorporateEventLifecycleEndpointSendExpireCorporateEvent2Port corporateEventLifecycleEndpointSendExpireCorporateEvent2PortAdapter,
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient itsmServiceAssetLifecycleService,
        ILogger<CorporateEventLifecycleEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService,
        ApiWarehousing.WarehouseService.WarehouseServiceClient warehousingWarehouseService)
    {
        _auditAuditEventService = auditAuditEventService;
        _bus = bus;
        _certificatesCertificateQueryService = certificatesCertificateQueryService;
        _corporateEventLifecycleEndpointSendExpireCorporateEvent2Port = corporateEventLifecycleEndpointSendExpireCorporateEvent2PortAdapter;
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _invoker = invoker;
        _itsmServiceAssetLifecycleService = itsmServiceAssetLifecycleService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _venuesVenueService = venuesVenueService;
        _warehousingWarehouseService = warehousingWarehouseService;
    }
    /// <summary>
    /// Activates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ActivateCorporateEventResponse> ActivateCorporateEvent(ApiCorporateevents.ActivateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "ActivateCorporateEvent", "accepted", token);
        var input0 = new ApiCorporateevents.SyncVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.SyncVenuesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueSyncService.SyncVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiCorporateevents.ActivateCorporateEventRequest, ApiFeatureflags.SummarizeFeatureFlagsRequest>(value => new ApiFeatureflags.SummarizeFeatureFlagsRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Reason = (value.Reason ?? ""),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _featureflagsFeatureFlagQueryService.SummarizeFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ActivateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Venue?.Address?.Recipient ?? ""),
                },
            },
            Outcome = new ApiCorporateevents.ActivateCorporateEventResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ActivateCorporateEvent", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ExpireCorporateEventResponse> ExpireCorporateEvent(ApiCorporateevents.ExpireCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "ExpireCorporateEvent", "accepted", token);
        var input0 = new ApiCorporateevents.GenerateRegistrationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _corporateeventsRegistrationReportingService.GenerateRegistrationReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ExpireCorporateEventToBrowseCertificates1(reply0);
        using var rpc1 = _certificatesCertificateQueryService.BrowseCertificatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.LintCorporateEventsRequest
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
            Details = new ApiCorporateevents.LintCorporateEventsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply2 = await SendExpireCorporateEvent2Async(input2, token);
        var response = new ApiCorporateevents.ExpireCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Outcome = new ApiCorporateevents.ExpireCorporateEventResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ExpireCorporateEvent", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reinstates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ReinstateCorporateEventResponse> ReinstateCorporateEvent(ApiCorporateevents.ReinstateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "ReinstateCorporateEvent", "accepted", token);
        var input0 = new ApiCorporateevents.DeleteCorporateEventReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.CorporateEventId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _corporateeventsCorporateEventReportingService.DeleteCorporateEventReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ReinstateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
            Outcome = new ApiCorporateevents.ReinstateCorporateEventResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ReinstateCorporateEvent", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.RenewCorporateEventResponse> RenewCorporateEvent(ApiCorporateevents.RenewCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "RenewCorporateEvent", "accepted", token);
        var input0 = new ApiFeatureflags.SummarizeFeatureFlagsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _featureflagsFeatureFlagQueryService.SummarizeFeatureFlagsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _venuesVenueService.UpdateVenueAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiCorporateevents.RenewCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
            Outcome = new ApiCorporateevents.RenewCorporateEventResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "RenewCorporateEvent", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ScheduleCorporateEventExpiryResponse> ScheduleCorporateEventExpiry(ApiCorporateevents.ScheduleCorporateEventExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleCorporateEventExpiry for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWarehousing.BatchGetWarehousesRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _warehousingWarehouseService.BatchGetWarehousesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ScheduleServiceAssetExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScheduleServiceAssetExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _itsmServiceAssetLifecycleService.ScheduleServiceAssetExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiAudit.UpdateAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    Unit = (reply1.ServiceAsset?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (reply1.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _auditAuditEventService.UpdateAuditEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.ScheduleCorporateEventExpiryResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ServiceAsset?.Audit?.Etag ?? ""),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.AuditEvent?.Subject?.DisplayName ?? ""),
                },
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.AuditEvent?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ScheduleCorporateEventExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.SuspendCorporateEventResponse> SuspendCorporateEvent(ApiCorporateevents.SuspendCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "SuspendCorporateEvent", "accepted", token);
        var input0 = new ApiCorporateevents.ShareCorporateEventReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareCorporateEventReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventReportingService.ShareCorporateEventReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCorporateevents.ShareCorporateEventReportRequest>("acme.corporateevents.v1.CorporateEventLifecycleService/SuspendCorporateEvent/0", input0.Clone()), token);
        var input1 = new ApiCorporateevents.ScheduleCorporateEventReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (reply0.CorporateEvent?.CorporateEventId ?? ""),
            Details = new ApiCorporateevents.ScheduleCorporateEventReportRequest.Types.Details
            {
                Period = (reply0.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _corporateeventsCorporateEventReportingService.ScheduleCorporateEventReportAsync);
        var input2 = new ApiFeatureflags.ExportFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ExportFeatureFlagsRequest.Types.Filter
            {
                Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        using var stream2 = _featureflagsFeatureFlagQueryService.ExportFeatureFlags(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportFeatureFlags frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiDocuments.RetryDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var named3 = _registry.Get<ApiDocuments.DocumentExportService.DocumentExportServiceClient>("acme.documents.v1.DocumentExportService");
        using var rpc3 = named3.RetryDocumentExportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.SuspendCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.Document?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.CorporateEvent?.Location?.PostalCode ?? ""),
                },
            },
            Outcome = new ApiCorporateevents.SuspendCorporateEventResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "SuspendCorporateEvent", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.TerminateCorporateEventResponse> TerminateCorporateEvent(ApiCorporateevents.TerminateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "TerminateCorporateEvent", "accepted", token);
        var input0 = new ApiCorporateevents.ListVenueConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.ListVenueConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var descriptor0 = new Method<ApiCorporateevents.ListVenueConflictsRequest, ApiCorporateevents.ListVenueConflictsResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".VenueSyncService", "ListVenueConflicts",
            Wire.Marshaller<ApiCorporateevents.ListVenueConflictsRequest>(), Wire.Marshaller<ApiCorporateevents.ListVenueConflictsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ReconcileDocumentFoldersRequest
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
                            UserId = (request.Details?.NewOwner?.UserId ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.NotifyContact?.Email ?? ""),
                    },
                } },
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderSyncService.ReconcileDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.TerminateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.DocumentFolder?.Location?.CountryCode ?? ""),
                },
            },
            Outcome = new ApiCorporateevents.TerminateCorporateEventResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "TerminateCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiCorporateevents.LintCorporateEventsResponse> SendExpireCorporateEvent2Async(ApiCorporateevents.LintCorporateEventsRequest request, CancellationToken token)
    {
        return await _corporateEventLifecycleEndpointSendExpireCorporateEvent2Port.SendAsync(request, token);
    }
}
