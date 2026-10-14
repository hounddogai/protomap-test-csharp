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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Routes learning paths through approval, with escalation and delegation.
/// </summary>
public sealed class LearningPathApprovalEndpoint : ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceBase
{
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.CertificationValidationService.CertificationValidationServiceClient _learningCertificationValidationService;
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _learningCourseApprovalService;
    private readonly ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient _learningCourseEnrollmentCalculationService;
    private readonly ApiLearning.CourseQueryService.CourseQueryServiceClient _learningCourseQueryService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient _learningLearningPathQueryService;
    private readonly ILearningPathApprovalEndpointSendRecallLearningPath1Port _learningPathApprovalEndpointSendRecallLearningPath1Port;
    private readonly ILogger<LearningPathApprovalEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.BrowseCoursesRequest, CallOptions, AsyncUnaryCall<ApiLearning.BrowseCoursesResponse>> _sendDelegateLearningPathApproval2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient _workplacesDeskWorkflowService;

    public LearningPathApprovalEndpoint(
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        CallInvoker invoker,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.CertificationValidationService.CertificationValidationServiceClient learningCertificationValidationService,
        ApiLearning.CourseApprovalService.CourseApprovalServiceClient learningCourseApprovalService,
        ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient learningCourseEnrollmentCalculationService,
        ApiLearning.CourseQueryService.CourseQueryServiceClient learningCourseQueryService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient learningLearningPathQueryService,
        ILearningPathApprovalEndpointSendRecallLearningPath1Port learningPathApprovalEndpointSendRecallLearningPath1PortAdapter,
        ILogger<LearningPathApprovalEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        RetryPolicy policy,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient workplacesDeskWorkflowService)
    {
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _invoker = invoker;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningCertificationValidationService = learningCertificationValidationService;
        _learningCourseApprovalService = learningCourseApprovalService;
        _learningCourseEnrollmentCalculationService = learningCourseEnrollmentCalculationService;
        _learningCourseQueryService = learningCourseQueryService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _learningLearningPathQueryService = learningLearningPathQueryService;
        _learningPathApprovalEndpointSendRecallLearningPath1Port = learningPathApprovalEndpointSendRecallLearningPath1PortAdapter;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _policy = policy;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _settings = settings;
        _workplacesDeskWorkflowService = workplacesDeskWorkflowService;
        _sendDelegateLearningPathApproval2 = _learningCourseQueryService.BrowseCoursesAsync;
    }
    /// <summary>
    /// Approves a learning path.
    /// </summary>
    public override async Task<ApiLearning.ApproveLearningPathResponse> ApproveLearningPath(ApiLearning.ApproveLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.NormalizeOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.NormalizeOfferRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.NormalizeOfferRequest>();
        using var rpc0 = _recruitingOfferValidationService.NormalizeOfferAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.RecalculateLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (reply0.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                            UpdatedBy = (reply0.Offer?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiLearning.RecalculateLearningPathsRequest>(json1);
        using var rpc1 = _learningLearningPathCalculationService.RecalculateLearningPathsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.EstimateOfferRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Details = new ApiRecruiting.EstimateOfferRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                Recipient = (reply0.Offer?.BillingAddress?.Recipient ?? ""),
            },
        };
        input2.Offer = new ApiRecruiting.Offer
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (reply1.LearningPath?.Audit?.UpdatedBy ?? ""),
            },
            BillingAddress = new ApiCommon.PostalAddress
            {
                Recipient = (request.LearningPath?.Location?.Recipient ?? ""),
            },
        };
        using var rpc2 = _recruitingOfferCalculationService.EstimateOfferAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.ApproveLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Attributes = { (request.LearningPath?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (reply0.Offer?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
            },
            Outcome = new ApiLearning.ApproveLearningPathResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ApproveLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a learning path.
    /// </summary>
    public override async Task<ApiLearning.CommentLearningPathApprovalResponse> CommentLearningPathApproval(ApiLearning.CommentLearningPathApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentLearningPathApproval for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.SearchMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.SearchMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.LearningPath?.Manager?.UserId ?? ""),
                Region = (request.LearningPath?.Location?.Region ?? ""),
            },
        };
        using var rpc0 = _metricsMetricSeriesService.SearchMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.LookupLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPathId = (request.LearningPathId ?? ""),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _learningLearningPathQueryService.LookupLearningPathAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLearning.CommentLearningPathApprovalResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                LearningPathId = (request.LearningPathId ?? ""),
                ParentId = (request.LearningPath?.ParentId ?? ""),
                Path = { (reply1.LearningPath?.Path ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "CommentLearningPathApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a learning path.
    /// </summary>
    public override async Task<ApiLearning.DelegateLearningPathApprovalResponse> DelegateLearningPathApproval(ApiLearning.DelegateLearningPathApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateLearningPathApproval for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.ApproveCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.LearningPath?.Manager?.UserId ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _learningCourseApprovalService.ApproveCourseAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.LintOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.LintOffersRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _recruitingOfferValidationService.LintOffersAsync);
        var input2 = new ApiLearning.BrowseCoursesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.BrowseCoursesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Offer?.OfferId ?? ""),
                Region = (request.LearningPath?.Location?.Region ?? ""),
            },
        };
        using var rpc2 = _sendDelegateLearningPathApproval2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.RejectCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    Region = (request.LearningPath?.Location?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Course?.Owner?.DisplayName ?? ""),
                    UserId = (request.LearningPath?.Manager?.UserId ?? ""),
                },
            },
        };
        using var rpc3 = _learningCourseApprovalService.RejectCourseAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.DelegateLearningPathApprovalResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply1.Offer?.BillingAddress?.Region ?? ""),
                },
                Name = (reply3.Course?.Name ?? ""),
                Status = (request.LearningPath?.Status ?? (ApiLearning.LearningPathStatus)0),
            },
            Outcome = new ApiLearning.DelegateLearningPathApprovalResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "DelegateLearningPathApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a learning path.
    /// </summary>
    public override async Task<ApiLearning.EscalateLearningPathResponse> EscalateLearningPath(ApiLearning.EscalateLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.NormalizeCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.LearningPath?.Location?.Recipient ?? ""),
                },
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiLearning.NormalizeCertificationRequest, ApiLearning.NormalizeCertificationResponse>(
            MethodType.Unary, "acme.learning.v1" + ".CertificationValidationService", "NormalizeCertification",
            Wire.Marshaller<ApiLearning.NormalizeCertificationRequest>(), Wire.Marshaller<ApiLearning.NormalizeCertificationResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainJobOpeningDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainJobOpeningDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Certification?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiRecruiting.ExplainJobOpeningDecisionRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _recruitingJobOpeningValidationService.ExplainJobOpeningDecisionAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.GetLearningPathRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPathId = (request.LearningPath?.LearningPathId ?? ""),
        };
        using var rpc2 = _learningLearningPathCalculationService.GetLearningPathRatesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiLearning.RecalculateCourseEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Certification?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _learningCourseEnrollmentCalculationService.RecalculateCourseEnrollmentsAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiLearning.EscalateLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Attributes = { (request.LearningPath?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.LearningPath?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply3.CourseEnrollment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply3.CourseEnrollment?.Audit?.UpdatedBy ?? ""),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply1.JobOpening?.BillingAddress?.CountryCode ?? ""),
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "EscalateLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a learning path.
    /// </summary>
    public override async Task<ApiLearning.GetLearningPathApprovalStatusResponse> GetLearningPathApprovalStatus(ApiLearning.GetLearningPathApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LearningPathId, "learning_path_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetLearningPathApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", request.LearningPathId,
            "GetLearningPathApprovalStatus", "accepted", token);
        var response = new ApiLearning.GetLearningPathApprovalStatusResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                LearningPathId = (request.LearningPathId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists learning paths a page at a time.
    /// </summary>
    public override async Task<ApiLearning.ListPendingLearningPathsResponse> ListPendingLearningPaths(ApiLearning.ListPendingLearningPathsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingLearningPaths for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", "batch",
            "ListPendingLearningPaths", "accepted", token);
        var response = new ApiLearning.ListPendingLearningPathsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ListPendingLearningPaths", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a learning path.
    /// </summary>
    public override async Task<ApiLearning.RecallLearningPathResponse> RecallLearningPath(ApiLearning.RecallLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RecallLearningPathToListSignatureVersions0();
        using var rpc0 = _documentsSignatureHistoryService.ListSignatureVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.ApproveCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.LearningPath?.Location?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LearningPath?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LearningPath?.Manager?.DisplayName ?? ""),
                    Email = (request.LearningPath?.Manager?.Email ?? ""),
                },
            },
        };
        var reply1 = await SendRecallLearningPath1Async(input1, token);
        var response = new ApiLearning.RecallLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (reply1.Course?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Course?.Address?.PostalCode ?? ""),
                },
                Manager = (request.LearningPath?.Manager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "RecallLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a learning path.
    /// </summary>
    public override async Task<ApiLearning.RejectLearningPathResponse> RejectLearningPath(ApiLearning.RejectLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.StartDeskWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LearningPath?.Manager?.DisplayName ?? ""),
                    Email = (request.LearningPath?.Manager?.Email ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _workplacesDeskWorkflowService.StartDeskWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.SimulateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.LearningPath?.Manager?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiLearning.SimulateLearningPathRequest>();
        using var rpc1 = _learningLearningPathCalculationService.SimulateLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.CompareCertificationOptionsRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiLearning.CompareCertificationOptionsRequest>(json2);
        using var rpc2 = _learningCertificationCalculationService.CompareCertificationOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.CalculateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.LearningPath?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                            {
                                ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc3 = _learningLearningPathCalculationService.CalculateLearningPathAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.RejectLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply0.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Name = (reply3.LearningPath?.Name ?? ""),
                        Path = { (reply1.LearningPath?.Path ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        Status = (reply1.LearningPath?.Status ?? (ApiLearning.LearningPathStatus)0),
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (reply2.Certification?.Counterparty?.Email ?? ""),
                    },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = (request.LearningPath?.Location ?? new ApiCommon.PostalAddress()),
                Manager = new ApiIdentity.UserRef
                {
                    Email = (reply3.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Outcome = new ApiLearning.RejectLearningPathResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "RejectLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a learning path.
    /// </summary>
    public override async Task<ApiLearning.SubmitLearningPathResponse> SubmitLearningPath(ApiLearning.SubmitLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitLearningPath for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SearchJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SearchJobOpeningsRequest.Types.Filter
            {
                Region = (request.LearningPath?.Location?.Region ?? ""),
            },
        };
        using var rpc0 = _recruitingJobOpeningService.SearchJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.QuoteCourseEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _learningCourseEnrollmentCalculationService.QuoteCourseEnrollmentAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLearning.SubmitLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.CourseEnrollment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                LearningPathId = (request.LearningPathId ?? ""),
                ParentId = (request.LearningPath?.ParentId ?? ""),
            },
            Outcome = new ApiLearning.SubmitLearningPathResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "SubmitLearningPath", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLearning.ApproveCourseResponse> SendRecallLearningPath1Async(ApiLearning.ApproveCourseRequest request, CancellationToken token)
    {
        return await _learningPathApprovalEndpointSendRecallLearningPath1Port.SendAsync(request, token);
    }
}
