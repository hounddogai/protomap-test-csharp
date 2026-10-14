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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPayments = Acme.Payments.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRisk = Acme.Risk.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Runs the long-lived workflow that takes a benefit plan from start to finish.
/// </summary>
public sealed class BenefitPlanWorkflowEndpoint : ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceBase
{
    private readonly IBenefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port _benefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port;
    private readonly ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient _benefitsBenefitPlanQueryService;
    private readonly ApiBenefits.BenefitPlanService.BenefitPlanServiceClient _benefitsBenefitPlanService;
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _benefitsEnrollmentApprovalService;
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ILogger<BenefitPlanWorkflowEndpoint> _logger;
    private readonly ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient _paymentsPaymentCalculationService;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient _riskRiskRegisterQueryService;
    private readonly IOptions<MeshOptions> _settings;

    public BenefitPlanWorkflowEndpoint(
        IBenefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port benefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0PortAdapter,
        ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient benefitsBenefitPlanQueryService,
        ApiBenefits.BenefitPlanService.BenefitPlanServiceClient benefitsBenefitPlanService,
        ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient benefitsEnrollmentApprovalService,
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        NotificationBus bus,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ILogger<BenefitPlanWorkflowEndpoint> logger,
        ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient paymentsPaymentCalculationService,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiRisk.RiskRegisterQueryService.RiskRegisterQueryServiceClient riskRiskRegisterQueryService,
        IOptions<MeshOptions> settings)
    {
        _benefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port = benefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0PortAdapter;
        _benefitsBenefitPlanQueryService = benefitsBenefitPlanQueryService;
        _benefitsBenefitPlanService = benefitsBenefitPlanService;
        _benefitsEnrollmentApprovalService = benefitsEnrollmentApprovalService;
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _bus = bus;
        _documentsSignatureExportService = documentsSignatureExportService;
        _invoker = invoker;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _logger = logger;
        _paymentsPaymentCalculationService = paymentsPaymentCalculationService;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _registry = registry;
        _repository = repository;
        _riskRiskRegisterQueryService = riskRiskRegisterQueryService;
        _settings = settings;
    }
    /// <summary>
    /// Advances a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.AdvanceBenefitPlanWorkflowResponse> AdvanceBenefitPlanWorkflow(ApiBenefits.AdvanceBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPayments.SimulatePaymentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Payment = new ApiPayments.Payment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.BenefitPlan?.Audit?.UpdatedBy ?? ""),
                },
                Lines = { new ApiPayments.PaymentLine
                {
                    Tax = new ApiPayments.PaymentLineTax
                    {
                        Amount = (request.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
                        {
                            ApiBenefits.BenefitPlan.ActionOneofCase.Discount => (request.BenefitPlan?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _paymentsPaymentCalculationService.SimulatePaymentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CloneJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Payment?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.BenefitPlan?.Audit?.UpdatedBy ?? ""),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Amount = (request.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
                    {
                        ApiBenefits.BenefitPlan.ActionOneofCase.Discount => (request.BenefitPlan?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc1 = _recruitingJobOpeningService.CloneJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.CloneJobOpeningRequest>("acme.benefits.v1.BenefitPlanWorkflowService/AdvanceBenefitPlanWorkflow/1", input1.Clone()), token);
        var response = new ApiBenefits.AdvanceBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Description = (request.BenefitPlan?.Description ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "AdvanceBenefitPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.CancelBenefitPlanWorkflowResponse> CancelBenefitPlanWorkflow(ApiBenefits.CancelBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ShareSignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareSignatureExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureExportService.ShareSignatureExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CheckCandidateEligibilityRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.BenefitPlan?.Audit?.Etag ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiRecruiting.CheckCandidateEligibilityRequest>();
        using var rpc1 = _recruitingCandidateValidationService.CheckCandidateEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.EstimateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                    Email = (request.BenefitPlan?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventCalculationService.EstimateLifeEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiBenefits.RejectEnrollmentRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.Details = new ApiBenefits.RejectEnrollmentRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                Email = (reply1.Candidate?.Contact?.Email ?? ""),
            },
        };
        input3.Enrollment = new ApiBenefits.Enrollment
        {
            Holder = new ApiRecruiting.CandidateRef
            {
                CandidateId = (reply2.LifeEvent?.Subject?.CandidateId ?? ""),
                DisplayName = (request.BenefitPlan?.Owner?.DisplayName ?? ""),
            },
        };
        using var rpc3 = _benefitsEnrollmentApprovalService.RejectEnrollmentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.CancelBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Candidate?.Audit?.UpdatedBy ?? ""),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Version = (request.BenefitPlan?.Version ?? 0L),
            },
            Outcome = new ApiBenefits.CancelBenefitPlanWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "CancelBenefitPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.InspectBenefitPlanWorkflowResponse> InspectBenefitPlanWorkflow(ApiBenefits.InspectBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BenefitPlanId, "benefit_plan_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", request.BenefitPlanId,
            "InspectBenefitPlanWorkflow", "accepted", token);
        var input0 = new ApiBenefits.CalculateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                CorrelationId = (request.Context?.Actor?.UserId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiBenefits.LifeEventMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
                Source = (request.BenefitPlanId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.CalculateLifeEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.InspectBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists benefit plans a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.ListBenefitPlanWorkflowsResponse> ListBenefitPlanWorkflows(ApiBenefits.ListBenefitPlanWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListBenefitPlanWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "ListBenefitPlanWorkflows", "accepted", token);
        var response = new ApiBenefits.ListBenefitPlanWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "ListBenefitPlanWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.PauseBenefitPlanWorkflowResponse> PauseBenefitPlanWorkflow(ApiBenefits.PauseBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.BenefitPlan?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.BenefitPlan?.Audit?.UpdatedBy ?? ""),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Amount = (request.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
                    {
                        ApiBenefits.BenefitPlan.ActionOneofCase.Discount => (request.BenefitPlan?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc0 = _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.BatchCreateBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Record = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Block = new ApiBenefits.BenefitPlanBlockAction
                {
                    Reason = (request.BenefitPlan?.Block?.Reason ?? ""),
                },
            },
        };
        using var stream1 = _benefitsBenefitPlanService.BatchCreateBenefitPlans(cancellationToken: token);
        await stream1.RequestStream.WriteAsync(input1, token);
        await stream1.RequestStream.CompleteAsync();
        var reply1 = await stream1.ResponseAsync;
        var response = new ApiBenefits.PauseBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.JobOpening?.Audit?.CreatedBy ?? ""),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Parameters = { (request.BenefitPlan?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiBenefits.PauseBenefitPlanWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "PauseBenefitPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.ResumeBenefitPlanWorkflowResponse> ResumeBenefitPlanWorkflow(ApiBenefits.ResumeBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.DelegateServiceAssetApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.BenefitPlan?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _itsmServiceAssetApprovalService.DelegateServiceAssetApprovalAsync);
        var input1 = new ApiBenefits.CalculateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.ServiceAsset?.Owner?.DisplayName ?? ""),
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.CalculateLifeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.ResumeBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LifeEvent?.Subject?.DisplayName ?? ""),
                },
                Status = (request.BenefitPlan?.Status ?? (ApiBenefits.BenefitPlanStatus)0),
            },
            Outcome = new ApiBenefits.ResumeBenefitPlanWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "ResumeBenefitPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.RetryBenefitPlanWorkflowResponse> RetryBenefitPlanWorkflow(ApiBenefits.RetryBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SummarizeCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient>("acme.recruiting.v1.CandidateQueryService");
        using var rpc0 = named0.SummarizeCandidatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.BrowseCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.BrowseCandidatesRequest.Types.Filter
            {
                CreatedAfter = (request.BenefitPlan?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.BenefitPlan?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.BenefitPlan?.Owner?.UserId ?? ""),
            },
        };
        var descriptor1 = new Method<ApiRecruiting.BrowseCandidatesRequest, ApiRecruiting.BrowseCandidatesResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".CandidateQueryService", "BrowseCandidates",
            Wire.Marshaller<ApiRecruiting.BrowseCandidatesRequest>(), Wire.Marshaller<ApiRecruiting.BrowseCandidatesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.CloneJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var command2 = new DispatchCommand<ApiRecruiting.CloneJobOpeningRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _recruitingJobOpeningService.CloneJobOpeningAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiBenefits.RetryBenefitPlanWorkflowRequest, ApiRisk.BrowseRiskRegistersRequest>(value => new ApiRisk.BrowseRiskRegistersRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRisk.BrowseRiskRegistersRequest.Types.Filter
            {
                CreatedAfter = (value.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply2.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (reply2.JobOpening?.BillingAddress?.Region ?? ""),
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _riskRiskRegisterQueryService.BrowseRiskRegistersAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.RetryBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Priority = (request.BenefitPlan?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "RetryBenefitPlanWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.SignalBenefitPlanWorkflowResponse> SignalBenefitPlanWorkflow(ApiBenefits.SignalBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.BenefitPlan?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Reason = (request.BenefitPlan?.Block?.Reason ?? ""),
        };
        using var rpc0 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiBenefits.ResolveEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _benefitsEnrollmentQueryService.ResolveEnrollmentsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiBenefits.SignalBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Name = (request.BenefitPlan?.Name ?? ""),
            },
            Outcome = new ApiBenefits.SignalBenefitPlanWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "SignalBenefitPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.StartBenefitPlanWorkflowResponse> StartBenefitPlanWorkflow(ApiBenefits.StartBenefitPlanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartBenefitPlanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.PeekBenefitPlanRequest
        {
            BenefitPlanId = (request.BenefitPlan?.BenefitPlanId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await SendStartBenefitPlanWorkflow0Async(input0, token);
        var input1 = new ApiBenefits.ResolveBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _benefitsBenefitPlanQueryService.ResolveBenefitPlansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.StartBenefitPlanWorkflowResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Name = (request.BenefitPlan?.Name ?? ""),
                Priority = (reply0.BenefitPlan?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "StartBenefitPlanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiBenefits.PeekBenefitPlanResponse> SendStartBenefitPlanWorkflow0Async(ApiBenefits.PeekBenefitPlanRequest request, CancellationToken token)
    {
        return await _benefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port.SendAsync(request, token);
    }
}
