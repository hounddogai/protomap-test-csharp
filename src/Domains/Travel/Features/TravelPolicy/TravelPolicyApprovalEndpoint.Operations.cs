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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelPolicyApprovalEndpoint
{
    /// <summary>
    /// Approves a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ApproveTravelPolicyResponse> ApproveTravelPolicy(ApiTravel.ApproveTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SignalTripWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _travelTripWorkflowService.SignalTripWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.SuggestUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SuggestUsersRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Trip?.Counterparty?.EmployeeId ?? ""),
                Region = (reply0.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _identityUserQueryService.SuggestUsersAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiItsm.ScheduleProblemExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Trip?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply0.Trip?.Counterparty?.EmployeeId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _itsmProblemLifecycleService.ScheduleProblemExpiryAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiCorporateevents.ResumeVenueWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ResumeVenueWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Problem?.Requester?.Email ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Problem?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor3 = new Method<ApiCorporateevents.ResumeVenueWorkflowRequest, ApiCorporateevents.ResumeVenueWorkflowResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".VenueWorkflowService", "ResumeVenueWorkflow",
            Wire.Marshaller<ApiCorporateevents.ResumeVenueWorkflowRequest>(), Wire.Marshaller<ApiCorporateevents.ResumeVenueWorkflowResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.ApproveTravelPolicyResponse
        {
            Outcome = new ApiTravel.ApproveTravelPolicyResponse.Types.Outcome
            {
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply2.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Description = (request.TravelPolicy?.Description ?? ""),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ApproveTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a travel policy.
    /// </summary>
    public override async Task<ApiTravel.CommentTravelPolicyApprovalResponse> CommentTravelPolicyApproval(ApiTravel.CommentTravelPolicyApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentTravelPolicyApproval for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CloneEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.CloneEmployeeRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmployeeService.CloneEmployeeAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiTravel.CommentTravelPolicyApprovalRequest, ApiHelpdesk.ScheduleSlaPolicyExpiryRequest>(value => new ApiHelpdesk.ScheduleSlaPolicyExpiryRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScheduleSlaPolicyExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Employee?.Contact?.Email ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Employee?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _helpdeskSlaPolicyLifecycleService.ScheduleSlaPolicyExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.CommentTravelPolicyApprovalResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Status = (request.TravelPolicy?.Status ?? (ApiTravel.TravelPolicyStatus)0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "CommentTravelPolicyApproval", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.DelegateTravelPolicyApprovalResponse> DelegateTravelPolicyApproval(ApiTravel.DelegateTravelPolicyApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateTravelPolicyApproval for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.SummarizeVenuesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiVenues.SummarizeVenuesRequest.Types.Details
            {
                Period = (request.TravelPolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (request.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _venuesVenueQueryService.SummarizeVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.DelegateTravelPolicyApprovalToGetTravelPolicyLifecycle1();
        using var rpc1 = _travelTravelPolicyLifecycleService.GetTravelPolicyLifecycleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.DelegateTravelPolicyApprovalResponse
        {
            Outcome = new ApiTravel.DelegateTravelPolicyApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Scopes = { (reply1.TravelPolicy?.Scopes ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "DelegateTravelPolicyApproval", (request.TravelPolicy?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.EscalateTravelPolicyResponse> EscalateTravelPolicy(ApiTravel.EscalateTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.CommentTravelBookingApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CommentTravelBookingApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.TravelPolicy?.Owner?.Email ?? ""),
                },
                Lines = { new ApiTravel.TravelBookingLine
                {
                    Tax = new ApiTravel.TravelBookingLineTax
                    {
                        Amount = (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
            },
        };
        using var rpc0 = _travelTravelBookingApprovalService.CommentTravelBookingApprovalAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiIdentity.BatchGetUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _identityUserService.BatchGetUsersAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTravel.EscalateTravelPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Status = (request.TravelPolicy?.Status ?? (ApiTravel.TravelPolicyStatus)0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "EscalateTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel policy.
    /// </summary>
    public override async Task<ApiTravel.GetTravelPolicyApprovalStatusResponse> GetTravelPolicyApprovalStatus(ApiTravel.GetTravelPolicyApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        _logger.LogInformation("Handling GetTravelPolicyApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "GetTravelPolicyApprovalStatus", "accepted", token);
        var input0 = new ApiSearch.LintSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.LintSearchIndexesRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            SearchIndex = new ApiSearch.SearchIndex
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiSearch.LintSearchIndexesRequest>();
        using var rpc0 = _searchSearchIndexValidationService.LintSearchIndexesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelPolicyApprovalStatusResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.SearchIndex?.Owner?.UserId ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists travel policies a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListPendingTravelPoliciesResponse> ListPendingTravelPolicies(ApiTravel.ListPendingTravelPoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingTravelPolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", "batch",
            "ListPendingTravelPolicies", "accepted", token);
        var response = new ApiTravel.ListPendingTravelPoliciesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ListPendingTravelPolicies", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a travel policy.
    /// </summary>
    public override async Task<ApiTravel.RecallTravelPolicyResponse> RecallTravelPolicy(ApiTravel.RecallTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBoxoffice.FindBoxOfficeSalesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBoxoffice.FindBoxOfficeSalesRequest.Types.Filter
            {
                CreatedAfter = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.TravelPolicy?.Owner?.UserId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiBoxoffice.FindBoxOfficeSalesRequest>(json0);
        using var rpc0 = _boxofficeBoxOfficeSaleQueryService.FindBoxOfficeSalesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ScorePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.TravelPolicy?.Owner?.Email ?? ""),
                },
                Route = new ApiEmployees.PositionRouteAction
                {
                    Target = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                    {
                        ApiTravel.TravelPolicy.ActionOneofCase.Route => (request.TravelPolicy?.Route?.Target ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _employeesPositionValidationService.ScorePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.ScheduleTravelPolicyReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicyId = (request.TravelPolicy?.TravelPolicyId ?? ""),
        };
        using var rpc2 = _travelTravelPolicyReportingService.ScheduleTravelPolicyReportAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.SummarizeTripsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SummarizeTripsRequest.Types.Details
            {
                Period = (reply2.Period ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (request.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc3 = _travelTripQueryService.SummarizeTripsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.RecallTravelPolicyResponse
        {
            Outcome = new ApiTravel.RecallTravelPolicyResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = (request.TravelPolicy?.Owner ?? new ApiIdentity.UserRef()),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.Name ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "RecallTravelPolicy", (request.TravelPolicy?.Name ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a travel policy.
    /// </summary>
    public override async Task<ApiTravel.RejectTravelPolicyResponse> RejectTravelPolicy(ApiTravel.RejectTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ResolveItineraryConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ResolveItineraryConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Lines = { new ApiTravel.ItineraryLine
                {
                    Tax = new ApiTravel.ItineraryLineTax
                    {
                        Amount = (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
            },
        };
        using var rpc0 = _travelItinerarySyncService.ResolveItineraryConflictAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SubscribeDocumentUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Reason = (request.TravelPolicy?.Block?.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentNotificationService.SubscribeDocumentUpdatesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTravel.RejectTravelPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Document?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Effective = (request.TravelPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "RejectTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a travel policy.
    /// </summary>
    public override async Task<ApiTravel.SubmitTravelPolicyResponse> SubmitTravelPolicy(ApiTravel.SubmitTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.AnnotateDocumentFolderVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateDocumentFolderVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Approver?.Email ?? ""),
                            UserId = (request.TravelPolicy?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderHistoryService.AnnotateDocumentFolderVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiRatelimiting.SimulateRateLimitResponse();
        foreach (var item in (request.TravelPolicy?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelPolicyCondition>()))
        {
            var input1 = new ApiRatelimiting.SimulateRateLimitRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                RateLimit = new ApiRatelimiting.RateLimit
                {
                    Conditions = { new ApiRatelimiting.RateLimitCondition
                    {
                        Negate = item.Negate,
                    } },
                },
            };
            using var rpc1 = _ratelimitingRateLimitCalculationService.SimulateRateLimitAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiTravel.DelegateTripApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.DocumentFolder?.Location?.Locality ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc2 = _travelTripApprovalService.DelegateTripApprovalAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.SubmitTravelPolicyResponse
        {
            Outcome = new ApiTravel.SubmitTravelPolicyResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Trip?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Name = (request.TravelPolicy?.Name ?? ""),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "SubmitTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
}
