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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class InterviewSyncEndpoint
{
    /// <summary>
    /// Returns an interview.
    /// </summary>
    public override async Task<ApiRecruiting.GetInterviewSyncStateResponse> GetInterviewSyncState(ApiRecruiting.GetInterviewSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        _logger.LogInformation("Handling GetInterviewSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "GetInterviewSyncState", "accepted", token);
        var input0 = new ApiGiftcards.SuggestGiftCardsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiGiftcards.SuggestGiftCardsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.InterviewId ?? ""),
            },
        };
        var named0 = _registry.Get<ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient>("acme.giftcards.v1.GiftCardQueryService");
        using var rpc0 = named0.SuggestGiftCardsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.CancelShiftWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                    ParticipantId = (reply0.Result?.CorrelationId ?? ""),
                } },
            },
        };
        var descriptor1 = new Method<ApiScheduling.CancelShiftWorkflowRequest, ApiScheduling.CancelShiftWorkflowResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".ShiftWorkflowService", "CancelShiftWorkflow",
            Wire.Marshaller<ApiScheduling.CancelShiftWorkflowRequest>(), Wire.Marshaller<ApiScheduling.CancelShiftWorkflowResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.GetInterviewSyncStateResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Shift?.Organizer?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists interviews a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListInterviewConflictsResponse> ListInterviewConflicts(ApiRecruiting.ListInterviewConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListInterviewConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", "batch",
            "ListInterviewConflicts", "accepted", token);
        var input0 = new ApiScheduling.SubmitSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.SubmitSwapRequestRequest.Types.Details
            {
                ApproverEmail = (request.Context?.Actor?.Email ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Sla = new ApiScheduling.SwapRequestSla
                {
                    DueAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
            },
        };
        var command0 = new DispatchCommand<ApiScheduling.SubmitSwapRequestRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _schedulingSwapRequestApprovalService.SubmitSwapRequestAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ListInterviewConflictsResponse
        {
            Interviews = { new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.SwapRequest?.Requester?.Email ?? ""),
                    },
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ListInterviewConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls an interview.
    /// </summary>
    public override async Task<ApiRecruiting.PullInterviewsResponse> PullInterviews(ApiRecruiting.PullInterviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullInterviews for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiWorkplaces.StartDeskWorkflowResponse();
        foreach (var item in (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiWorkplaces.StartDeskWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiWorkplaces.StartDeskWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc0 = _workplacesDeskWorkflowService.StartDeskWorkflowAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiAuthorization.CloneAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _authorizationAccessPolicyService.CloneAccessPolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.PullInterviewsToSearchDeliveries2(reply0, reply1);
        using var rpc2 = _deliveriesDeliveryService.SearchDeliveriesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.PullInterviewsResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.AccessPolicy?.Audit?.UpdatedBy ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.AccessPolicy?.Owner?.Email ?? ""),
                    },
                } },
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "PullInterviews", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs an interview.
    /// </summary>
    public override async Task<ApiRecruiting.PushInterviewResponse> PushInterview(ApiRecruiting.PushInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Interview?.Location?.Locality ?? ""),
                },
            },
        };
        var reply0 = await SendPushInterview0Async(input0, token);
        var input1 = new ApiRecruiting.SummarizeCandidatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SummarizeCandidatesRequest.Types.Details
            {
                Period = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingCandidateQueryService.SummarizeCandidatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.PushInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Employee?.RelationshipManager?.Email ?? ""),
                    },
                } },
                Title = (request.Interview?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Interview?.Organizer?.UserId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "PushInterview", (request.Interview?.Organizer?.UserId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ReconcileInterviewsResponse> ReconcileInterviews(ApiRecruiting.ReconcileInterviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileInterviews for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SubscribeDocumentTemplateUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentTemplateUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Interview?.Organizer?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentTemplateNotificationService.SubscribeDocumentTemplateUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.WatchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.WatchEmployeesRequest.Types.Filter
            {
                Region = (request.Interview?.Location?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Interview?.Organizer?.Email ?? ""),
            },
        };
        using var stream1 = _employeesEmployeeService.WatchEmployees(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchEmployees frame {Type}", change1.GetType().Name);
        }
        var response = new ApiRecruiting.ReconcileInterviewsResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.DocumentTemplate?.Audit?.CreatedBy ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Organizer = (request.Interview?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Interview?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ReconcileInterviews", (request.Interview?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ResolveInterviewConflictResponse> ResolveInterviewConflict(ApiRecruiting.ResolveInterviewConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveInterviewConflict for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SimulateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Daily = new ApiRecruiting.InterviewDailyRule
                {
                    Interval = (request.Interview ?? new ApiRecruiting.Interview()).RecurrenceCase switch
                    {
                        ApiRecruiting.Interview.RecurrenceOneofCase.Daily => (request.Interview?.Daily?.Interval ?? 0),
                        _ => 0
                    },
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Interview?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _recruitingInterviewCalculationService.SimulateInterviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.RecalculateOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Interview?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingOfferCalculationService.RecalculateOffersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ResolveInterviewConflictResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.Offer?.Counterparty?.Email ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.Offer?.Counterparty?.Email ?? ""),
                    },
                } },
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ResolveInterviewConflict", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of interviews with a peer.
    /// </summary>
    public override async Task StreamInterviewChanges(IAsyncStreamReader<ApiRecruiting.StreamInterviewChangesRequest> requests, IServerStreamWriter<ApiRecruiting.StreamInterviewChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling StreamInterviewChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiRecruiting.CheckInterviewEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Interview = new ApiRecruiting.Interview
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = request.FrameCase switch
                        {
                            ApiRecruiting.StreamInterviewChangesRequest.FrameOneofCase.Change => (request.Change?.Audit?.Etag ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _recruitingInterviewValidationService.CheckInterviewEligibilityAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var response = new ApiRecruiting.StreamInterviewChangesResponse
            {
                Interview = new ApiRecruiting.Interview
                {
                    InterviewId = (request.Change?.InterviewId ?? ""),
                    Metadata = { (reply0.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
                Result = new ApiCommon.OperationResult
                {
                    Message = (reply0.Result?.Message ?? ""),
                },
                Sequence = (request.Ack?.Sequence ?? 0L),
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "interview",
                "audit", "StreamInterviewChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs an interview.
    /// </summary>
    public override async Task<ApiRecruiting.SyncInterviewsResponse> SyncInterviews(ApiRecruiting.SyncInterviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncInterviews for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ResolveCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateQueryService.ResolveCandidatesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.BrowseCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.BrowseCandidatesRequest.Types.Filter
            {
                CreatedAfter = (request.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Interview?.Location?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _recruitingCandidateQueryService.BrowseCandidatesAsync);
        var response = new ApiRecruiting.SyncInterviewsResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiRecruiting.SyncInterviewsResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "SyncInterviews", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.CreateEmployeeResponse> SendPushInterview0Async(ApiEmployees.CreateEmployeeRequest request, CancellationToken token)
    {
        return await _interviewSyncEndpointSendPushInterview0Port.SendAsync(request, token);
    }
}
