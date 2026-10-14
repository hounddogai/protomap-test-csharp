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
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TripLifecycleEndpoint
{
    /// <summary>
    /// Activates a trip.
    /// </summary>
    public override async Task<ApiTravel.ActivateTripResponse> ActivateTrip(ApiTravel.ActivateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling ActivateTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "ActivateTrip", "accepted", token);
        var input0 = new ApiItsm.ApproveChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ApproveChangeRequestRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendActivateTrip0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.CompareTripOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiTravel.TripCalculationService.TripCalculationServiceClient>("acme.travel.v1.TripCalculationService");
        using var rpc1 = named1.CompareTripOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ActivateTripResponse
        {
            Outcome = new ApiTravel.ActivateTripResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = (reply1.Trip?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ActivateTrip", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a trip.
    /// </summary>
    public override async Task<ApiTravel.ExpireTripResponse> ExpireTrip(ApiTravel.ExpireTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling ExpireTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "ExpireTrip", "accepted", token);
        var input0 = new ApiTravel.ValidateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ValidateTripRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiTravel.ValidateTripRequest, ApiTravel.ValidateTripResponse>(
            MethodType.Unary, "acme.travel.v1" + ".TripValidationService", "ValidateTrip",
            Wire.Marshaller<ApiTravel.ValidateTripRequest>(), Wire.Marshaller<ApiTravel.ValidateTripResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDrivers.BatchGetDriversRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command1 = new DispatchCommand<ApiDrivers.BatchGetDriversRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _driversDriverService.BatchGetDriversAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ExpireTripResponse
        {
            Outcome = new ApiTravel.ExpireTripResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ExpireTrip", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a trip.
    /// </summary>
    public override async Task<ApiTravel.GetTripLifecycleResponse> GetTripLifecycle(ApiTravel.GetTripLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling GetTripLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "GetTripLifecycle", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.GetTripLifecycleRequest, ApiEmployees.PreviewEmploymentContractRequest>(value => new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTripLifecycleResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                TripId = (request.TripId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a trip.
    /// </summary>
    public override async Task<ApiTravel.ReinstateTripResponse> ReinstateTrip(ApiTravel.ReinstateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling ReinstateTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "ReinstateTrip", "accepted", token);
        var input0 = new ApiLicensing.ApproveEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ApproveEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingEntitlementApprovalService.ApproveEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ReinstateTripToValidatePosition1(reply0);
        using var rpc1 = _employeesPositionValidationService.ValidatePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.ScoreTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Position?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply1.Position?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Lines = { new ApiTravel.TravelBookingLine
                {
                    Tax = new ApiTravel.TravelBookingLineTax
                    {
                        Amount = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                        {
                            ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply0.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc2 = _travelTravelBookingValidationService.ScoreTravelBookingAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.CheckEmploymentContractEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                        {
                            ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply0.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc3 = _employeesEmploymentContractValidationService.CheckEmploymentContractEligibilityAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.ReinstateTripResponse
        {
            Outcome = new ApiTravel.ReinstateTripResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply3.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                Total = (reply2.TravelBooking?.Total ?? new ApiCommon.Money()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ReinstateTrip", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a trip.
    /// </summary>
    public override async Task<ApiTravel.RenewTripResponse> RenewTrip(ApiTravel.RenewTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling RenewTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "RenewTrip", "accepted", token);
        var input0 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.RecalculateEmploymentContractsRequest>();
        using var rpc0 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.DelegateProblemApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiItsm.DelegateProblemApprovalRequest>(json1);
        using var rpc1 = _itsmProblemApprovalService.DelegateProblemApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.RenewTripResponse
        {
            Outcome = new ApiTravel.RenewTripResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
                Counterparty = (reply0.EmploymentContract?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "RenewTrip", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a trip.
    /// </summary>
    public override async Task<ApiTravel.ScheduleTripExpiryResponse> ScheduleTripExpiry(ApiTravel.ScheduleTripExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleTripExpiry for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ValidateItineraryRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Itinerary = new ApiTravel.Itinerary
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                CountryCode = (request.Trip?.BillingAddress?.CountryCode ?? ""),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _travelItineraryValidationService.ValidateItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.LintItinerariesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Trip?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Trip?.BillingAddress?.PostalCode ?? ""),
                    Region = (request.Trip?.BillingAddress?.Region ?? ""),
                },
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    HolderName = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Card => (request.Trip?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelItineraryValidationService.LintItinerariesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ScheduleTripExpiryResponse
        {
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Itinerary?.Counterparty?.DisplayName ?? ""),
                },
                Number = (reply1.Itinerary?.Counterparty?.Number ?? ""),
                Total = (request.Trip?.Total ?? new ApiCommon.Money()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ScheduleTripExpiry", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a trip.
    /// </summary>
    public override async Task<ApiTravel.SuspendTripResponse> SuspendTrip(ApiTravel.SuspendTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling SuspendTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "SuspendTrip", "accepted", token);
        var input0 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiTravel.RecalculateTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RecalculateTravelPoliciesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _travelTravelPolicyCalculationService.RecalculateTravelPoliciesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.SuspendTripResponse
        {
            Outcome = new ApiTravel.SuspendTripResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "SuspendTrip", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a trip.
    /// </summary>
    public override async Task<ApiTravel.TerminateTripResponse> TerminateTrip(ApiTravel.TerminateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling TerminateTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "TerminateTrip", "accepted", token);
        var input0 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync);
        var input1 = new ApiTravel.EstimateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendTerminateTrip1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.TerminateTripResponse
        {
            Outcome = new ApiTravel.TerminateTripResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply1.Trip?.Counterparty?.EmployeeId ?? ""),
                },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "TerminateTrip", (request.Reason ?? ""), token);
        return response;
    }
}
