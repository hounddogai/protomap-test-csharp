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
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TripQueryEndpoint
{
    /// <summary>
    /// Lists trips a page at a time.
    /// </summary>
    public override async Task<ApiTravel.BrowseTripsResponse> BrowseTrips(ApiTravel.BrowseTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "BrowseTrips", "accepted", token);
        var input0 = new ApiHelpdesk.LintTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiHelpdesk.TicketValidationService.TicketValidationServiceClient>("acme.helpdesk.v1.TicketValidationService");
        using var rpc0 = named0.LintTicketsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.BrowseTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "BrowseTrips", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts trips by status.
    /// </summary>
    public override async Task<ApiTravel.CountTripsResponse> CountTrips(ApiTravel.CountTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "CountTrips", "accepted", token);
        var input0 = new ApiTravel.CompareTravelPolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CompareTravelPolicyOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiTravel.CompareTravelPolicyOptionsRequest, ApiTravel.CompareTravelPolicyOptionsResponse>(
            MethodType.Unary, "acme.travel.v1" + ".TravelPolicyCalculationService", "CompareTravelPolicyOptions",
            Wire.Marshaller<ApiTravel.CompareTravelPolicyOptionsRequest>(), Wire.Marshaller<ApiTravel.CompareTravelPolicyOptionsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.CountTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "CountTrips", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching trip.
    /// </summary>
    public override async Task ExportTrips(ApiTravel.ExportTripsRequest request, IServerStreamWriter<ApiTravel.ExportTripsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "ExportTrips", "accepted", token);
        var input0 = new ApiTravel.ValidateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
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
        var command0 = new DispatchCommand<ApiTravel.ValidateTravelBookingRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _travelTravelBookingValidationService.ValidateTravelBookingAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ExportTripsResponse
        {
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.TravelBooking?.BillingAddress?.Recipient ?? ""),
                    Region = (request.Filter?.Region ?? ""),
                },
                Notes = (reply0.TravelBooking?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ExportTrips", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists trips a page at a time.
    /// </summary>
    public override async Task<ApiTravel.FindTripsResponse> FindTrips(ApiTravel.FindTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "FindTrips", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.FindTripsRequest, ApiTravel.PreviewTripRequest>(value => new ApiTravel.PreviewTripRequest
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
        using var rpc0 = _travelTripValidationService.PreviewTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.FindTripsResponse
        {
            Trips = { new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Trip?.Counterparty?.Email ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "FindTrips", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a trip.
    /// </summary>
    public override async Task<ApiTravel.LookupTripResponse> LookupTrip(ApiTravel.LookupTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling LookupTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "LookupTrip", "accepted", token);
        var input0 = new ApiDevices.ValidateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ValidateManagedDeviceRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _devicesManagedDeviceValidationService.ValidateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.LookupTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                TripId = (request.TripId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a trip.
    /// </summary>
    public override async Task<ApiTravel.PeekTripResponse> PeekTrip(ApiTravel.PeekTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling PeekTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "PeekTrip", "accepted", token);
        var input0 = request.PeekTripToCheckTravelBookingEligibility0();
        using var rpc0 = _travelTravelBookingValidationService.CheckTravelBookingEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.PeekTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                TripId = (request.TripId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several trips at once.
    /// </summary>
    public override async Task<ApiTravel.ResolveTripsResponse> ResolveTrips(ApiTravel.ResolveTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "ResolveTrips", "accepted", token);
        var input0 = new ApiLedger.ListLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.ListLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.Context?.TraceId ?? ""),
            },
        };
        var reply0 = await SendResolveTrips0Async(input0, token);
        var response = new ApiTravel.ResolveTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists trips a page at a time.
    /// </summary>
    public override async Task<ApiTravel.SuggestTripsResponse> SuggestTrips(ApiTravel.SuggestTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "SuggestTrips", "accepted", token);
        var input0 = new ApiLedger.ScoreLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.ScoreLedgerAccountRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OpenedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _ledgerLedgerAccountValidationService.ScoreLedgerAccountAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.NormalizeTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.NormalizeTravelPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.LedgerAccount?.Contact?.Phone ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.LedgerAccount?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiTravel.NormalizeTravelPolicyRequest>();
        using var rpc1 = _travelTravelPolicyValidationService.NormalizeTravelPolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.SuggestTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Trips = { new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.LedgerAccount?.Contact?.Email ?? ""),
                },
                Number = (reply0.LedgerAccount?.Holder?.Number ?? ""),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "SuggestTrips", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a trip.
    /// </summary>
    public override async Task<ApiTravel.SummarizeTripsResponse> SummarizeTrips(ApiTravel.SummarizeTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "SummarizeTrips", "accepted", token);
        var input0 = new ApiTravel.RecalculateTravelBookingsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.RecalculateTravelBookingsRequest>(json0);
        using var rpc0 = _travelTravelBookingCalculationService.RecalculateTravelBookingsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainItineraryDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Bank = new ApiTravel.ItineraryBankSettlement
                {
                    Bic = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (reply0.TravelBooking?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelItineraryValidationService.ExplainItineraryDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.CalculateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Itinerary?.Counterparty?.DisplayName ?? ""),
                    EmployeeId = (reply0.TravelBooking?.Counterparty?.EmployeeId ?? ""),
                },
            },
        };
        using var rpc2 = _travelTravelBookingCalculationService.CalculateTravelBookingAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.NormalizeItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                    Region = (reply0.TravelBooking?.BillingAddress?.Region ?? ""),
                },
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    HolderName = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Card => (reply0.TravelBooking?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (reply2.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Card => (reply2.TravelBooking?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _travelItineraryValidationService.NormalizeItineraryAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.SummarizeTripsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TripId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "SummarizeTrips", (request.TripId ?? ""), token);
        return response;
    }
    private async Task<ApiLedger.ListLedgerAccountsResponse> SendResolveTrips0Async(ApiLedger.ListLedgerAccountsRequest request, CancellationToken token)
    {
        return await _tripQueryEndpointSendResolveTrips0Port.SendAsync(request, token);
    }
}
