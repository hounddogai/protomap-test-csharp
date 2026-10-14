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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Routes onboarding tasks through approval, with escalation and delegation.
/// </summary>
public sealed class OnboardingTaskApprovalEndpoint : ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<OnboardingTaskApprovalEndpoint> _logger;
    private readonly ApiLoyalty.LoyaltyMemberCalculationService.LoyaltyMemberCalculationServiceClient _loyaltyLoyaltyMemberCalculationService;
    private readonly ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient _onboardingEquipmentReportingService;
    private readonly ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient _onboardingOnboardingPlanApprovalService;
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _onboardingOnboardingTaskReportingService;
    private readonly ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient _onboardingOnboardingTaskValidationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _travelTravelBookingReportingService;
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _workplacesVisitorPassSyncService;

    public OnboardingTaskApprovalEndpoint(
        NotificationBus bus,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<OnboardingTaskApprovalEndpoint> logger,
        ApiLoyalty.LoyaltyMemberCalculationService.LoyaltyMemberCalculationServiceClient loyaltyLoyaltyMemberCalculationService,
        ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient onboardingEquipmentReportingService,
        ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient onboardingOnboardingPlanApprovalService,
        ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient onboardingOnboardingTaskReportingService,
        ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient onboardingOnboardingTaskValidationService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient travelTravelBookingReportingService,
        ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient workplacesVisitorPassSyncService)
    {
        _bus = bus;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _loyaltyLoyaltyMemberCalculationService = loyaltyLoyaltyMemberCalculationService;
        _onboardingEquipmentReportingService = onboardingEquipmentReportingService;
        _onboardingOnboardingPlanApprovalService = onboardingOnboardingPlanApprovalService;
        _onboardingOnboardingTaskReportingService = onboardingOnboardingTaskReportingService;
        _onboardingOnboardingTaskValidationService = onboardingOnboardingTaskValidationService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _travelTravelBookingReportingService = travelTravelBookingReportingService;
        _workplacesVisitorPassSyncService = workplacesVisitorPassSyncService;
    }
    /// <summary>
    /// Approves an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ApproveOnboardingTaskResponse> ApproveOnboardingTask(ApiOnboarding.ApproveOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.PushVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.PushVisitorPassRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _workplacesVisitorPassSyncService.PushVisitorPassAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ApproveOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Comments = { new ApiOnboarding.OnboardingTaskComment
                {
                    CreatedAt = (reply0.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.ApproveOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ApproveOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.CommentOnboardingTaskApprovalResponse> CommentOnboardingTaskApproval(ApiOnboarding.CommentOnboardingTaskApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentOnboardingTaskApproval for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.GenerateRegistrationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.GenerateRegistrationReportRequest.Types.Details
            {
                Filters = { (request.OnboardingTask?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationReportingService.GenerateRegistrationReportAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.CommentOnboardingTaskApprovalResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.CommentOnboardingTaskApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingTask?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "CommentOnboardingTaskApproval", (request.OnboardingTask?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.DelegateOnboardingTaskApprovalResponse> DelegateOnboardingTaskApproval(ApiOnboarding.DelegateOnboardingTaskApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateOnboardingTaskApproval for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.OnboardingTask?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskComment>()).Select(async item =>
        {
            var input0 = new ApiLedger.CloneLedgerAccountRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LedgerAccount = new ApiLedger.LedgerAccount
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                        PreferredChannel = (request.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                    },
                    Holder = new ApiLedger.PostingBatchRef
                    {
                        DisplayName = (request.OnboardingTask?.Assignee?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc0 = _ledgerLedgerAccountService.CloneLedgerAccountAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLedger.CloneLedgerAccountResponse();
        var input1 = new ApiLoyalty.ExplainLoyaltyMemberCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LoyaltyMember = new ApiLoyalty.LoyaltyMember
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingTask?.Audit?.Etag ?? ""),
                },
                Bank = new ApiLoyalty.LoyaltyMemberBankFunding
                {
                    Bic = (reply0.LedgerAccount ?? new ApiLedger.LedgerAccount()).PaymentCase switch
                    {
                        ApiLedger.LedgerAccount.FundingOneofCase.Bank => (reply0.LedgerAccount?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.OnboardingTask?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _loyaltyLoyaltyMemberCalculationService.ExplainLoyaltyMemberCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.DelegateOnboardingTaskApprovalResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.LoyaltyMember?.Audit?.CreatedAt ?? new Timestamp()),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply1.LoyaltyMember?.Contact?.PreferredChannel ?? ""),
                },
            },
            Outcome = new ApiOnboarding.DelegateOnboardingTaskApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "DelegateOnboardingTaskApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.EscalateOnboardingTaskResponse> EscalateOnboardingTask(ApiOnboarding.EscalateOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ExplainOnboardingTaskDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingTask?.Audit?.Etag ?? ""),
                },
                Customer = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.OnboardingTask ?? new ApiOnboarding.OnboardingTask()).OriginCase switch
                    {
                        ApiOnboarding.OnboardingTask.OriginOneofCase.Customer => (request.OnboardingTask?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingTaskValidationService.ExplainOnboardingTaskDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ShareRegistrationReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareRegistrationReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.OnboardingTask?.Assignee?.Email ?? ""),
                    PreferredChannel = (request.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationReportingService.ShareRegistrationReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.DeleteOnboardingTaskReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Etag = (reply0.OnboardingTask?.Audit?.Etag ?? ""),
            OnboardingTaskId = (request.OnboardingTask?.OnboardingTaskId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var command2 = new DispatchCommand<ApiOnboarding.DeleteOnboardingTaskReportRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _onboardingOnboardingTaskReportingService.DeleteOnboardingTaskReportAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.EscalateOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Registration?.Audit?.CreatedBy ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Requester = (request.OnboardingTask?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Outcome = new ApiOnboarding.EscalateOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (reply2.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingTask?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "EscalateOnboardingTask", (request.OnboardingTask?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingTaskApprovalStatusResponse> GetOnboardingTaskApprovalStatus(ApiOnboarding.GetOnboardingTaskApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetOnboardingTaskApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "GetOnboardingTaskApprovalStatus", "accepted", token);
        var profile0 = new MappingProfile<ApiOnboarding.GetOnboardingTaskApprovalStatusRequest, ApiLedger.BrowseLedgerAccountsRequest>(value => new ApiLedger.BrowseLedgerAccountsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.BrowseLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                Labels = { (value.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.OnboardingTaskId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _ledgerLedgerAccountQueryService.BrowseLedgerAccountsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.CountEquipmentTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.CountEquipmentTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OnboardingTaskId ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _onboardingEquipmentReportingService.CountEquipmentTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.GetOnboardingTaskApprovalStatusResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists onboarding tasks a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListPendingOnboardingTasksResponse> ListPendingOnboardingTasks(ApiOnboarding.ListPendingOnboardingTasksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingOnboardingTasks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "ListPendingOnboardingTasks", "accepted", token);
        var input0 = new ApiTravel.StreamTravelBookingMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.StreamTravelBookingMetricsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream0 = _travelTravelBookingReportingService.StreamTravelBookingMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamTravelBookingMetrics frame {Type}", change0.GetType().Name);
        }
        var response = new ApiOnboarding.ListPendingOnboardingTasksResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ListPendingOnboardingTasks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.RecallOnboardingTaskResponse> RecallOnboardingTask(ApiOnboarding.RecallOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RecordSignatureVersionRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordSignatureVersionRequest.Types.Details
            {
                AuthorEmail = (request.Details?.Approver?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.RecordSignatureVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.RecordSignatureVersionRequest>("acme.onboarding.v1.OnboardingTaskApprovalService/RecallOnboardingTask/0", input0.Clone()), token);
        var response = new ApiOnboarding.RecallOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.RecallOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingTask?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "RecallOnboardingTask", (request.OnboardingTask?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.RejectOnboardingTaskResponse> RejectOnboardingTask(ApiOnboarding.RejectOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.GenerateLicenseAssignmentReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.GenerateLicenseAssignmentReportRequest.Types.Details
            {
                Filters = { (request.OnboardingTask?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingLicenseAssignmentReportingService.GenerateLicenseAssignmentReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ListPendingOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.ListPendingOnboardingPlansRequest.Types.Filter
            {
                CreatedAfter = (request.OnboardingTask?.Sla?.DueAt ?? new Timestamp()),
                OwnerId = (request.OnboardingTask?.Sla?.PolicyId ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiOnboarding.ListPendingOnboardingPlansRequest>();
        using var rpc1 = _onboardingOnboardingPlanApprovalService.ListPendingOnboardingPlansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.RejectOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Priority = (request.OnboardingTask?.Priority ?? 0),
            },
            Outcome = new ApiOnboarding.RejectOnboardingTaskResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingTask?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "RejectOnboardingTask", (request.OnboardingTask?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.SubmitOnboardingTaskResponse> SubmitOnboardingTask(ApiOnboarding.SubmitOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitOnboardingTask for {Tenant}", requestContext.TenantId);
        if (request.OnboardingTask is { } record)
        {
            var snapshot = OnboardingTaskMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.OnboardingTask?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiOnboarding.OnboardingTaskStatusChange>()))
        {
            var input0 = new ApiOnboarding.StreamOnboardingTaskMetricsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiOnboarding.StreamOnboardingTaskMetricsRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                },
                Subscriber = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    Name = (request.OnboardingTask?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            };
            using var stream0 = _onboardingOnboardingTaskReportingService.StreamOnboardingTaskMetrics(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received StreamOnboardingTaskMetrics frame {Type}", change0.GetType().Name);
            }
        }
        var response = new ApiOnboarding.SubmitOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = (request.OnboardingTask?.Assignee ?? new ApiIdentity.UserRef()),
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "SubmitOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
}
