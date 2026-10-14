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
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Runs the long-lived workflow that takes a compliance scan from start to finish.
/// </summary>
public sealed class ComplianceScanWorkflowEndpoint : ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceBase
{
    private readonly ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient _devicesComplianceScanApprovalService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient _devicesComplianceScanLifecycleService;
    private readonly ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient _devicesDevicePolicyApprovalService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ILogger<ComplianceScanWorkflowEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient _recommendationsRecommendationSetService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public ComplianceScanWorkflowEndpoint(
        ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient devicesComplianceScanApprovalService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient devicesComplianceScanLifecycleService,
        ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient devicesDevicePolicyApprovalService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        CallInvoker invoker,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ILogger<ComplianceScanWorkflowEndpoint> logger,
        RetryPolicy policy,
        ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient recommendationsRecommendationSetService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _devicesComplianceScanApprovalService = devicesComplianceScanApprovalService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesComplianceScanLifecycleService = devicesComplianceScanLifecycleService;
        _devicesDevicePolicyApprovalService = devicesDevicePolicyApprovalService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _invoker = invoker;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _logger = logger;
        _policy = policy;
        _recommendationsRecommendationSetService = recommendationsRecommendationSetService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _registry = registry;
        _repository = repository;
    }
    /// <summary>
    /// Advances a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.AdvanceComplianceScanWorkflowResponse> AdvanceComplianceScanWorkflow(ApiDevices.AdvanceComplianceScanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceComplianceScanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.EscalateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EscalateSoftwareLicenseRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingSoftwareLicenseApprovalService.EscalateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.PreviewOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiRecruiting.OfferBankSettlement
                {
                    Iban = (reply0.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (reply0.SoftwareLicense?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingOfferValidationService.PreviewOfferAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.CancelEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CancelEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (reply0.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Offer?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiLicensing.CancelEntitlementWorkflowRequest>(json2);
        using var rpc2 = _licensingEntitlementWorkflowService.CancelEntitlementWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.AdvanceComplianceScanWorkflowResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Attributes = { (request.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
            Outcome = new ApiDevices.AdvanceComplianceScanWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "AdvanceComplianceScanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.InspectComplianceScanWorkflowResponse> InspectComplianceScanWorkflow(ApiDevices.InspectComplianceScanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectComplianceScanWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "InspectComplianceScanWorkflow", "accepted", token);
        var input0 = new ApiDevices.ExplainComplianceScanCalculationRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainComplianceScanCalculationRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _devicesComplianceScanCalculationService.ExplainComplianceScanCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.InspectComplianceScanWorkflowResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists compliance scans a page at a time.
    /// </summary>
    public override async Task<ApiDevices.ListComplianceScanWorkflowsResponse> ListComplianceScanWorkflows(ApiDevices.ListComplianceScanWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListComplianceScanWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", "batch",
            "ListComplianceScanWorkflows", "accepted", token);
        var input0 = new ApiDevices.PauseDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PauseDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _devicesDevicePolicyWorkflowService.PauseDevicePolicyWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ListComplianceScanWorkflowsResponse
        {
            ComplianceScans = { new ApiDevices.ComplianceScan
            {
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ListComplianceScanWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.PauseComplianceScanWorkflowResponse> PauseComplianceScanWorkflow(ApiDevices.PauseComplianceScanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseComplianceScanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDevices.ApproveComplianceScanResponse();
        foreach (var item in (request.ComplianceScan?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiDevices.ComplianceScanMeasurement>()))
        {
            var input0 = new ApiDevices.ApproveComplianceScanRequest
            {
                ComplianceScan = new ApiDevices.ComplianceScan
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDevices.ComplianceScanMeasurement
                    {
                        Metric = (item.Metric ?? ""),
                        Unit = (item.Unit ?? ""),
                        Value = item.Value,
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _devicesComplianceScanApprovalService.ApproveComplianceScanAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiDevices.ActivateComplianceScanRequest
        {
            ComplianceScanId = (request.ComplianceScan?.ComplianceScanId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ActivateComplianceScanRequest.Types.Details
            {
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _devicesComplianceScanLifecycleService.ActivateComplianceScanAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.PauseComplianceScanWorkflowResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Source = (request.ComplianceScan?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.ComplianceScan?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "PauseComplianceScanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ResumeComplianceScanWorkflowResponse> ResumeComplianceScanWorkflow(ApiDevices.ResumeComplianceScanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeComplianceScanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ApproveDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ApproveDevicePolicyRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _devicesDevicePolicyApprovalService.ApproveDevicePolicyAsync);
        var input1 = new ApiDevices.ScheduleComplianceScanExpiryRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ScheduleComplianceScanExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanLifecycleService.ScheduleComplianceScanExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ResumeComplianceScanWorkflowResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (reply1.ComplianceScan?.ComplianceScanId ?? ""),
                Kind = (request.ComplianceScan?.Kind ?? ""),
                Sequence = (reply1.ComplianceScan?.Sequence ?? 0L),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ResumeComplianceScanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.RetryComplianceScanWorkflowResponse> RetryComplianceScanWorkflow(ApiDevices.RetryComplianceScanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryComplianceScanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExplainInterviewDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient>("acme.recruiting.v1.InterviewValidationService");
        using var rpc0 = named0.ExplainInterviewDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Interview?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Interview?.Location?.Region ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiLicensing.DelegateLicenseAssignmentApprovalRequest, ApiLicensing.DelegateLicenseAssignmentApprovalResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".LicenseAssignmentApprovalService", "DelegateLicenseAssignmentApproval",
            Wire.Marshaller<ApiLicensing.DelegateLicenseAssignmentApprovalRequest>(), Wire.Marshaller<ApiLicensing.DelegateLicenseAssignmentApprovalResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.RetryComplianceScanWorkflowResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                OccurredAt = (request.ComplianceScan?.OccurredAt ?? new Timestamp()),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiDevices.RetryComplianceScanWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "RetryComplianceScanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.StartComplianceScanWorkflowResponse> StartComplianceScanWorkflow(ApiDevices.StartComplianceScanWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartComplianceScanWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecommendations.UpdateRecommendationSetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RecommendationSet = new ApiRecommendations.RecommendationSet
            {
                Counterparty = new ApiCustomers.CustomerRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _recommendationsRecommendationSetService.UpdateRecommendationSetAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.ApproveDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyApprovalService.ApproveDevicePolicyAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiDevices.StartComplianceScanWorkflowResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Subject = (request.ComplianceScan?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "StartComplianceScanWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
