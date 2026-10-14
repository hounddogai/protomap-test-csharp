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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMerchandising = Acme.Merchandising.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Builds and schedules reports about itineraries.
/// </summary>
public sealed class ItineraryReportingEndpoint : ApiTravel.ItineraryReportingService.ItineraryReportingServiceBase
{
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient _corporateeventsVenueWorkflowService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly IItineraryReportingEndpointSendStreamItineraryMetrics0Port _itineraryReportingEndpointSendStreamItineraryMetrics0Port;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ILogger<ItineraryReportingEndpoint> _logger;
    private readonly ApiMerchandising.CollectionService.CollectionServiceClient _merchandisingCollectionService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient _travelTravelBookingLifecycleService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient _travelTravelPolicyValidationService;
    private readonly ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient _warehousingWarehouseQueryService;

    public ItineraryReportingEndpoint(
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient corporateeventsVenueWorkflowService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        IItineraryReportingEndpointSendStreamItineraryMetrics0Port itineraryReportingEndpointSendStreamItineraryMetrics0PortAdapter,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ILogger<ItineraryReportingEndpoint> logger,
        ApiMerchandising.CollectionService.CollectionServiceClient merchandisingCollectionService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient travelTravelBookingLifecycleService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient travelTravelPolicyValidationService,
        ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient warehousingWarehouseQueryService)
    {
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _corporateeventsVenueWorkflowService = corporateeventsVenueWorkflowService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _itineraryReportingEndpointSendStreamItineraryMetrics0Port = itineraryReportingEndpointSendStreamItineraryMetrics0PortAdapter;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _logger = logger;
        _merchandisingCollectionService = merchandisingCollectionService;
        _repository = repository;
        _settings = settings;
        _travelItinerarySyncService = travelItinerarySyncService;
        _travelTravelBookingLifecycleService = travelTravelBookingLifecycleService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTravelPolicyValidationService = travelTravelPolicyValidationService;
        _warehousingWarehouseQueryService = warehousingWarehouseQueryService;
    }
    /// <summary>
    /// Counts itineraries by status.
    /// </summary>
    public override async Task<ApiTravel.CountItineraryTotalsResponse> CountItineraryTotals(ApiTravel.CountItineraryTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountItineraryTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", "batch",
            "CountItineraryTotals", "accepted", token);
        var input0 = new ApiIdentity.SuggestUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SuggestUsersRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiIdentity.SuggestUsersRequest>(json0);
        using var rpc0 = _identityUserQueryService.SuggestUsersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ResolveDocumentConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentSyncService.ResolveDocumentConflictAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.CountItineraryTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "CountItineraryTotals", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an itinerary.
    /// </summary>
    public override async Task<ApiTravel.DeleteItineraryReportResponse> DeleteItineraryReport(ApiTravel.DeleteItineraryReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteItineraryReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "DeleteItineraryReport", "accepted", token);
        var input0 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.RetryVenueWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.RetryVenueWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsVenueWorkflowService.RetryVenueWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiDocuments.StreamDocumentChangesRequest
        {
            Change = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiDocuments.DocumentBankSettlement
                {
                    Iban = (reply0.EmploymentContract?.Bank?.Iban ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var stream2 = _documentsDocumentSyncService.StreamDocumentChanges(cancellationToken: token);
        async Task Drain2Async()
        {
            await foreach (var change in stream2.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received peer frame {Type}", change.GetType().Name);
            }
        }
        var reader2 = Drain2Async();
        await stream2.RequestStream.WriteAsync(input2, token);
        await stream2.RequestStream.CompleteAsync();
        await reader2;
        var input3 = new ApiTravel.ResolveItineraryConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ResolveItineraryConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    Last4 = (reply0.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Card => (reply0.EmploymentContract?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _travelItinerarySyncService.ResolveItineraryConflictAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.DeleteItineraryReportResponse
        {
            EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ItineraryId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "DeleteItineraryReport", (request.ItineraryId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an itinerary.
    /// </summary>
    public override async Task<ApiTravel.FetchItineraryReportResponse> FetchItineraryReport(ApiTravel.FetchItineraryReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FetchItineraryReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "FetchItineraryReport", "accepted", token);
        var response = new ApiTravel.FetchItineraryReportResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.GenerateItineraryReportResponse> GenerateItineraryReport(ApiTravel.GenerateItineraryReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GenerateItineraryReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "GenerateItineraryReport", "accepted", token);
        var input0 = new ApiCorporateevents.PushVenueRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.PushVenueRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueSyncService.PushVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.PullItinerariesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PullItinerariesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
        };
        using var rpc1 = _travelItinerarySyncService.PullItinerariesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.GenerateItineraryReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "GenerateItineraryReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists itineraries a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListItineraryReportsResponse> ListItineraryReports(ApiTravel.ListItineraryReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListItineraryReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", "batch",
            "ListItineraryReports", "accepted", token);
        var input0 = new ApiTravel.ScheduleTravelBookingExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScheduleTravelBookingExpiryRequest.Types.Details
            {
                Parameters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiTravel.ScheduleTravelBookingExpiryRequest, ApiTravel.ScheduleTravelBookingExpiryResponse>(
            MethodType.Unary, "acme.travel.v1" + ".TravelBookingLifecycleService", "ScheduleTravelBookingExpiry",
            Wire.Marshaller<ApiTravel.ScheduleTravelBookingExpiryRequest>(), Wire.Marshaller<ApiTravel.ScheduleTravelBookingExpiryResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ScoreTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.TravelBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiTravel.ScoreTravelPolicyRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _travelTravelPolicyValidationService.ScoreTravelPolicyAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ListItineraryReportsResponse
        {
            Itineraries = { new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ListItineraryReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ScheduleItineraryReportResponse> ScheduleItineraryReport(ApiTravel.ScheduleItineraryReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleItineraryReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "ScheduleItineraryReport", "accepted", token);
        var input0 = new ApiWarehousing.SuggestWarehousesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWarehousing.SuggestWarehousesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Context?.TenantId ?? ""),
            },
        };
        using var rpc0 = _warehousingWarehouseQueryService.SuggestWarehousesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ListTicketWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ListTicketWorkflowsRequest.Types.Filter
            {
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc1 = _helpdeskTicketWorkflowService.ListTicketWorkflowsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTravel.ScheduleItineraryReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ScheduleItineraryReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ShareItineraryReportResponse> ShareItineraryReport(ApiTravel.ShareItineraryReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ShareItineraryReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "ShareItineraryReport", "accepted", token);
        var input0 = request.ShareItineraryReportToBatchGetCollections0();
        using var rpc0 = _merchandisingCollectionService.BatchGetCollectionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ShareItineraryReportResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ShareItineraryReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of itineraries as they happen.
    /// </summary>
    public override async Task StreamItineraryMetrics(ApiTravel.StreamItineraryMetricsRequest request, IServerStreamWriter<ApiTravel.StreamItineraryMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StreamItineraryMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", "batch",
            "StreamItineraryMetrics", "accepted", token);
        var input0 = new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await SendStreamItineraryMetrics0Async(input0, token);
        var input1 = new ApiTravel.ListTravelPolicyReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.ListTravelPolicyReportsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _travelTravelPolicyReportingService.ListTravelPolicyReportsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ApproveEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiLicensing.ApproveEntitlementRequest>();
        using var rpc2 = _licensingEntitlementApprovalService.ApproveEntitlementAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.StreamItineraryMetricsResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (reply2.Entitlement?.Owner?.DisplayName ?? ""),
                },
                Metadata = { (reply0.EmploymentContract?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.Email ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "StreamItineraryMetrics", (request.Subscriber?.Email ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiEmployees.PreviewEmploymentContractResponse> SendStreamItineraryMetrics0Async(ApiEmployees.PreviewEmploymentContractRequest request, CancellationToken token)
    {
        return await _itineraryReportingEndpointSendStreamItineraryMetrics0Port.SendAsync(request, token);
    }
}
