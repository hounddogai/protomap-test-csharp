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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Moves a job opening between the states of its lifecycle.
/// </summary>
public sealed class JobOpeningLifecycleEndpoint : ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient _chatChatRoomQueryService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<JobOpeningLifecycleEndpoint> _logger;
    private readonly ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient _onboardingOnboardingPlanApprovalService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public JobOpeningLifecycleEndpoint(
        NotificationBus bus,
        ApiChat.ChatRoomQueryService.ChatRoomQueryServiceClient chatChatRoomQueryService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ILogger<JobOpeningLifecycleEndpoint> logger,
        ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient onboardingOnboardingPlanApprovalService,
        RetryPolicy policy,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _bus = bus;
        _chatChatRoomQueryService = chatChatRoomQueryService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _logger = logger;
        _onboardingOnboardingPlanApprovalService = onboardingOnboardingPlanApprovalService;
        _policy = policy;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _registry = registry;
        _repository = repository;
    }
    /// <summary>
    /// Activates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ActivateJobOpeningResponse> ActivateJobOpening(ApiRecruiting.ActivateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "ActivateJobOpening", "accepted", token);
        var input0 = new ApiDevices.RetryManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RetryManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _devicesManagedDeviceWorkflowService.RetryManagedDeviceWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiChat.BrowseChatRoomsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiChat.BrowseChatRoomsRequest.Types.Filter
            {
                CreatedAfter = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _chatChatRoomQueryService.BrowseChatRoomsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ActivateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Outcome = new ApiRecruiting.ActivateJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ActivateJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.GetJobOpeningLifecycleResponse> GetJobOpeningLifecycle(ApiRecruiting.GetJobOpeningLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetJobOpeningLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "GetJobOpeningLifecycle", "accepted", token);
        var input0 = new ApiRecruiting.ReconcileInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ReconcileInterviewsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _recruitingInterviewSyncService.ReconcileInterviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetJobOpeningLifecycleResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Interview?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Interview?.Location?.PostalCode ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ReinstateJobOpeningResponse> ReinstateJobOpening(ApiRecruiting.ReinstateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "ReinstateJobOpening", "accepted", token);
        var input0 = new ApiRecruiting.ScoreInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient>("acme.recruiting.v1.InterviewValidationService");
        using var rpc0 = named0.ScoreInterviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.ScoreInterviewRequest>("acme.recruiting.v1.JobOpeningLifecycleService/ReinstateJobOpening/0", input0.Clone()), token);
        var response = new ApiRecruiting.ReinstateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.Interview?.Organizer?.DisplayName ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Outcome = new ApiRecruiting.ReinstateJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ReinstateJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.RenewJobOpeningResponse> RenewJobOpening(ApiRecruiting.RenewJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "RenewJobOpening", "accepted", token);
        var input0 = new ApiOnboarding.SubmitOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SubmitOnboardingPlanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiOnboarding.SubmitOnboardingPlanRequest, ApiOnboarding.SubmitOnboardingPlanResponse>(
            MethodType.Unary, "acme.onboarding.v1" + ".OnboardingPlanApprovalService", "SubmitOnboardingPlan",
            Wire.Marshaller<ApiOnboarding.SubmitOnboardingPlanRequest>(), Wire.Marshaller<ApiOnboarding.SubmitOnboardingPlanResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.PreviewJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    Bic = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (reply0.OnboardingPlan?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.OnboardingPlan?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningValidationService.PreviewJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiRecruiting.RenewJobOpeningRequest, ApiEmployees.RecalculateEmploymentContractsRequest>(value => new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.BrowseEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseEmployeesRequest.Types.Filter
            {
                Region = (reply0.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _employeesEmployeeQueryService.BrowseEmployeesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.RenewJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.OnboardingPlan?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Status = (reply1.JobOpening?.Status ?? (ApiRecruiting.JobOpeningStatus)0),
            },
            Outcome = new ApiRecruiting.RenewJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply3.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "RenewJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ScheduleJobOpeningExpiryResponse> ScheduleJobOpeningExpiry(ApiRecruiting.ScheduleJobOpeningExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleJobOpeningExpiry for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ScheduleJobOpeningExpiryToPreviewEmploymentContract0();
        using var rpc0 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiRecruiting.CalculateJobOpeningResponse();
        foreach (var item in (request.JobOpening?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningAttachment>()))
        {
            var input1 = new ApiRecruiting.CalculateJobOpeningRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Attachments = { new ApiRecruiting.JobOpeningAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                    } },
                },
            };
            using var rpc1 = _recruitingJobOpeningCalculationService.CalculateJobOpeningAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiRecruiting.ScheduleJobOpeningExpiryResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.EmploymentContract?.Audit?.Etag ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Metadata = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ScheduleJobOpeningExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.SuspendJobOpeningResponse> SuspendJobOpening(ApiRecruiting.SuspendJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "SuspendJobOpening", "accepted", token);
        var input0 = new ApiDocuments.ExplainDocumentFolderDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainDocumentFolderDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.NotifyContact?.Email ?? ""),
                        UserId = (request.Details?.NewOwner?.UserId ?? ""),
                    },
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _documentsDocumentFolderValidationService.ExplainDocumentFolderDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.DocumentFolder?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.DocumentFolder?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DocumentFolder?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiEmployees.CheckPositionEligibilityRequest>();
        using var rpc1 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.BrowseCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.BrowseCandidatesRequest.Types.Filter
            {
                CreatedAfter = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply1.Outcome?.ReferenceId ?? ""),
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiRecruiting.BrowseCandidatesRequest>(json2);
        using var rpc2 = _recruitingCandidateQueryService.BrowseCandidatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.SummarizeCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (reply1.Position ?? new ApiEmployees.Position()).ActionCase switch
            {
                ApiEmployees.Position.ActionOneofCase.Block => (reply1.Position?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc3 = _recruitingCandidateQueryService.SummarizeCandidatesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.SuspendJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.DocumentFolder?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply1.Position?.Owner?.DisplayName ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Tax = new ApiRecruiting.JobOpeningLineTax
                    {
                        Amount = (reply1.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
            },
            Outcome = new ApiRecruiting.SuspendJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "SuspendJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.TerminateJobOpeningResponse> TerminateJobOpening(ApiRecruiting.TerminateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "TerminateJobOpening", "accepted", token);
        var input0 = new ApiRecruiting.CompareInterviewOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.NewOwner?.Email ?? ""),
                        Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _recruitingInterviewCalculationService.CompareInterviewOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.TerminateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Outcome = new ApiRecruiting.TerminateJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "TerminateJobOpening", (request.Reason ?? ""), token);
        return response;
    }
}
