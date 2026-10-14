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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Travel;

public sealed partial class ItineraryLifecycleEndpoint
{
    /// <summary>
    /// Activates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ActivateItineraryResponse> ActivateItinerary(ApiTravel.ActivateItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling ActivateItinerary for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "ActivateItinerary", "accepted", token);
        var input0 = new ApiLicensing.SubmitLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SubmitLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendActivateItinerary0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ActivateItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.ActivateItineraryResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ActivateItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ExpireItineraryResponse> ExpireItinerary(ApiTravel.ExpireItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling ExpireItinerary for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "ExpireItinerary", "accepted", token);
        var input0 = new ApiTravel.DeleteItineraryReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ItineraryId = (request.ItineraryId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient>("acme.travel.v1.ItineraryReportingService");
        using var rpc0 = named0.DeleteItineraryReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.ValidateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiIdentity.ValidateUserRequest, ApiIdentity.ValidateUserResponse>(
            MethodType.Unary, "acme.identity.v1" + ".UserValidationService", "ValidateUser",
            Wire.Marshaller<ApiIdentity.ValidateUserRequest>(), Wire.Marshaller<ApiIdentity.ValidateUserResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ExpireItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
                Number = (reply1.User?.NationalId?.Number ?? ""),
            },
            Outcome = new ApiTravel.ExpireItineraryResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ExpireItinerary", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an itinerary.
    /// </summary>
    public override async Task<ApiTravel.GetItineraryLifecycleResponse> GetItineraryLifecycle(ApiTravel.GetItineraryLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling GetItineraryLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "GetItineraryLifecycle", "accepted", token);
        var input0 = new ApiVenues.SummarizeVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiVenues.SummarizeVenuesRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var command0 = new DispatchCommand<ApiVenues.SummarizeVenuesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _venuesVenueQueryService.SummarizeVenuesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetItineraryLifecycleResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Renews an itinerary.
    /// </summary>
    public override async Task<ApiTravel.RenewItineraryResponse> RenewItinerary(ApiTravel.RenewItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling RenewItinerary for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "RenewItinerary", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.RenewItineraryRequest, ApiHelpdesk.CommentKnowledgeArticleApprovalRequest>(value => new ApiHelpdesk.CommentKnowledgeArticleApprovalRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CommentKnowledgeArticleApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (value.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskKnowledgeArticleApprovalService.CommentKnowledgeArticleApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.RenewItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.RenewItineraryResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "RenewItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ScheduleItineraryExpiryResponse> ScheduleItineraryExpiry(ApiTravel.ScheduleItineraryExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleItineraryExpiry for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.ValidateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.ValidateFeatureFlagRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _featureflagsFeatureFlagValidationService.ValidateFeatureFlagAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ValidateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ValidateTravelBookingRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.FeatureFlag?.Owner?.Email ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
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
                    Locality = (request.Itinerary?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _travelTravelBookingValidationService.ValidateTravelBookingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ScheduleItineraryExpiryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.TravelBooking?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = (request.Itinerary?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (reply1.TravelBooking?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ScheduleItineraryExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an itinerary.
    /// </summary>
    public override async Task<ApiTravel.SuspendItineraryResponse> SuspendItinerary(ApiTravel.SuspendItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling SuspendItinerary for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "SuspendItinerary", "accepted", token);
        var input0 = new ApiEmployees.SuggestOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var reply0 = await SendSuspendItinerary0Async(input0, token);
        var response = new ApiTravel.SuspendItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.SuspendItineraryResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "SuspendItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.TerminateItineraryResponse> TerminateItinerary(ApiTravel.TerminateItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        _logger.LogInformation("Handling TerminateItinerary for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "TerminateItinerary", "accepted", token);
        var input0 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeesEmployeeService.SearchEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ScheduleTripExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScheduleTripExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiTravel.ScheduleTripExpiryRequest>();
        using var rpc1 = _travelTripLifecycleService.ScheduleTripExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.TerminateItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                IssuedAt = (reply1.Trip?.IssuedAt ?? new Timestamp()),
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.TerminateItineraryResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "TerminateItinerary", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.SuggestOrgUnitsResponse> SendSuspendItinerary0Async(ApiEmployees.SuggestOrgUnitsRequest request, CancellationToken token)
    {
        return await _itineraryLifecycleEndpointSendSuspendItinerary0Port.SendAsync(request, token);
    }
}
