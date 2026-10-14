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
using ApiAnalytics = Acme.Analytics.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiProcurement = Acme.Procurement.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Runs the long-lived workflow that takes a dependent from start to finish.
/// </summary>
public sealed class DependentWorkflowEndpoint : ApiBenefits.DependentWorkflowService.DependentWorkflowServiceBase
{
    private readonly ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient _analyticsAnalyticsEventService;
    private readonly ApiBenefits.DependentApprovalService.DependentApprovalServiceClient _benefitsDependentApprovalService;
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _benefitsDependentValidationService;
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient _benefitsEnrollmentWorkflowService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _learningCourseApprovalService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<DependentWorkflowEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient _procurementPurchaseRequisitionCalculationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.DelegateCourseApprovalRequest, CallOptions, AsyncUnaryCall<ApiLearning.DelegateCourseApprovalResponse>> _sendInspectDependentWorkflow0;

    public DependentWorkflowEndpoint(
        ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient analyticsAnalyticsEventService,
        ApiBenefits.DependentApprovalService.DependentApprovalServiceClient benefitsDependentApprovalService,
        ApiBenefits.DependentValidationService.DependentValidationServiceClient benefitsDependentValidationService,
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient benefitsEnrollmentWorkflowService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        CallInvoker invoker,
        ApiLearning.CourseApprovalService.CourseApprovalServiceClient learningCourseApprovalService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<DependentWorkflowEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient procurementPurchaseRequisitionCalculationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository)
    {
        _analyticsAnalyticsEventService = analyticsAnalyticsEventService;
        _benefitsDependentApprovalService = benefitsDependentApprovalService;
        _benefitsDependentValidationService = benefitsDependentValidationService;
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _benefitsEnrollmentWorkflowService = benefitsEnrollmentWorkflowService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _invoker = invoker;
        _learningCourseApprovalService = learningCourseApprovalService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _procurementPurchaseRequisitionCalculationService = procurementPurchaseRequisitionCalculationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _sendInspectDependentWorkflow0 = _learningCourseApprovalService.DelegateCourseApprovalAsync;
    }
    /// <summary>
    /// Advances a dependent.
    /// </summary>
    public override async Task<ApiBenefits.AdvanceDependentWorkflowResponse> AdvanceDependentWorkflow(ApiBenefits.AdvanceDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceDependentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.RetryEnrollmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.RetryEnrollmentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Dependent?.RelationshipManager?.Email ?? ""),
                    Phone = (request.Dependent?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            Enrollment = new ApiBenefits.Enrollment
            {
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Dependent?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _benefitsEnrollmentWorkflowService.RetryEnrollmentWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiBenefits.AdvanceDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Enrollment?.Audit?.UpdatedBy ?? ""),
                },
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = (request.Dependent?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiBenefits.AdvanceDependentWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "AdvanceDependentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a dependent.
    /// </summary>
    public override async Task<ApiBenefits.CancelDependentWorkflowResponse> CancelDependentWorkflow(ApiBenefits.CancelDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelDependentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ValidateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ValidateOfferRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingOfferValidationService.ValidateOfferAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.ExplainInterviewCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Offer?.Counterparty?.Email ?? ""),
                        Phone = (request.Details?.Notify?.Phone ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _recruitingInterviewCalculationService.ExplainInterviewCalculationAsync);
        var response = new ApiBenefits.CancelDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Offer?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply1.Interview?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
                },
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Interview?.Organizer?.UserId ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "CancelDependentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a dependent.
    /// </summary>
    public override async Task<ApiBenefits.InspectDependentWorkflowResponse> InspectDependentWorkflow(ApiBenefits.InspectDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DependentId, "dependent_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectDependentWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "dependent", request.DependentId,
            "InspectDependentWorkflow", "accepted", token);
        var input0 = new ApiLearning.DelegateCourseApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.DelegateCourseApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendInspectDependentWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.InspectDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                DependentId = (request.DependentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists dependents a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.ListDependentWorkflowsResponse> ListDependentWorkflows(ApiBenefits.ListDependentWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListDependentWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "dependent", "batch",
            "ListDependentWorkflows", "accepted", token);
        var input0 = new ApiAnalytics.CreateAnalyticsEventRequest
        {
            AnalyticsEvent = new ApiAnalytics.AnalyticsEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAnalytics.AnalyticsEventMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _analyticsAnalyticsEventService.CreateAnalyticsEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.ListDependentWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "ListDependentWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a dependent.
    /// </summary>
    public override async Task<ApiBenefits.PauseDependentWorkflowResponse> PauseDependentWorkflow(ApiBenefits.PauseDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseDependentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.SearchNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.SearchNotificationsRequest.Types.Filter
            {
                CreatedAfter = (request.Dependent?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiNotifications.SearchNotificationsRequest, ApiNotifications.SearchNotificationsResponse>(
            MethodType.Unary, "acme.notifications.v1" + ".NotificationService", "SearchNotifications",
            Wire.Marshaller<ApiNotifications.SearchNotificationsRequest>(), Wire.Marshaller<ApiNotifications.SearchNotificationsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.CountEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.CountEnrollmentsRequest.Types.Filter
            {
                CreatedAfter = (request.Dependent ?? new ApiBenefits.Dependent()).IdentityDocumentCase switch
                {
                    ApiBenefits.Dependent.IdentityDocumentOneofCase.Passport => (request.Dependent?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Dependent?.DependentId ?? ""),
                Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _benefitsEnrollmentQueryService.CountEnrollmentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.PauseDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                DependentId = (request.DependentId ?? ""),
                Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
            },
            Outcome = new ApiBenefits.PauseDependentWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "PauseDependentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a dependent.
    /// </summary>
    public override async Task<ApiBenefits.ResumeDependentWorkflowResponse> ResumeDependentWorkflow(ApiBenefits.ResumeDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeDependentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiBenefits.ResumeDependentWorkflowRequest, ApiRecruiting.BatchGetJobOpeningsRequest>(value => new ApiRecruiting.BatchGetJobOpeningsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _recruitingJobOpeningService.BatchGetJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.ResumeDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
                },
                DependentId = (request.DependentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "ResumeDependentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a dependent.
    /// </summary>
    public override async Task<ApiBenefits.RetryDependentWorkflowResponse> RetryDependentWorkflow(ApiBenefits.RetryDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryDependentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.BrowseJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.BrowseJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (request.Dependent ?? new ApiBenefits.Dependent()).IdentityDocumentCase switch
                {
                    ApiBenefits.Dependent.IdentityDocumentOneofCase.Passport => (request.Dependent?.Passport?.ExpiresAt ?? new Timestamp()),
                    _ => new Timestamp()
                },
                Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _recruitingJobOpeningQueryService.BrowseJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.ApproveDependentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Preferences = new ApiBenefits.DependentPreferences
                {
                    Language = (request.Dependent?.Preferences?.Language ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsDependentApprovalService.ApproveDependentAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.SimulateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SimulateOfferRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Dependent?.PrimaryAddress?.Recipient ?? ""),
                },
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Dependent?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.Dependent?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _recruitingOfferCalculationService.SimulateOfferAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiRecruiting.EstimateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Dependent?.RelationshipManager?.Email ?? ""),
                        PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _recruitingInterviewCalculationService.EstimateInterviewAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.RetryDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                DependentId = (request.DependentId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply3.Interview?.Location?.CountryCode ?? ""),
                    Recipient = (reply2.Offer?.BillingAddress?.Recipient ?? ""),
                    Region = (reply3.Interview?.Location?.Region ?? ""),
                },
                RelationshipManager = (request.Dependent?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiBenefits.RetryDependentWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "RetryDependentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a dependent.
    /// </summary>
    public override async Task<ApiBenefits.SignalDependentWorkflowResponse> SignalDependentWorkflow(ApiBenefits.SignalDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalDependentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.GetDependentRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DependentId = (request.Dependent?.DependentId ?? ""),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiBenefits.GetDependentRulesRequest>();
        using var rpc0 = _benefitsDependentValidationService.GetDependentRulesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.ExplainLifeEventCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.ExplainLifeEventCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Dependent?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Dependent?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.ExplainLifeEventCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.SignalDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
                },
                DependentId = (request.DependentId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Dependent?.PrimaryAddress?.Recipient ?? ""),
                },
            },
            Outcome = new ApiBenefits.SignalDependentWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.LifeEvent?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "SignalDependentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a dependent.
    /// </summary>
    public override async Task<ApiBenefits.StartDependentWorkflowResponse> StartDependentWorkflow(ApiBenefits.StartDependentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartDependentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiProcurement.ExplainPurchaseRequisitionCalculationRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiProcurement.ExplainPurchaseRequisitionCalculationRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Phone = (request.Dependent?.Contact?.Phone ?? ""),
            },
        };
        input0.PurchaseRequisition = new ApiProcurement.PurchaseRequisition
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (request.Dependent?.Audit?.Etag ?? ""),
                UpdatedBy = (request.Dependent?.Audit?.UpdatedBy ?? ""),
            },
            Counterparty = new ApiProcurement.PurchaseOrderRef
            {
                DisplayName = (request.Dependent?.RelationshipManager?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _procurementPurchaseRequisitionCalculationService.ExplainPurchaseRequisitionCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.PauseEnrollmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.PauseEnrollmentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Dependent?.Contact?.Phone ?? ""),
                },
            },
            Enrollment = new ApiBenefits.Enrollment
            {
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Dependent?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsEnrollmentWorkflowService.PauseEnrollmentWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.StreamLicenseAssignmentMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply1.Enrollment?.Holder?.Email ?? ""),
                Phone = (request.Dependent?.Contact?.Phone ?? ""),
            },
        };
        using var stream2 = _licensingLicenseAssignmentReportingService.StreamLicenseAssignmentMetrics(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamLicenseAssignmentMetrics frame {Type}", change2.GetType().Name);
        }
        var response = new ApiBenefits.StartDependentWorkflowResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.PurchaseRequisition?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.PurchaseRequisition?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.Enrollment?.Contact?.Phone ?? ""),
                },
                DependentId = (request.DependentId ?? ""),
                Status = (request.Dependent?.Status ?? (ApiBenefits.DependentStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "StartDependentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
