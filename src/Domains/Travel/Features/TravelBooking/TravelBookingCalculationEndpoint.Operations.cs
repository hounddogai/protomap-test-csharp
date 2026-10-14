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
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelBookingCalculationEndpoint
{
    /// <summary>
    /// Calculates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.CalculateTravelBookingResponse> CalculateTravelBooking(ApiTravel.CalculateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CompareComplianceScanOptionsRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.TravelBooking?.Counterparty?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CompareComplianceScanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CompareComplianceScanOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.CalculateTravelBookingResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                IssuedAt = (request.TravelBooking?.IssuedAt ?? new Timestamp()),
                Lines = { new ApiTravel.TravelBookingLine
                {
                    Attributes = { (reply0.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "CalculateTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.EstimateTravelBookingResponse> EstimateTravelBooking(ApiTravel.EstimateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CompareTripOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CompareTripOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await SendEstimateTravelBooking0Async(input0, token);
        var input1 = new ApiAudit.SuggestAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.SuggestAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Trip?.DueAt ?? new Timestamp()),
                OwnerId = (request.TravelBooking?.Counterparty?.EmployeeId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _auditAuditEventQueryService.SuggestAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.EstimateTravelBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Trip?.BillingAddress?.Locality ?? ""),
                },
                Number = (request.TravelBooking?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "EstimateTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ExplainTravelBookingCalculationResponse> ExplainTravelBookingCalculation(ApiTravel.ExplainTravelBookingCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainTravelBookingCalculation for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.EstimateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.EstimateTripRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.TravelBooking?.BillingAddress?.Recipient ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Bank = new ApiTravel.TripBankSettlement
                {
                    AccountHolder = (request.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (request.TravelBooking?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelTripCalculationService.EstimateTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.SimulateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.TravelBooking?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiTravel.SimulateTripRequest>(json1);
        using var rpc1 = _travelTripCalculationService.SimulateTripAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ExplainTravelBookingCalculationResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply1.Trip?.BillingAddress?.Region ?? ""),
                },
                Metadata = { (request.TravelBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Total = (reply0.Trip?.Total ?? new ApiCommon.Money()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ExplainTravelBookingCalculation", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel booking.
    /// </summary>
    public override async Task<ApiTravel.GetTravelBookingRatesResponse> GetTravelBookingRates(ApiTravel.GetTravelBookingRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        _logger.LogInformation("Handling GetTravelBookingRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "GetTravelBookingRates", "accepted", token);
        var input0 = new ApiDevices.ExplainComplianceScanCalculationRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainComplianceScanCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _devicesComplianceScanCalculationService.ExplainComplianceScanCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelBookingRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ComplianceScan?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a travel booking.
    /// </summary>
    public override async Task<ApiTravel.QuoteTravelBookingResponse> QuoteTravelBooking(ApiTravel.QuoteTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ExplainTripCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (request.TravelBooking?.BillingAddress?.Locality ?? ""),
                    Region = (request.TravelBooking?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _travelTripCalculationService.ExplainTripCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.TravelBooking?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingLine>()).Select(async item =>
        {
            var input1 = new ApiTravel.EstimateTripRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Trip = new ApiTravel.Trip
                {
                    Lines = { new ApiTravel.TripLine
                    {
                        Tax = new ApiTravel.TripLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                        },
                    } },
                },
            };
            using var rpc1 = _travelTripCalculationService.EstimateTripAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiTravel.EstimateTripResponse();
        var reply2 = new ApiHelpdesk.ExplainSlaPolicyCalculationResponse();
        foreach (var item in (request.TravelBooking?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingLine>()))
        {
            var input2 = new ApiHelpdesk.ExplainSlaPolicyCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SlaPolicy = new ApiHelpdesk.SlaPolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.Trip?.Audit?.CreatedBy ?? ""),
                    },
                    Description = (item.Description ?? ""),
                    Discount = new ApiHelpdesk.SlaPolicyDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (reply1.Trip?.Counterparty?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _helpdeskSlaPolicyCalculationService.ExplainSlaPolicyCalculationAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiDevices.CompareComplianceScanOptionsRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CompareComplianceScanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.TravelBooking?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc3 = _devicesComplianceScanCalculationService.CompareComplianceScanOptionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.QuoteTravelBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply3.ComplianceScan?.Subject?.Email ?? ""),
                },
                DueAt = (request.TravelBooking?.DueAt ?? new Timestamp()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "QuoteTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.RecalculateTravelBookingsResponse> RecalculateTravelBookings(ApiTravel.RecalculateTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateTravelBookings for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CalculateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CalculateTripRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.TravelBooking?.BillingAddress?.Recipient ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.TravelBooking?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendRecalculateTravelBookings0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.RecalculateTravelBookingsResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Trip?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = (request.TravelBooking?.BillingAddress ?? new ApiCommon.PostalAddress()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "RecalculateTravelBookings", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.SimulateTravelBookingResponse> SimulateTravelBooking(ApiTravel.SimulateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.RecalculateTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RecalculateTripsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.TravelBooking?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiTravel.TripCalculationService.TripCalculationServiceClient>("acme.travel.v1.TripCalculationService");
        using var rpc0 = named0.RecalculateTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CompareSlaPolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CompareSlaPolicyOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Trip?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.TravelBooking?.BillingAddress?.PostalCode ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiHelpdesk.CompareSlaPolicyOptionsRequest, ApiHelpdesk.CompareSlaPolicyOptionsResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".SlaPolicyCalculationService", "CompareSlaPolicyOptions",
            Wire.Marshaller<ApiHelpdesk.CompareSlaPolicyOptionsRequest>(), Wire.Marshaller<ApiHelpdesk.CompareSlaPolicyOptionsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiHelpdesk.CompareSlaPolicyOptionsRequest>("acme.travel.v1.TravelBookingCalculationService/SimulateTravelBooking/1", input1.Clone()), token);
        var input2 = new ApiFeatureflags.NormalizeFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiFeatureflags.NormalizeFeatureFlagRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _featureflagsFeatureFlagValidationService.NormalizeFeatureFlagAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.SimulateTravelBookingResponse
        {
            Breakdown = new ApiTravel.SimulateTravelBookingResponse.Types.Breakdown
            {
                Lines = { new ApiTravel.SimulateTravelBookingResponse.Types.Breakdown.Types.Line
                {
                    Amount = (reply2.FeatureFlag?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply1.SlaPolicy?.Owner?.Email ?? ""),
                },
                Lines = { new ApiTravel.TravelBookingLine
                {
                    Description = (reply1.SlaPolicy?.Description ?? ""),
                } },
                Status = (request.TravelBooking?.Status ?? (ApiTravel.TravelBookingStatus)0),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "SimulateTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.CompareTripOptionsResponse> SendEstimateTravelBooking0Async(ApiTravel.CompareTripOptionsRequest request, CancellationToken token)
    {
        return await _travelBookingCalculationEndpointSendEstimateTravelBooking0Port.SendAsync(request, token);
    }
}
