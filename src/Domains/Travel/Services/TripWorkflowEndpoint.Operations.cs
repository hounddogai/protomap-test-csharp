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
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TripWorkflowEndpoint
{
    /// <summary>
    /// Advances a trip.
    /// </summary>
    public override async Task<ApiTravel.AdvanceTripWorkflowResponse> AdvanceTripWorkflow(ApiTravel.AdvanceTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceTripWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTreasury.UpdateCashPositionRequest
        {
            CashPosition = new ApiTreasury.CashPosition
            {
                Card = new ApiTreasury.CashPositionCardFunding
                {
                    Network = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Card => (request.Trip?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _treasuryCashPositionService.UpdateCashPositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.AdvanceTripWorkflowResponse
        {
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.CashPosition?.Holder?.DisplayName ?? ""),
                },
                Number = (request.Trip?.Number ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "AdvanceTripWorkflow", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a trip.
    /// </summary>
    public override async Task<ApiTravel.CancelTripWorkflowResponse> CancelTripWorkflow(ApiTravel.CancelTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelTripWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiChat.EstimateChatRoomRequest
        {
            ChatRoom = new ApiChat.ChatRoom
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiChat.EstimateChatRoomRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Trip?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiChat.EstimateChatRoomRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _chatChatRoomCalculationService.EstimateChatRoomAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.CancelTripWorkflowResponse
        {
            Outcome = new ApiTravel.CancelTripWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Lines = { new ApiTravel.TripLine
                {
                    Quantity = (reply0.ChatRoom?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
                Total = (request.Trip?.Total ?? new ApiCommon.Money()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "CancelTripWorkflow", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a trip.
    /// </summary>
    public override async Task<ApiTravel.InspectTripWorkflowResponse> InspectTripWorkflow(ApiTravel.InspectTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        _logger.LogInformation("Handling InspectTripWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "InspectTripWorkflow", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.InspectTripWorkflowRequest, ApiEmployees.NormalizeEmploymentContractRequest>(value => new ApiEmployees.NormalizeEmploymentContractRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizeEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
                Locale = (value.Context?.Locale ?? ""),
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                IssuedAt = (value.Context?.Deadline ?? new Timestamp()),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _employeesEmploymentContractValidationService.NormalizeEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.InspectTripWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                TripId = (request.TripId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists trips a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListTripWorkflowsResponse> ListTripWorkflows(ApiTravel.ListTripWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListTripWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "ListTripWorkflows", "accepted", token);
        var response = new ApiTravel.ListTripWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ListTripWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a trip.
    /// </summary>
    public override async Task<ApiTravel.PauseTripWorkflowResponse> PauseTripWorkflow(ApiTravel.PauseTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseTripWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.PullItinerariesRequest
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
                    UpdatedAt = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    Network = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Card => (request.Trip?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _travelItinerarySyncService.PullItinerariesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.PauseTripWorkflowResponse
        {
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Itinerary?.BillingAddress?.CountryCode ?? ""),
                },
                Notes = (request.Trip?.Notes ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "PauseTripWorkflow", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a trip.
    /// </summary>
    public override async Task<ApiTravel.ResumeTripWorkflowResponse> ResumeTripWorkflow(ApiTravel.ResumeTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeTripWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ResumeTripWorkflowToScoreNotification0();
        using var rpc0 = _notificationsNotificationValidationService.ScoreNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.CountItineraryTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.CountItineraryTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Notification?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await SendResumeTripWorkflow1Async(input1, token);
        var response = new ApiTravel.ResumeTripWorkflowResponse
        {
            Outcome = new ApiTravel.ResumeTripWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Notification?.Requester?.Email ?? ""),
                },
                Total = (request.Trip?.Total ?? new ApiCommon.Money()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ResumeTripWorkflow", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a trip.
    /// </summary>
    public override async Task<ApiTravel.RetryTripWorkflowResponse> RetryTripWorkflow(ApiTravel.RetryTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryTripWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ReconcileItinerariesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ReconcileItinerariesRequest.Types.Details
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
                    Lines = { (request.Trip?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _travelItinerarySyncService.ReconcileItinerariesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.RetryTripWorkflowResponse
        {
            Outcome = new ApiTravel.RetryTripWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Status = (request.Trip?.Status ?? (ApiTravel.TripStatus)0),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "RetryTripWorkflow", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a trip.
    /// </summary>
    public override async Task<ApiTravel.SignalTripWorkflowResponse> SignalTripWorkflow(ApiTravel.SignalTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalTripWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CancelRegistrationWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Card = new ApiCorporateevents.RegistrationCardFunding
                {
                    HolderName = (request.Trip?.Card?.HolderName ?? ""),
                    Network = (request.Trip?.Card?.Network ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsRegistrationWorkflowService.CancelRegistrationWorkflowAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiTravel.GenerateTravelPolicyReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _travelTravelPolicyReportingService.GenerateTravelPolicyReportAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiHelpdesk.SubmitTicketRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Details = new ApiHelpdesk.SubmitTicketRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                Email = (reply0.Registration?.Contact?.Email ?? ""),
            },
        };
        input2.Ticket = new ApiHelpdesk.Ticket
        {
            Requester = new ApiCommon.ContactInfo
            {
                Phone = (reply0.Registration?.Contact?.Phone ?? ""),
            },
        };
        using var rpc2 = _helpdeskTicketApprovalService.SubmitTicketAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiConcessions.ResolveConcessionStandsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc3 = _concessionsConcessionStandQueryService.ResolveConcessionStandsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.SignalTripWorkflowResponse
        {
            Outcome = new ApiTravel.SignalTripWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Registration?.Contact?.Email ?? ""),
                },
                Notes = (request.Trip?.Notes ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "SignalTripWorkflow", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a trip.
    /// </summary>
    public override async Task<ApiTravel.StartTripWorkflowResponse> StartTripWorkflow(ApiTravel.StartTripWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartTripWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ListTravelPolicyConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.ListTravelPolicyConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTravelPolicySyncService.ListTravelPolicyConflictsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.StartTripWorkflowResponse
        {
            Outcome = new ApiTravel.StartTripWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                IssuedAt = (request.Trip?.IssuedAt ?? new Timestamp()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "StartTripWorkflow", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiTravel.CountItineraryTotalsResponse> SendResumeTripWorkflow1Async(ApiTravel.CountItineraryTotalsRequest request, CancellationToken token)
    {
        return await _tripWorkflowEndpointSendResumeTripWorkflow1Port.SendAsync(request, token);
    }
}
