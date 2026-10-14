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
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Routes interviews through approval, with escalation and delegation.
/// </summary>
public sealed class InterviewApprovalEndpoint : ApiRecruiting.InterviewApprovalService.InterviewApprovalServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiCart.CartQueryService.CartQueryServiceClient _cartCartQueryService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly IInterviewApprovalEndpointSendEscalateInterview0Port _interviewApprovalEndpointSendEscalateInterview0Port;
    private readonly ILogger<InterviewApprovalEndpoint> _logger;
    private readonly ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient _performanceFeedbackWorkflowService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient _recruitingCandidateApprovalService;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient _recruitingInterviewReportingService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient _recruitingJobOpeningLifecycleService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient _recruitingJobOpeningWorkflowService;
    private readonly ApiRecruiting.OfferReportingService.OfferReportingServiceClient _recruitingOfferReportingService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient _workplacesBuildingApprovalService;

    public InterviewApprovalEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiCart.CartQueryService.CartQueryServiceClient cartCartQueryService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        IInterviewApprovalEndpointSendEscalateInterview0Port interviewApprovalEndpointSendEscalateInterview0PortAdapter,
        ILogger<InterviewApprovalEndpoint> logger,
        ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient performanceFeedbackWorkflowService,
        RetryPolicy policy,
        ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient recruitingCandidateApprovalService,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient recruitingInterviewReportingService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient recruitingJobOpeningLifecycleService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient recruitingJobOpeningWorkflowService,
        ApiRecruiting.OfferReportingService.OfferReportingServiceClient recruitingOfferReportingService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient workplacesBuildingApprovalService)
    {
        _auditAuditEventService = auditAuditEventService;
        _cartCartQueryService = cartCartQueryService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _interviewApprovalEndpointSendEscalateInterview0Port = interviewApprovalEndpointSendEscalateInterview0PortAdapter;
        _logger = logger;
        _performanceFeedbackWorkflowService = performanceFeedbackWorkflowService;
        _policy = policy;
        _recruitingCandidateApprovalService = recruitingCandidateApprovalService;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingInterviewReportingService = recruitingInterviewReportingService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _recruitingJobOpeningLifecycleService = recruitingJobOpeningLifecycleService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _recruitingJobOpeningWorkflowService = recruitingJobOpeningWorkflowService;
        _recruitingOfferReportingService = recruitingOfferReportingService;
        _registry = registry;
        _repository = repository;
        _workplacesBuildingApprovalService = workplacesBuildingApprovalService;
    }
    /// <summary>
    /// Approves an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ApproveInterviewResponse> ApproveInterview(ApiRecruiting.ApproveInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.WatchAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.WatchAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Region = (request.Interview?.Location?.Region ?? ""),
            },
            Since = (request.Interview?.Audit?.CreatedAt ?? new Timestamp()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        using var stream0 = _auditAuditEventService.WatchAuditEvents(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchAuditEvents frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiDocuments.ExplainSignatureCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureCalculationService.ExplainSignatureCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ApproveInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ApproveInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an interview.
    /// </summary>
    public override async Task<ApiRecruiting.CommentInterviewApprovalResponse> CommentInterviewApproval(ApiRecruiting.CommentInterviewApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentInterviewApproval for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExportCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ExportCandidatesRequest.Types.Filter
            {
                Region = (request.Interview?.Location?.Region ?? ""),
            },
        };
        using var stream0 = _recruitingCandidateQueryService.ExportCandidates(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportCandidates frame {Type}", change0.GetType().Name);
        }
        var reply1 = new ApiRecruiting.AdvanceJobOpeningWorkflowResponse();
        foreach (var item in (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()))
        {
            var input1 = new ApiRecruiting.AdvanceJobOpeningWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiRecruiting.AdvanceJobOpeningWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
            };
            using var rpc1 = _recruitingJobOpeningWorkflowService.AdvanceJobOpeningWorkflowAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiRecruiting.CommentInterviewApprovalResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = (request.Interview?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "CommentInterviewApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.DelegateInterviewApprovalResponse> DelegateInterviewApproval(ApiRecruiting.DelegateInterviewApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateInterviewApproval for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExportJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ExportJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (request.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (request.Interview?.Location?.Region ?? ""),
            },
        };
        using var stream0 = _recruitingJobOpeningQueryService.ExportJobOpenings(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportJobOpenings frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiPerformance.CancelFeedbackWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.CancelFeedbackWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackWorkflowService.CancelFeedbackWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.DelegateInterviewApprovalResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiRecruiting.DelegateInterviewApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "DelegateInterviewApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.EscalateInterviewResponse> EscalateInterview(ApiRecruiting.EscalateInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.LintJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.LintJobOpeningsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Interview?.Location?.Recipient ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await SendEscalateInterview0Async(input0, token);
        var response = new ApiRecruiting.EscalateInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "EscalateInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an interview.
    /// </summary>
    public override async Task<ApiRecruiting.GetInterviewApprovalStatusResponse> GetInterviewApprovalStatus(ApiRecruiting.GetInterviewApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetInterviewApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "GetInterviewApprovalStatus", "accepted", token);
        var input0 = new ApiRecruiting.ScheduleJobOpeningExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ScheduleJobOpeningExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    InterviewId = (request.InterviewId ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingJobOpeningLifecycleService.ScheduleJobOpeningExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetInterviewApprovalStatusResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists interviews a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListPendingInterviewsResponse> ListPendingInterviews(ApiRecruiting.ListPendingInterviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingInterviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", "batch",
            "ListPendingInterviews", "accepted", token);
        var input0 = new ApiEmployees.ExplainPositionDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainPositionDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.ExplainPositionDecisionRequest>();
        using var rpc0 = _employeesPositionValidationService.ExplainPositionDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ListPendingBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiWorkplaces.ListPendingBuildingsRequest>(json1);
        using var rpc1 = _workplacesBuildingApprovalService.ListPendingBuildingsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ListPendingInterviewsResponse
        {
            Interviews = { new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Position?.Audit?.UpdatedBy ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ListPendingInterviews", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an interview.
    /// </summary>
    public override async Task<ApiRecruiting.RecallInterviewResponse> RecallInterview(ApiRecruiting.RecallInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ListInterviewConflictsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiRecruiting.ListInterviewConflictsRequest.Types.Filter
        {
            CreatedAfter = (request.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
            OwnerId = (request.Interview?.Organizer?.UserId ?? ""),
            Region = (request.Interview?.Location?.Region ?? ""),
        };
        using var rpc0 = _recruitingInterviewSyncService.ListInterviewConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCart.ExportCartsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCart.ExportCartsRequest.Types.Filter
            {
                CreatedAfter = (request.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Interview?.Organizer?.UserId ?? ""),
                Region = (request.Interview?.Location?.Region ?? ""),
            },
        };
        using var stream1 = _cartCartQueryService.ExportCarts(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportCarts frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiRecruiting.RejectCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Interview?.Audit?.CreatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RejectCandidateRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Interview?.Organizer?.UserId ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateApprovalService.RejectCandidateAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.RecallInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (reply2.Candidate?.Contact?.Phone ?? ""),
                    },
                } },
                Title = (request.Interview?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "RecallInterview", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an interview.
    /// </summary>
    public override async Task<ApiRecruiting.RejectInterviewResponse> RejectInterview(ApiRecruiting.RejectInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.EstimateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateInterviewRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Interview?.Location?.Region ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Interview?.Organizer?.UserId ?? ""),
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
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingInterviewCalculationService.EstimateInterviewAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.DeleteInterviewReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Etag = (request.Interview?.Audit?.Etag ?? ""),
            InterviewId = (request.Interview?.InterviewId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _recruitingInterviewReportingService.DeleteInterviewReportAsync);
        var response = new ApiRecruiting.RejectInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = (request.Interview?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = (reply0.Interview?.Organizer ?? new ApiIdentity.UserRef()),
                Window = (reply0.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiRecruiting.RejectInterviewResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Interview?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "RejectInterview", (request.Interview?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an interview.
    /// </summary>
    public override async Task<ApiRecruiting.SubmitInterviewResponse> SubmitInterview(ApiRecruiting.SubmitInterviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitInterview for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.StreamOfferMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Interview?.Organizer?.Email ?? ""),
            },
        };
        using var stream0 = _recruitingOfferReportingService.StreamOfferMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamOfferMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Interview?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.Interview?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Interview?.Organizer?.Email ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiEmployees.PositionValidationService.PositionValidationServiceClient>("acme.employees.v1.PositionValidationService");
        using var rpc1 = named1.PreviewPositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SubmitInterviewResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Position?.Audit?.CreatedBy ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "SubmitInterview", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.LintJobOpeningsResponse> SendEscalateInterview0Async(ApiRecruiting.LintJobOpeningsRequest request, CancellationToken token)
    {
        return await _interviewApprovalEndpointSendEscalateInterview0Port.SendAsync(request, token);
    }
}
