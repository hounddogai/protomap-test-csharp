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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TripApprovalEndpoint
{
    /// <summary>
    /// Approves a trip.
    /// </summary>
    public override async Task<ApiTravel.ApproveTripResponse> ApproveTrip(ApiTravel.ApproveTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.PushItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PushItineraryRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Trip?.BillingAddress?.Locality ?? ""),
                },
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    Last4 = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Card => (request.Trip?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelItinerarySyncService.PushItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Bank = new ApiLicensing.LicenseAssignmentBankSettlement
                {
                    AccountHolder = (reply0.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (reply0.Itinerary?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                    Bic = (reply0.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (reply0.Itinerary?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentWorkflowService.AdvanceLicenseAssignmentWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiTravel.SearchTripsResponse();
        foreach (var item in (request.Trip?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TripLine>()))
        {
            var input2 = new ApiTravel.SearchTripsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiTravel.SearchTripsRequest.Types.Filter
                {
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Region = (request.Trip?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc2 = _travelTripService.SearchTripsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiTravel.ApproveTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.LicenseAssignment?.BillingAddress?.PostalCode ?? ""),
                },
                Counterparty = (request.Trip?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ApproveTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a trip.
    /// </summary>
    public override async Task<ApiTravel.DelegateTripApprovalResponse> DelegateTripApproval(ApiTravel.DelegateTripApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateTripApproval for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Trip?.TripId ?? ""),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesEmployeeService.ListEmployeesAsync);
        var input1 = new ApiTravel.QuoteTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.QuoteTravelPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Trip?.BillingAddress?.Recipient ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _sendDelegateTripApproval1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.DelegateTripApprovalResponse
        {
            Outcome = new ApiTravel.DelegateTripApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "DelegateTripApproval", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a trip.
    /// </summary>
    public override async Task<ApiTravel.EscalateTripResponse> EscalateTrip(ApiTravel.EscalateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ShareTravelPolicyReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ShareTravelPolicyReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _travelTravelPolicyReportingService.ShareTravelPolicyReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Trip?.BillingAddress?.Locality ?? ""),
                    PostalCode = (request.Trip?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiEmployees.CreateEmployeeRequest, ApiEmployees.CreateEmployeeResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeService", "CreateEmployee",
            Wire.Marshaller<ApiEmployees.CreateEmployeeRequest>(), Wire.Marshaller<ApiEmployees.CreateEmployeeResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.Trip?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TripLine>()).Select(async item =>
        {
            var input2 = new ApiDevices.RetryDevicePolicyWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DevicePolicy = new ApiDevices.DevicePolicy
                {
                    Discount = new ApiDevices.DevicePolicyDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc2 = _devicesDevicePolicyWorkflowService.RetryDevicePolicyWorkflowAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiDevices.RetryDevicePolicyWorkflowResponse();
        var input3 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CompareEmploymentContractOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Employee?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (reply0.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (reply0.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc3 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.EscalateTripResponse
        {
            Trip = new ApiTravel.Trip
            {
                BillingAddress = (request.Trip?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Employee?.RelationshipManager?.DisplayName ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Description = (reply0.TravelPolicy?.Description ?? ""),
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (reply0.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
                Number = (reply3.EmploymentContract?.Number ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "EscalateTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a trip.
    /// </summary>
    public override async Task<ApiTravel.GetTripApprovalStatusResponse> GetTripApprovalStatus(ApiTravel.GetTripApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling GetTripApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "GetTripApprovalStatus", "accepted", token);
        var input0 = new ApiItsm.DelegateProblemApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.DelegateProblemApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Problem = new ApiItsm.Problem
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmProblemApprovalService.DelegateProblemApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTripApprovalStatusResponse
        {
            Trip = new ApiTravel.Trip
            {
                DueAt = (reply0.Problem?.Sla?.DueAt ?? new Timestamp()),
                TripId = (request.TripId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists trips a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListPendingTripsResponse> ListPendingTrips(ApiTravel.ListPendingTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "ListPendingTrips", "accepted", token);
        var input0 = request.ListPendingTripsToSummarizeEmployees0();
        using var rpc0 = _employeesEmployeeQueryService.SummarizeEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ListPendingTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ListPendingTrips", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a trip.
    /// </summary>
    public override async Task<ApiTravel.RecallTripResponse> RecallTrip(ApiTravel.RecallTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SuggestEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
            {
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await SendRecallTrip0Async(input0, token);
        var response = new ApiTravel.RecallTripResponse
        {
            Outcome = new ApiTravel.RecallTripResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "RecallTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a trip.
    /// </summary>
    public override async Task<ApiTravel.SubmitTripResponse> SubmitTrip(ApiTravel.SubmitTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CountTravelBookingTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.CountTravelBookingTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _travelTravelBookingReportingService.CountTravelBookingTotalsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformers.CountPerformersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformers.CountPerformersRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Trip?.TripId ?? ""),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiPerformers.CountPerformersRequest>();
        using var rpc1 = _performersPerformerQueryService.CountPerformersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Trip?.BillingAddress?.CountryCode ?? ""),
                },
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    Last4 = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Card => (request.Trip?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.SubmitTripResponse
        {
            Outcome = new ApiTravel.SubmitTripResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                },
                Metadata = { (request.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "SubmitTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.SuggestEmployeesResponse> SendRecallTrip0Async(ApiEmployees.SuggestEmployeesRequest request, CancellationToken token)
    {
        return await _tripApprovalEndpointSendRecallTrip0Port.SendAsync(request, token);
    }
}
