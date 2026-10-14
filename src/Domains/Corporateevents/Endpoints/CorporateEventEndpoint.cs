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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Manages corporate events: creates, reads, changes, and lists them.
/// </summary>
public sealed class CorporateEventEndpoint : ApiCorporateevents.CorporateEventService.CorporateEventServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ICorporateEventEndpointSendCreateCorporateEvent0Port _corporateEventEndpointSendCreateCorporateEvent0Port;
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiCorporateevents.VenueValidationService.VenueValidationServiceClient _corporateeventsVenueValidationService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ILogger<CorporateEventEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.CountChangeRequestsRequest, CallOptions, AsyncUnaryCall<ApiItsm.CountChangeRequestsResponse>> _sendWatchCorporateEvents0;

    public CorporateEventEndpoint(
        NotificationBus bus,
        ICorporateEventEndpointSendCreateCorporateEvent0Port corporateEventEndpointSendCreateCorporateEvent0PortAdapter,
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiCorporateevents.VenueValidationService.VenueValidationServiceClient corporateeventsVenueValidationService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ILogger<CorporateEventEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _bus = bus;
        _corporateEventEndpointSendCreateCorporateEvent0Port = corporateEventEndpointSendCreateCorporateEvent0PortAdapter;
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _corporateeventsVenueValidationService = corporateeventsVenueValidationService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _registry = registry;
        _repository = repository;
        _sendWatchCorporateEvents0 = _itsmChangeRequestQueryService.CountChangeRequestsAsync;
    }
    /// <summary>
    /// Takes a stream of corporate events and answers once at the end.
    /// </summary>
    public override async Task<ApiCorporateevents.BatchCreateCorporateEventsResponse> BatchCreateCorporateEvents(IAsyncStreamReader<ApiCorporateevents.BatchCreateCorporateEventsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiCorporateevents.BatchCreateCorporateEventsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling BatchCreateCorporateEvents for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiLicensing.ExplainEntitlementCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLicensing.ExplainEntitlementCalculationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = request.ChunkCase switch
                        {
                            ApiCorporateevents.BatchCreateCorporateEventsRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Name ?? new ApiCommon.PersonName()),
                            _ => new ApiCommon.PersonName()
                        },
                        Phone = request.ChunkCase switch
                        {
                            ApiCorporateevents.BatchCreateCorporateEventsRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Phone ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _licensingEntitlementCalculationService.ExplainEntitlementCalculationAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var reply1 = new ApiDevices.EstimateDevicePolicyResponse();
            foreach (var item in (request.Record?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
            {
                var input1 = new ApiDevices.EstimateDevicePolicyRequest
                {
                    Context = (request.Context ?? new ApiCommon.RequestContext()),
                    Details = new ApiDevices.EstimateDevicePolicyRequest.Types.Details
                    {
                        Destination = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.Record?.Location?.Recipient ?? ""),
                        },
                    },
                    DevicePolicy = new ApiDevices.DevicePolicy
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.Record?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                        Owner = new ApiIdentity.UserRef
                        {
                            Email = (item.Contact?.Email ?? ""),
                        },
                    },
                };
                using var rpc1 = _devicesDevicePolicyCalculationService.EstimateDevicePolicyAsync(input1, cancellationToken: token);
                reply1 = await rpc1.ResponseAsync;
            }
            var response = new ApiCorporateevents.BatchCreateCorporateEventsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                    ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.Title ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
                "audit", "BatchCreateCorporateEvents", (request.Record?.Title ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Creates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.CloneCorporateEventResponse> CloneCorporateEvent(ApiCorporateevents.CloneCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.EstimateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EstimateLicenseAssignmentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.CorporateEvent?.Location?.Region ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.CorporateEvent?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingLicenseAssignmentCalculationService.EstimateLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiLicensing.CloneSoftwareLicenseResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()))
        {
            var input1 = new ApiLicensing.CloneSoftwareLicenseRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
            };
            using var rpc1 = _licensingSoftwareLicenseService.CloneSoftwareLicenseAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiCorporateevents.CloneCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.LicenseAssignment?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Organizer = (request.CorporateEvent?.Organizer ?? new ApiIdentity.UserRef()),
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CloneCorporateEvent", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.CreateCorporateEventResponse> CreateCorporateEvent(ApiCorporateevents.CreateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CountRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.CountRegistrationsRequest.Types.Filter
            {
                CreatedAfter = (request.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.CorporateEvent?.Organizer?.UserId ?? ""),
                Region = (request.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var reply0 = await SendCreateCorporateEvent0Async(input0, token);
        var input1 = new ApiCorporateevents.SummarizeCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.CorporateEvent?.CorporateEventId ?? ""),
            Details = new ApiCorporateevents.SummarizeCorporateEventsRequest.Types.Details
            {
                Filters = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _corporateeventsCorporateEventQueryService.SummarizeCorporateEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CreateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Status = (request.CorporateEvent?.Status ?? (ApiCorporateevents.CorporateEventStatus)0),
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CreateCorporateEvent", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.GetCorporateEventResponse> GetCorporateEvent(ApiCorporateevents.GetCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "GetCorporateEvent", "accepted", token);
        var input0 = new ApiCorporateevents.CheckCorporateEventEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
            Details = new ApiCorporateevents.CheckCorporateEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.CheckCorporateEventEligibilityRequest>();
        using var rpc0 = _corporateeventsCorporateEventValidationService.CheckCorporateEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCorporateevents.CheckCorporateEventEligibilityRequest>("acme.corporateevents.v1.CorporateEventService/GetCorporateEvent/0", input0.Clone()), token);
        var response = new ApiCorporateevents.GetCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists corporate events a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListCorporateEventsResponse> ListCorporateEvents(ApiCorporateevents.ListCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "ListCorporateEvents", "accepted", token);
        var input0 = new ApiCorporateevents.FindCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.FindCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var rpc0 = _corporateeventsCorporateEventQueryService.FindCorporateEventsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.CalculateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CalculateDevicePolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyCalculationService.CalculateDevicePolicyAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiCorporateevents.ListCorporateEventsResponse
        {
            CorporateEvents = { new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ListCorporateEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists corporate events a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.SearchCorporateEventsResponse> SearchCorporateEvents(ApiCorporateevents.SearchCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SearchCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "SearchCorporateEvents", "accepted", token);
        var response = new ApiCorporateevents.SearchCorporateEventsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "SearchCorporateEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.UpdateCorporateEventResponse> UpdateCorporateEvent(ApiCorporateevents.UpdateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.CreateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.CorporateEvent?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.CorporateEvent?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _featureflagsFeatureFlagService.CreateFeatureFlagAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiMetrics.LintMetricSeriesesResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiMetrics.LintMetricSeriesesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiMetrics.LintMetricSeriesesRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
            };
            using var rpc1 = _metricsMetricSeriesValidationService.LintMetricSeriesesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiCorporateevents.LintVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply1.MetricSeries?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _corporateeventsVenueValidationService.LintVenuesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.RevertDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentTemplateRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.CorporateEvent?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply1.MetricSeries?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    Recipient = (reply2.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply2.Venue?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                    Region = (reply2.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply2.Venue?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _documentsDocumentTemplateHistoryService.RevertDocumentTemplateAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.UpdateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEvent?.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.Venue?.Address?.CountryCode ?? ""),
                },
                Organizer = (request.CorporateEvent?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.CorporateEvent?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "UpdateCorporateEvent", (request.CorporateEvent?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of corporate events as they happen.
    /// </summary>
    public override async Task WatchCorporateEvents(ApiCorporateevents.WatchCorporateEventsRequest request, IServerStreamWriter<ApiCorporateevents.WatchCorporateEventsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "WatchCorporateEvents", "accepted", token);
        var input0 = new ApiItsm.CountChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.CountChangeRequestsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _sendWatchCorporateEvents0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ExplainTicketDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiHelpdesk.TicketValidationService.TicketValidationServiceClient>("acme.helpdesk.v1.TicketValidationService");
        using var rpc1 = named1.ExplainTicketDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.WatchCorporateEventsResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Ticket?.Audit?.CreatedBy ?? ""),
                    Etag = (reply1.Ticket?.Audit?.Etag ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.Phone ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "WatchCorporateEvents", (request.Subscriber?.Phone ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiCorporateevents.CountRegistrationsResponse> SendCreateCorporateEvent0Async(ApiCorporateevents.CountRegistrationsRequest request, CancellationToken token)
    {
        return await _corporateEventEndpointSendCreateCorporateEvent0Port.SendAsync(request, token);
    }
}
