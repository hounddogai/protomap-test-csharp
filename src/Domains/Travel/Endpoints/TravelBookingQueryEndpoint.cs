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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Answers read-only questions about travel bookings, for screens and reports.
/// </summary>
public sealed class TravelBookingQueryEndpoint : ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceBase
{
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ILogger<TravelBookingQueryEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly IRecordRepository _repository;
    private readonly ITravelBookingQueryEndpointSendExportTravelBookings0Port _travelBookingQueryEndpointSendExportTravelBookings0Port;
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _travelItineraryCalculationService;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public TravelBookingQueryEndpoint(
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ILogger<TravelBookingQueryEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        IRecordRepository repository,
        ITravelBookingQueryEndpointSendExportTravelBookings0Port travelBookingQueryEndpointSendExportTravelBookings0PortAdapter,
        ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient travelItineraryCalculationService,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _repository = repository;
        _travelBookingQueryEndpointSendExportTravelBookings0Port = travelBookingQueryEndpointSendExportTravelBookings0PortAdapter;
        _travelItineraryCalculationService = travelItineraryCalculationService;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTripCalculationService = travelTripCalculationService;
    }
    /// <summary>
    /// Lists travel bookings a page at a time.
    /// </summary>
    public override async Task<ApiTravel.BrowseTravelBookingsResponse> BrowseTravelBookings(ApiTravel.BrowseTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "BrowseTravelBookings", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.BrowseTravelBookingsRequest, ApiTravel.RecalculateTripsRequest>(value => new ApiTravel.RecalculateTripsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _travelTripCalculationService.RecalculateTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.BrowseTravelBookingsResponse
        {
            TravelBookings = { new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply0.Trip?.Counterparty?.EmployeeId ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "BrowseTravelBookings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts travel bookings by status.
    /// </summary>
    public override async Task<ApiTravel.CountTravelBookingsResponse> CountTravelBookings(ApiTravel.CountTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "CountTravelBookings", "accepted", token);
        var input0 = new ApiMetrics.PreviewMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.PreviewMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _metricsMetricSeriesValidationService.PreviewMetricSeriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply0.MetricSeries?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    PostalCode = (reply0.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply0.MetricSeries?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                    Region = (reply0.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply0.MetricSeries?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateSyncService.SyncDocumentTemplatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.CountTravelBookingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "CountTravelBookings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching travel booking.
    /// </summary>
    public override async Task ExportTravelBookings(ApiTravel.ExportTravelBookingsRequest request, IServerStreamWriter<ApiTravel.ExportTravelBookingsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "ExportTravelBookings", "accepted", token);
        var input0 = new ApiCorporateevents.EstimateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.EstimateRegistrationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendExportTravelBookings0Async(input0, token);
        var response = new ApiTravel.ExportTravelBookingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ExportTravelBookings", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists travel bookings a page at a time.
    /// </summary>
    public override async Task<ApiTravel.FindTravelBookingsResponse> FindTravelBookings(ApiTravel.FindTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "FindTravelBookings", "accepted", token);
        var response = new ApiTravel.FindTravelBookingsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "FindTravelBookings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel booking.
    /// </summary>
    public override async Task<ApiTravel.LookupTravelBookingResponse> LookupTravelBooking(ApiTravel.LookupTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "LookupTravelBooking", "accepted", token);
        var input0 = new ApiMetrics.SuggestMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.SuggestMetricSeriesesRequest.Types.Filter
            {
                OwnerId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _metricsMetricSeriesQueryService.SuggestMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.LookupTravelBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a travel booking.
    /// </summary>
    public override async Task<ApiTravel.PeekTravelBookingResponse> PeekTravelBooking(ApiTravel.PeekTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekTravelBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "PeekTravelBooking", "accepted", token);
        var input0 = new ApiTravel.NormalizeTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.NormalizeTravelBookingRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTravel.NormalizeTravelBookingRequest>();
        using var rpc0 = _travelTravelBookingValidationService.NormalizeTravelBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.PeekTravelBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several travel bookings at once.
    /// </summary>
    public override async Task<ApiTravel.ResolveTravelBookingsResponse> ResolveTravelBookings(ApiTravel.ResolveTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "ResolveTravelBookings", "accepted", token);
        var input0 = new ApiTravel.ExplainItineraryCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainItineraryCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.ExplainItineraryCalculationRequest>(json0);
        using var rpc0 = _travelItineraryCalculationService.ExplainItineraryCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.SummarizeRegistrationsRequest();
        input1.Annotations.Add((reply0.Itinerary?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiCorporateevents.SummarizeRegistrationsRequest.Types.Details
        {
            Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        using var rpc1 = _corporateeventsRegistrationQueryService.SummarizeRegistrationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ResolveTravelBookingsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
            TravelBookings = { new ApiTravel.TravelBooking
            {
                Notes = (reply0.Itinerary?.Notes ?? ""),
            } },
        };
        return response;
    }
    /// <summary>
    /// Lists travel bookings a page at a time.
    /// </summary>
    public override async Task<ApiTravel.SuggestTravelBookingsResponse> SuggestTravelBookings(ApiTravel.SuggestTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "SuggestTravelBookings", "accepted", token);
        var response = new ApiTravel.SuggestTravelBookingsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "SuggestTravelBookings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a travel booking.
    /// </summary>
    public override async Task<ApiTravel.SummarizeTravelBookingsResponse> SummarizeTravelBookings(ApiTravel.SummarizeTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "SummarizeTravelBookings", "accepted", token);
        var input0 = new ApiTravel.QuoteTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Block = new ApiTravel.TravelPolicyBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _travelTravelPolicyCalculationService.QuoteTravelPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainTravelPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Block = new ApiTravel.TravelPolicyBlockAction
                {
                    NotifyEmail = (reply0.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Block => (reply0.TravelPolicy?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                    Reason = (reply0.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Block => (reply0.TravelPolicy?.Block?.Reason ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelTravelPolicyCalculationService.ExplainTravelPolicyCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.SimulateItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (reply0.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiTravel.ItineraryLine
                {
                    Tax = new ApiTravel.ItineraryLineTax
                    {
                        Amount = (reply1.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (reply1.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc2 = _travelItineraryCalculationService.SimulateItineraryAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.ExportDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ExportDevicePoliciesRequest.Types.Filter
            {
                CreatedAfter = (reply0.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply1.TravelPolicy?.Owner?.UserId ?? ""),
                Region = (reply2.Itinerary?.BillingAddress?.Region ?? ""),
            },
            Format = (request.Details?.Format ?? ""),
        };
        using var stream3 = _devicesDevicePolicyQueryService.ExportDevicePolicies(input3, cancellationToken: token);
        await foreach (var change3 in stream3.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDevicePolicies frame {Type}", change3.GetType().Name);
        }
        var response = new ApiTravel.SummarizeTravelBookingsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "SummarizeTravelBookings", (request.TravelBookingId ?? ""), token);
        return response;
    }
    private async Task<ApiCorporateevents.EstimateRegistrationResponse> SendExportTravelBookings0Async(ApiCorporateevents.EstimateRegistrationRequest request, CancellationToken token)
    {
        return await _travelBookingQueryEndpointSendExportTravelBookings0Port.SendAsync(request, token);
    }
}
