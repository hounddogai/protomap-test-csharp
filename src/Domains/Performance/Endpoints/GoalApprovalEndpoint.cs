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
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Routes goals through approval, with escalation and delegation.
/// </summary>
public sealed class GoalApprovalEndpoint : ApiPerformance.GoalApprovalService.GoalApprovalServiceBase
{
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _corporateeventsCorporateEventApprovalService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly IGoalApprovalEndpointSendGetGoalApprovalStatus0Port _goalApprovalEndpointSendGetGoalApprovalStatus0Port;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient _itsmChangeRequestWorkflowService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<GoalApprovalEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _performanceCalibrationCalculationService;
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _performanceCalibrationValidationService;
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _performanceReviewApprovalService;
    private readonly ApiPerformance.ReviewService.ReviewServiceClient _performanceReviewService;
    private readonly ApiPersonalization.ProfileService.ProfileServiceClient _personalizationProfileService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiPerformance.CreateReviewRequest, CallOptions, AsyncUnaryCall<ApiPerformance.CreateReviewResponse>> _sendDelegateGoalApproval0;
    private readonly ApiTravel.TripApprovalService.TripApprovalServiceClient _travelTripApprovalService;

    public GoalApprovalEndpoint(
        ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient corporateeventsCorporateEventApprovalService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        IGoalApprovalEndpointSendGetGoalApprovalStatus0Port goalApprovalEndpointSendGetGoalApprovalStatus0PortAdapter,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient itsmChangeRequestWorkflowService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<GoalApprovalEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient performanceCalibrationCalculationService,
        ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient performanceCalibrationValidationService,
        ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient performanceReviewApprovalService,
        ApiPerformance.ReviewService.ReviewServiceClient performanceReviewService,
        ApiPersonalization.ProfileService.ProfileServiceClient personalizationProfileService,
        RetryPolicy policy,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        IRecordRepository repository,
        ApiTravel.TripApprovalService.TripApprovalServiceClient travelTripApprovalService)
    {
        _corporateeventsCorporateEventApprovalService = corporateeventsCorporateEventApprovalService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _goalApprovalEndpointSendGetGoalApprovalStatus0Port = goalApprovalEndpointSendGetGoalApprovalStatus0PortAdapter;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _itsmChangeRequestWorkflowService = itsmChangeRequestWorkflowService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _performanceCalibrationCalculationService = performanceCalibrationCalculationService;
        _performanceCalibrationValidationService = performanceCalibrationValidationService;
        _performanceReviewApprovalService = performanceReviewApprovalService;
        _performanceReviewService = performanceReviewService;
        _personalizationProfileService = personalizationProfileService;
        _policy = policy;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _repository = repository;
        _travelTripApprovalService = travelTripApprovalService;
        _sendDelegateGoalApproval0 = _performanceReviewService.CreateReviewAsync;
    }
    /// <summary>
    /// Approves a goal.
    /// </summary>
    public override async Task<ApiPerformance.ApproveGoalResponse> ApproveGoal(ApiPerformance.ApproveGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Goal?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainJobOpeningCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainJobOpeningCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Goal?.Requester?.Phone ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    Iban = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Bank => (reply0.JobOpening?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.JobOpening?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningCalculationService.ExplainJobOpeningCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        foreach (var item in (request.Goal?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.GoalComment>()))
        {
            var input2 = new ApiPerformance.WatchReviewsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Subscriber = new ApiCommon.ContactInfo
                {
                    Email = (item.AuthorEmail ?? ""),
                    Name = (request.Goal?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Goal?.Requester?.Phone ?? ""),
                },
            };
            using var stream2 = _performanceReviewService.WatchReviews(input2, cancellationToken: token);
            await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received WatchReviews frame {Type}", change2.GetType().Name);
            }
        }
        var input3 = new ApiHelpdesk.ListEscalationReportsRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.Filter = new ApiHelpdesk.ListEscalationReportsRequest.Types.Filter
        {
            CreatedAfter = (reply0.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
            Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
        };
        using var rpc3 = _helpdeskEscalationReportingService.ListEscalationReportsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.ApproveGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.JobOpening?.Audit?.CreatedBy ?? ""),
                },
                CustomFields = { (request.Goal?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                GoalId = (request.GoalId ?? ""),
            },
            Outcome = new ApiPerformance.ApproveGoalResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "ApproveGoal", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a goal.
    /// </summary>
    public override async Task<ApiPerformance.CommentGoalApprovalResponse> CommentGoalApproval(ApiPerformance.CommentGoalApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentGoalApproval for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.CalculateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Comments = { new ApiPerformance.CalibrationComment
                {
                    CreatedAt = (request.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _performanceCalibrationCalculationService.CalculateCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListSignatureConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureConflictsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Calibration?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
            },
        };
        using var rpc1 = _documentsSignatureSyncService.ListSignatureConflictsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.DelegateCorporateEventApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Goal?.Assignee?.Email ?? ""),
                        Phone = (request.Goal?.Requester?.Phone ?? ""),
                    },
                } },
            },
        };
        using var rpc2 = _corporateeventsCorporateEventApprovalService.DelegateCorporateEventApprovalAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiMetrics.BrowseMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.BrowseMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
                Region = (reply2.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _metricsMetricSeriesQueryService.BrowseMetricSeriesesAsync);
        var response = new ApiPerformance.CommentGoalApprovalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = (reply0.Calibration?.Assignee ?? new ApiIdentity.UserRef()),
                GoalId = (request.GoalId ?? ""),
                Subject = (request.Goal?.Subject ?? ""),
            },
            Outcome = new ApiPerformance.CommentGoalApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "CommentGoalApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a goal.
    /// </summary>
    public override async Task<ApiPerformance.DelegateGoalApprovalResponse> DelegateGoalApproval(ApiPerformance.DelegateGoalApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateGoalApproval for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.CreateReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Goal?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiPerformance.GoalRef
                {
                    DisplayName = (request.Goal?.Assignee?.DisplayName ?? ""),
                    Priority = (request.Goal?.Priority ?? 0),
                },
            },
        };
        using var rpc0 = _sendDelegateGoalApproval0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiTravel.ListPendingTripsResponse();
        foreach (var item in (request.Goal?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.GoalStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiTravel.ListPendingTripsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiTravel.ListPendingTripsRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    Region = (reply0.Review?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc1 = _travelTripApprovalService.ListPendingTripsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiRecruiting.BatchGetJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor2 = new Method<ApiRecruiting.BatchGetJobOpeningsRequest, ApiRecruiting.BatchGetJobOpeningsResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".JobOpeningService", "BatchGetJobOpenings",
            Wire.Marshaller<ApiRecruiting.BatchGetJobOpeningsRequest>(), Wire.Marshaller<ApiRecruiting.BatchGetJobOpeningsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiRecruiting.FindCandidatesResponse();
        foreach (var item in (request.Goal?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.GoalComment>()))
        {
            var input3 = new ApiRecruiting.FindCandidatesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiRecruiting.FindCandidatesRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Region = (reply0.Review?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc3 = _recruitingCandidateQueryService.FindCandidatesAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiPerformance.DelegateGoalApprovalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = (request.Goal?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Review?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiPerformance.GoalComment
                {
                    CreatedAt = (reply0.Review?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                GoalId = (request.GoalId ?? ""),
            },
            Outcome = new ApiPerformance.DelegateGoalApprovalResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "DelegateGoalApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a goal.
    /// </summary>
    public override async Task<ApiPerformance.EscalateGoalResponse> EscalateGoal(ApiPerformance.EscalateGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiPerformance.EscalateGoalRequest, ApiItsm.SignalChangeRequestWorkflowRequest>(value => new ApiItsm.SignalChangeRequestWorkflowRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (value.Goal?.Assignee?.DisplayName ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _itsmChangeRequestWorkflowService.SignalChangeRequestWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.CommentReviewApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Review = new ApiPerformance.Review
            {
                Counterparty = new ApiPerformance.GoalRef
                {
                    DisplayName = (reply0.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _performanceReviewApprovalService.CommentReviewApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.EscalateGoalToListUsers2(reply1);
        using var rpc2 = _identityUserService.ListUsersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.EscalateGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
                Status = (request.Goal?.Status ?? (ApiPerformance.GoalStatus)0),
            },
            Outcome = new ApiPerformance.EscalateGoalResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "EscalateGoal", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a goal.
    /// </summary>
    public override async Task<ApiPerformance.GetGoalApprovalStatusResponse> GetGoalApprovalStatus(ApiPerformance.GetGoalApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.GoalId, "goal_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetGoalApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", request.GoalId,
            "GetGoalApprovalStatus", "accepted", token);
        var input0 = new ApiPerformance.CloneReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Review = new ApiPerformance.Review
            {
                Counterparty = new ApiPerformance.GoalRef
                {
                    GoalId = (request.GoalId ?? ""),
                },
            },
        };
        var reply0 = await SendGetGoalApprovalStatus0Async(input0, token);
        var input1 = new ApiRecruiting.ResolveJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingJobOpeningQueryService.ResolveJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.GetGoalApprovalStatusResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Review?.Audit?.Etag ?? ""),
                },
                GoalId = (request.GoalId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists goals a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.ListPendingGoalsResponse> ListPendingGoals(ApiPerformance.ListPendingGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "ListPendingGoals", "accepted", token);
        var input0 = new ApiRecruiting.ValidateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ValidateJobOpeningRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.ValidateJobOpeningRequest>();
        using var rpc0 = _recruitingJobOpeningValidationService.ValidateJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.NormalizeJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    Bic = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Bank => (reply0.JobOpening?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningValidationService.NormalizeJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.ListPendingGoalsResponse
        {
            Goals = { new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "ListPendingGoals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a goal.
    /// </summary>
    public override async Task<ApiPerformance.RecallGoalResponse> RecallGoal(ApiPerformance.RecallGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.PreviewCalibrationRequest();
        input0.Calibration = new ApiPerformance.Calibration
        {
            Assignee = new ApiIdentity.UserRef
            {
                DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
            },
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.Goal?.Audit?.UpdatedBy ?? ""),
            },
            Requester = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiPerformance.PreviewCalibrationRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.Goal?.Assignee?.Email ?? ""),
            },
        };
        using var rpc0 = _performanceCalibrationValidationService.PreviewCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.RecallGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = (request.Goal?.Assignee ?? new ApiIdentity.UserRef()),
                GoalId = (request.GoalId ?? ""),
                Requester = (reply0.Calibration?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "RecallGoal", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a goal.
    /// </summary>
    public override async Task<ApiPerformance.RejectGoalResponse> RejectGoal(ApiPerformance.RejectGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.ExplainLedgerAccountDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.ExplainLedgerAccountDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Goal?.Assignee?.Email ?? ""),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Goal?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
                Holder = new ApiLedger.PostingBatchRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _ledgerLedgerAccountValidationService.ExplainLedgerAccountDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiPerformance.QuoteCalibrationResponse();
        foreach (var item in (request.Goal?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.GoalStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiPerformance.QuoteCalibrationRequest
            {
                Calibration = new ApiPerformance.Calibration
                {
                    History = { new ApiPerformance.CalibrationStatusChange
                    {
                        ChangedAt = (item.ChangedAt ?? new Timestamp()),
                        ChangedBy = (item.ChangedBy ?? ""),
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _performanceCalibrationCalculationService.QuoteCalibrationAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiPerformance.RejectGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiPerformance.GoalComment
                {
                    CreatedAt = (reply0.LedgerAccount?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                CustomFields = { (request.Goal?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                GoalId = (request.GoalId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "RejectGoal", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a goal.
    /// </summary>
    public override async Task<ApiPerformance.SubmitGoalResponse> SubmitGoal(ApiPerformance.SubmitGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CompareJobOpeningOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Goal?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Goal?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningCalculationService.CompareJobOpeningOptionsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiPersonalization.WatchProfilesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPersonalization.WatchProfilesRequest.Types.Filter
            {
                Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Phone = (request.Goal?.Requester?.Phone ?? ""),
            },
        };
        using var stream1 = _personalizationProfileService.WatchProfiles(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchProfiles frame {Type}", change1.GetType().Name);
        }
        var response = new ApiPerformance.SubmitGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
                Priority = (request.Goal?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "SubmitGoal", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiPerformance.CloneReviewResponse> SendGetGoalApprovalStatus0Async(ApiPerformance.CloneReviewRequest request, CancellationToken token)
    {
        return await _goalApprovalEndpointSendGetGoalApprovalStatus0Port.SendAsync(request, token);
    }
}
