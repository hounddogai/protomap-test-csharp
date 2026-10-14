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
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Runs the long-lived workflow that takes an onboarding task from start to finish.
/// </summary>
public sealed class OnboardingTaskWorkflowEndpoint : ApiOnboarding.OnboardingTaskWorkflowService.OnboardingTaskWorkflowServiceBase
{
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDrivers.DriverService.DriverServiceClient _driversDriverService;
    private readonly ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient _learningCertificationWorkflowService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ILogger<OnboardingTaskWorkflowEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient _onboardingEquipmentApprovalService;
    private readonly ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient _onboardingEquipmentCalculationService;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient _onboardingOnboardingTaskApprovalService;
    private readonly ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient _onboardingOnboardingTaskQueryService;
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _onboardingOnboardingTaskReportingService;
    private readonly IOnboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port _onboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port;
    private readonly ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient _performanceFeedbackApprovalService;
    private readonly ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient _prescriptionsPrescriptionCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiOnboarding.EscalateEquipmentRequest, CallOptions, AsyncUnaryCall<ApiOnboarding.EscalateEquipmentResponse>> _sendRetryOnboardingTaskWorkflow2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;

    public OnboardingTaskWorkflowEndpoint(
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDrivers.DriverService.DriverServiceClient driversDriverService,
        ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient learningCertificationWorkflowService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ILogger<OnboardingTaskWorkflowEndpoint> logger,
        ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient onboardingEquipmentApprovalService,
        ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient onboardingEquipmentCalculationService,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient onboardingOnboardingTaskApprovalService,
        ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient onboardingOnboardingTaskQueryService,
        ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient onboardingOnboardingTaskReportingService,
        IOnboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port onboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0PortAdapter,
        ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient performanceFeedbackApprovalService,
        ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient prescriptionsPrescriptionCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService)
    {
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _driversDriverService = driversDriverService;
        _learningCertificationWorkflowService = learningCertificationWorkflowService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _logger = logger;
        _onboardingEquipmentApprovalService = onboardingEquipmentApprovalService;
        _onboardingEquipmentCalculationService = onboardingEquipmentCalculationService;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _onboardingOnboardingTaskApprovalService = onboardingOnboardingTaskApprovalService;
        _onboardingOnboardingTaskQueryService = onboardingOnboardingTaskQueryService;
        _onboardingOnboardingTaskReportingService = onboardingOnboardingTaskReportingService;
        _onboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port = onboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0PortAdapter;
        _performanceFeedbackApprovalService = performanceFeedbackApprovalService;
        _prescriptionsPrescriptionCalculationService = prescriptionsPrescriptionCalculationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _travelItineraryReportingService = travelItineraryReportingService;
        _sendRetryOnboardingTaskWorkflow2 = _onboardingEquipmentApprovalService.EscalateEquipmentAsync;
    }
    /// <summary>
    /// Advances an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.AdvanceOnboardingTaskWorkflowResponse> AdvanceOnboardingTaskWorkflow(ApiOnboarding.AdvanceOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input0 = new ApiLicensing.StartEntitlementWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Block = new ApiLicensing.EntitlementBlockAction
                    {
                        NotifyEmail = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc0 = _licensingEntitlementWorkflowService.StartEntitlementWorkflowAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLicensing.StartEntitlementWorkflowResponse();
        var profile1 = new MappingProfile<ApiOnboarding.AdvanceOnboardingTaskWorkflowRequest, ApiOnboarding.EstimateEquipmentRequest>(value => new ApiOnboarding.EstimateEquipmentRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _onboardingEquipmentCalculationService.EstimateEquipmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.AdvanceOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.Equipment?.Owner?.Email ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Subject = (request.OnboardingTask?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "AdvanceOnboardingTaskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.CancelOnboardingTaskWorkflowResponse> CancelOnboardingTaskWorkflow(ApiOnboarding.CancelOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.StartManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.StartManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.OnboardingTask?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _devicesManagedDeviceWorkflowService.StartManagedDeviceWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.CountItineraryTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.CountItineraryTotalsRequest.Types.Filter
            {
                Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _travelItineraryReportingService.CountItineraryTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.CancelOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = (request.OnboardingTask?.Assignee ?? new ApiIdentity.UserRef()),
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.CancelOnboardingTaskWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "CancelOnboardingTaskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.InspectOnboardingTaskWorkflowResponse> InspectOnboardingTaskWorkflow(ApiOnboarding.InspectOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "InspectOnboardingTaskWorkflow", "accepted", token);
        var input0 = new ApiOnboarding.SuggestOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.SuggestOnboardingTasksRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OnboardingTaskId ?? ""),
            },
        };
        var reply0 = await SendInspectOnboardingTaskWorkflow0Async(input0, token);
        var response = new ApiOnboarding.InspectOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists onboarding tasks a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListOnboardingTaskWorkflowsResponse> ListOnboardingTaskWorkflows(ApiOnboarding.ListOnboardingTaskWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListOnboardingTaskWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "ListOnboardingTaskWorkflows", "accepted", token);
        var input0 = new ApiPerformance.RecallFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.RecallFeedbackRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Source = (request.Filter?.OwnerId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _performanceFeedbackApprovalService.RecallFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ListOnboardingTaskWorkflowsResponse
        {
            OnboardingTasks = { new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Feedback?.Subject?.DisplayName ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ListOnboardingTaskWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.PauseOnboardingTaskWorkflowResponse> PauseOnboardingTaskWorkflow(ApiOnboarding.PauseOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPrescriptions.RecalculatePrescriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Prescription = new ApiPrescriptions.Prescription
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingTask?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.OnboardingTask?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiPatients.PatientRef
                {
                    Email = (request.OnboardingTask?.Requester?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiPrescriptions.RecalculatePrescriptionsRequest>();
        using var rpc0 = _prescriptionsPrescriptionCalculationService.RecalculatePrescriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.RecallEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.RecallEquipmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.OnboardingTask?.Assignee?.DisplayName ?? ""),
                },
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Prescription?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiOnboarding.RecallEquipmentRequest>(json1);
        using var rpc1 = _onboardingEquipmentApprovalService.RecallEquipmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.PauseOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Prescription?.Audit?.UpdatedBy ?? ""),
                },
                CustomFields = { (request.OnboardingTask?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Equipment?.Owner?.Email ?? ""),
                },
                Tags = { (reply1.Equipment?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "PauseOnboardingTaskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ResumeOnboardingTaskWorkflowResponse> ResumeOnboardingTaskWorkflow(ApiOnboarding.ResumeOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.AdvanceCertificationWorkflowRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiLearning.AdvanceCertificationWorkflowRequest.Types.Details
        {
            Notify = new ApiCommon.ContactInfo
            {
                Email = (request.OnboardingTask?.Requester?.Email ?? ""),
                PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
            },
        };
        using var rpc0 = _learningCertificationWorkflowService.AdvanceCertificationWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ShareOnboardingTaskReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ShareOnboardingTaskReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.OnboardingTask?.Requester?.Email ?? ""),
                    Name = (request.OnboardingTask?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.OnboardingTask?.Requester?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingTaskReportingService.ShareOnboardingTaskReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiSearch.CountSearchIndexesResponse();
        foreach (var item in (request.OnboardingTask?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskStatusChange>()))
        {
            var input2 = new ApiSearch.CountSearchIndexesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiSearch.CountSearchIndexesRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                },
            };
            using var rpc2 = _searchSearchIndexQueryService.CountSearchIndexesAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiOnboarding.ResumeOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Certification?.Counterparty?.DisplayName ?? ""),
                },
                OnboardingTaskId = (reply1.OnboardingTask?.OnboardingTaskId ?? ""),
                Priority = (request.OnboardingTask?.Priority ?? 0),
                Status = (request.OnboardingTask?.Status ?? (ApiOnboarding.OnboardingTaskStatus)0),
                Tags = { (reply1.OnboardingTask?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ResumeOnboardingTaskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.RetryOnboardingTaskWorkflowResponse> RetryOnboardingTaskWorkflow(ApiOnboarding.RetryOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.EscalateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.EscalateFeedbackRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.OnboardingTask ?? new ApiOnboarding.OnboardingTask()).OriginCase switch
                    {
                        ApiOnboarding.OnboardingTask.OriginOneofCase.Customer => (request.OnboardingTask?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Details?.Notify?.Email ?? ""),
                    UserId = (request.OnboardingTask?.Assignee?.UserId ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.OnboardingTask?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _performanceFeedbackApprovalService.EscalateFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ExplainOnboardingPlanCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExplainOnboardingPlanCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _onboardingOnboardingPlanCalculationService.ExplainOnboardingPlanCalculationAsync);
        var input2 = new ApiOnboarding.EscalateEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.EscalateEquipmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.OnboardingTask?.Assignee?.Email ?? ""),
                },
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.OnboardingTask?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _sendRetryOnboardingTaskWorkflow2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.RecallEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RecallEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var named3 = _registry.Get<ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient>("acme.licensing.v1.EntitlementApprovalService");
        using var rpc3 = named3.RecallEntitlementAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.RetryOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.Feedback?.Subject?.Email ?? ""),
                    UserId = (reply2.Equipment?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Equipment?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Status = (request.OnboardingTask?.Status ?? (ApiOnboarding.OnboardingTaskStatus)0),
            },
            Outcome = new ApiOnboarding.RetryOnboardingTaskWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "RetryOnboardingTaskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.SignalOnboardingTaskWorkflowResponse> SignalOnboardingTaskWorkflow(ApiOnboarding.SignalOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input0 = new ApiOnboarding.EscalateEquipmentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiOnboarding.EscalateEquipmentRequest.Types.Details
                {
                    ApproverEmail = (item.AuthorEmail ?? ""),
                    Comment = (item.CommentId ?? ""),
                },
                Equipment = new ApiOnboarding.Equipment
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc0 = _onboardingEquipmentApprovalService.EscalateEquipmentAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiOnboarding.EscalateEquipmentResponse();
        var input1 = new ApiSearch.ResolveSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command1 = new DispatchCommand<ApiSearch.ResolveSearchIndexesRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _searchSearchIndexQueryService.ResolveSearchIndexesAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDrivers.UpdateDriverRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Driver = new ApiDrivers.Driver
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (reply0.Equipment?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _driversDriverService.UpdateDriverAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input3 = new ApiOnboarding.SubmitEquipmentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiOnboarding.SubmitEquipmentRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                    Comment = (item.CommentId ?? ""),
                },
                Equipment = new ApiOnboarding.Equipment
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc3 = _onboardingEquipmentApprovalService.SubmitEquipmentAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiOnboarding.SubmitEquipmentResponse();
        var response = new ApiOnboarding.SignalOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply2.Driver?.Contact?.Email ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Subject = (request.OnboardingTask?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "SignalOnboardingTaskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.StartOnboardingTaskWorkflowResponse> StartOnboardingTaskWorkflow(ApiOnboarding.StartOnboardingTaskWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartOnboardingTaskWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.StartOnboardingTaskWorkflowToRejectOnboardingTask0();
        using var rpc0 = _onboardingOnboardingTaskApprovalService.RejectOnboardingTaskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentExportRequest.Types.Filter
            {
                CreatedAfter = (reply0.OnboardingTask?.Sla?.DueAt ?? new Timestamp()),
                OwnerId = (reply0.OnboardingTask?.Customer?.EquipmentId ?? ""),
            },
        };
        using var stream1 = _documentsDocumentExportService.StreamDocumentExport(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentExport frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiOnboarding.CommentEquipmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CommentEquipmentApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.OnboardingTask?.Assignee?.Email ?? ""),
                },
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.OnboardingTask?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.OnboardingTask?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _onboardingEquipmentApprovalService.CommentEquipmentApprovalAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.StartOnboardingTaskWorkflowResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.OnboardingTask?.Audit?.CreatedBy ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Subject = (request.OnboardingTask?.Subject ?? ""),
            },
            Outcome = new ApiOnboarding.StartOnboardingTaskWorkflowResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "StartOnboardingTaskWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiOnboarding.SuggestOnboardingTasksResponse> SendInspectOnboardingTaskWorkflow0Async(ApiOnboarding.SuggestOnboardingTasksRequest request, CancellationToken token)
    {
        return await _onboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port.SendAsync(request, token);
    }
}
