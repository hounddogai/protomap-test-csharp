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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTours = Acme.Tours.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class ItineraryCalculationEndpoint
{
    /// <summary>
    /// Calculates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.CalculateItineraryResponse> CalculateItinerary(ApiTravel.CalculateItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.LintTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Bank = new ApiTravel.TripBankSettlement
                {
                    Bic = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (request.Itinerary?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelTripValidationService.LintTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.ValidateMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _metricsMetricSeriesValidationService.ValidateMetricSeriesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiCorporateevents.SummarizeCorporateEventsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsCorporateEventQueryService.SummarizeCorporateEventsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.CalculateItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = (request.Itinerary?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (reply0.Trip?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Lines = { new ApiTravel.ItineraryLine
                {
                    Quantity = (reply1.MetricSeries?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Itinerary?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "CalculateItinerary", (request.Itinerary?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an itinerary.
    /// </summary>
    public override async Task<ApiTravel.CompareItineraryOptionsResponse> CompareItineraryOptions(ApiTravel.CompareItineraryOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareItineraryOptions for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Itinerary?.IssuedAt ?? new Timestamp()),
                Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _documentsDocumentNotificationService.ListDocumentSubscriptionsAsync);
        var input1 = new ApiTravel.GetTravelBookingRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBookingId = (request.Itinerary?.Counterparty?.TravelBookingId ?? ""),
        };
        using var rpc1 = _sendCompareItineraryOptions1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.SimulateTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SimulateTravelPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.TravelBooking?.BillingAddress?.Locality ?? ""),
                    PostalCode = (request.Itinerary?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (request.Itinerary?.BillingAddress?.Recipient ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient>("acme.travel.v1.TravelPolicyCalculationService");
        using var rpc2 = named2.SimulateTravelPolicyAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.CompareItineraryOptionsResponse
        {
            Breakdown = new ApiTravel.CompareItineraryOptionsResponse.Types.Breakdown
            {
                Total = (reply1.TravelBooking?.Total ?? new ApiCommon.Money()),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply2.TravelPolicy?.Audit?.CreatedBy ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Notes = (request.Itinerary?.Notes ?? ""),
                Number = (reply1.TravelBooking?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "CompareItineraryOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.EstimateItineraryResponse> EstimateItinerary(ApiTravel.EstimateItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.LintIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Itinerary?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Itinerary?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
                Sla = new ApiItsm.IncidentSla
                {
                    DueAt = (request.Itinerary?.DueAt ?? new Timestamp()),
                },
            },
        };
        var descriptor0 = new Method<ApiItsm.LintIncidentsRequest, ApiItsm.LintIncidentsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".IncidentValidationService", "LintIncidents",
            Wire.Marshaller<ApiItsm.LintIncidentsRequest>(), Wire.Marshaller<ApiItsm.LintIncidentsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.RecalculateVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var command1 = new DispatchCommand<ApiCorporateevents.RecalculateVenuesRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _corporateeventsVenueCalculationService.RecalculateVenuesAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.ExplainTravelPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply0.Incident?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _travelTravelPolicyCalculationService.ExplainTravelPolicyCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.EstimateItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                IssuedAt = (request.Itinerary?.IssuedAt ?? new Timestamp()),
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "EstimateItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ExplainItineraryCalculationResponse> ExplainItineraryCalculation(ApiTravel.ExplainItineraryCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainItineraryCalculation for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.SuggestLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.SuggestLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.Itinerary?.DueAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Itinerary?.Counterparty?.TravelBookingId ?? ""),
                Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _ledgerLedgerAccountQueryService.SuggestLedgerAccountsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ExplainItineraryCalculationToCountAuditEvents1(reply0);
        using var rpc1 = _auditAuditEventQueryService.CountAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ExplainItineraryCalculationResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                DueAt = (request.Itinerary?.DueAt ?? new Timestamp()),
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.ExplainItineraryCalculationResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ExplainItineraryCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an itinerary.
    /// </summary>
    public override async Task<ApiTravel.GetItineraryRatesResponse> GetItineraryRates(ApiTravel.GetItineraryRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling GetItineraryRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "GetItineraryRates", "accepted", token);
        var input0 = new ApiTravel.ValidateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ValidateTravelBookingRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendGetItineraryRates0Async(input0, token);
        var input1 = new ApiTours.SummarizeToursRequest
        {
            Annotations = { (reply0.TravelBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTours.SummarizeToursRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _toursTourQueryService.SummarizeToursAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.GetItineraryRatesResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (reply0.TravelBooking?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an itinerary.
    /// </summary>
    public override async Task<ApiTravel.QuoteItineraryResponse> QuoteItinerary(ApiTravel.QuoteItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SuggestDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.SuggestDevicePoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Itinerary?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Itinerary?.ItineraryId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDevices.SuggestDevicePoliciesRequest>();
        using var rpc0 = _devicesDevicePolicyQueryService.SuggestDevicePoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.RecalculateTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RecalculateTravelPoliciesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiTravel.RecalculateTravelPoliciesRequest>(json1);
        using var rpc1 = _travelTravelPolicyCalculationService.RecalculateTravelPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.QuoteItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Metadata = { (request.Itinerary?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "QuoteItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.RecalculateItinerariesResponse> RecalculateItineraries(ApiTravel.RecalculateItinerariesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateItineraries for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SuggestDevicePoliciesRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiDevices.SuggestDevicePoliciesRequest.Types.Filter
        {
            CreatedAfter = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
            Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (request.Itinerary?.ItineraryId ?? ""),
            Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
        };
        using var rpc0 = _devicesDevicePolicyQueryService.SuggestDevicePoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.CheckTravelBookingEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Itinerary?.Audit?.Etag ?? ""),
                },
                Bank = new ApiTravel.TravelBookingBankSettlement
                {
                    AccountHolder = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (request.Itinerary?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _travelTravelBookingValidationService.CheckTravelBookingEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiTravel.CheckTravelBookingEligibilityRequest>("acme.travel.v1.ItineraryCalculationService/RecalculateItineraries/1", input1.Clone()), token);
        var response = new ApiTravel.RecalculateItinerariesResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                DueAt = (request.Itinerary?.DueAt ?? new Timestamp()),
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "RecalculateItineraries", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.SimulateItineraryResponse> SimulateItinerary(ApiTravel.SimulateItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SimulateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Itinerary?.BillingAddress?.CountryCode ?? ""),
                },
                Card = new ApiTravel.TripCardSettlement
                {
                    HolderName = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (request.Itinerary?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (request.Itinerary?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelTripCalculationService.SimulateTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.CompareTravelPolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CompareTravelPolicyOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (request.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _travelTravelPolicyCalculationService.CompareTravelPolicyOptionsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiIdentity.ExplainUserDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _identityUserValidationService.ExplainUserDecisionAsync);
        var response = new ApiTravel.SimulateItineraryResponse
        {
            Breakdown = new ApiTravel.SimulateItineraryResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
                Lines = { new ApiTravel.SimulateItineraryResponse.Types.Breakdown.Types.Line
                {
                    Amount = (reply1.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply2.User?.PrimaryAddress?.Locality ?? ""),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (reply2.User?.NationalId?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Total = (request.Itinerary?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "SimulateItinerary", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.ValidateTravelBookingResponse> SendGetItineraryRates0Async(ApiTravel.ValidateTravelBookingRequest request, CancellationToken token)
    {
        return await _itineraryCalculationEndpointSendGetItineraryRates0Port.SendAsync(request, token);
    }
}
