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
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiShows = Acme.Shows.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Computes the figures of an employment contract, such as totals, taxes, and estimates.
/// </summary>
public sealed class EmploymentContractCalculationEndpoint : ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly ApiConcessions.ConcessionStandService.ConcessionStandServiceClient _concessionsConcessionStandService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDispatch.DispatchOrderService.DispatchOrderServiceClient _dispatchDispatchOrderService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<EmploymentContractCalculationEndpoint> _logger;
    private readonly ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient _onboardingOnboardingPlanService;
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _performanceCalibrationValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftQueryService.ShiftQueryServiceClient _schedulingShiftQueryService;
    private readonly Func<ApiPerformance.LintCalibrationsRequest, CallOptions, AsyncUnaryCall<ApiPerformance.LintCalibrationsResponse>> _sendQuoteEmploymentContract0;
    private readonly ApiShows.ShowQueryService.ShowQueryServiceClient _showsShowQueryService;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;

    public EmploymentContractCalculationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        ApiConcessions.ConcessionStandService.ConcessionStandServiceClient concessionsConcessionStandService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDispatch.DispatchOrderService.DispatchOrderServiceClient dispatchDispatchOrderService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<EmploymentContractCalculationEndpoint> logger,
        ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient onboardingOnboardingPlanService,
        ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient performanceCalibrationValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.ShiftQueryService.ShiftQueryServiceClient schedulingShiftQueryService,
        ApiShows.ShowQueryService.ShowQueryServiceClient showsShowQueryService,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _concessionsConcessionStandService = concessionsConcessionStandService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _dispatchDispatchOrderService = dispatchDispatchOrderService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _invoker = invoker;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _onboardingOnboardingPlanService = onboardingOnboardingPlanService;
        _performanceCalibrationValidationService = performanceCalibrationValidationService;
        _registry = registry;
        _repository = repository;
        _schedulingShiftQueryService = schedulingShiftQueryService;
        _showsShowQueryService = showsShowQueryService;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _sendQuoteEmploymentContract0 = _performanceCalibrationValidationService.LintCalibrationsAsync;
    }
    /// <summary>
    /// Calculates an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.CalculateEmploymentContractResponse> CalculateEmploymentContract(ApiEmployees.CalculateEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ExplainComplianceScanCalculationRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.ExplainComplianceScanCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.CalculateEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Notes = (request.EmploymentContract?.Notes ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ComplianceScan?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "CalculateEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.CompareEmploymentContractOptionsResponse> CompareEmploymentContractOptions(ApiEmployees.CompareEmploymentContractOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareEmploymentContractOptions for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.ExplainAuditEventDecisionRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                    Number = (request.EmploymentContract?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ExplainAuditEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ExplainAuditEventDecisionAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RetryDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Bank = new ApiDocuments.DocumentBankSettlement
                {
                    AccountHolder = (request.EmploymentContract?.Bank?.AccountHolder ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentExportService.RetryDocumentExportAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiEmployees.CompareEmploymentContractOptionsResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Status = (request.EmploymentContract?.Status ?? (ApiEmployees.EmploymentContractStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "CompareEmploymentContractOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.EstimateEmploymentContractResponse> EstimateEmploymentContract(ApiEmployees.EstimateEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.ValidateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.EmploymentContract?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.ValidateAccessPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiAuthorization.ValidateAccessPolicyRequest>();
        using var rpc0 = _authorizationAccessPolicyValidationService.ValidateAccessPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.LintAuditEventsRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (reply0.AccessPolicy?.Owner?.DisplayName ?? ""),
                    Number = (request.EmploymentContract?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.LintAuditEventsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.AccessPolicy?.Owner?.Email ?? ""),
                },
            },
            Reason = (reply0.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
            {
                ApiAuthorization.AccessPolicy.ActionOneofCase.Block => (reply0.AccessPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _auditAuditEventValidationService.LintAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ResolveDocumentTemplateConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Bank = new ApiDocuments.DocumentTemplateBankSettlement
                {
                    Bic = (request.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Bank => (request.EmploymentContract?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (request.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateSyncService.ResolveDocumentTemplateConflictAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.EstimateEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.AccessPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.AccessPolicy?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.DocumentTemplate?.BillingAddress?.Recipient ?? ""),
                    Region = (reply2.DocumentTemplate?.BillingAddress?.Region ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Status = (request.EmploymentContract?.Status ?? (ApiEmployees.EmploymentContractStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "EstimateEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.ExplainEmploymentContractCalculationResponse> ExplainEmploymentContractCalculation(ApiEmployees.ExplainEmploymentContractCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainEmploymentContractCalculation for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDispatch.UpdateDispatchOrderResponse();
        foreach (var item in (request.EmploymentContract?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.EmploymentContractLine>()))
        {
            var input0 = new ApiDispatch.UpdateDispatchOrderRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DispatchOrder = new ApiDispatch.DispatchOrder
                {
                    Lines = { new ApiDispatch.DispatchOrderLine
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                        Description = (item.Description ?? ""),
                        Tax = new ApiDispatch.DispatchOrderLineTax
                        {
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc0 = _dispatchDispatchOrderService.UpdateDispatchOrderAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiPerformance.ValidateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.EmploymentContract?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ValidateCalibrationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _performanceCalibrationValidationService.ValidateCalibrationAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.SimulateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.DispatchOrder?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.ExplainEmploymentContractCalculationResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Calibration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Metadata = { (request.EmploymentContract?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "ExplainEmploymentContractCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.GetEmploymentContractRatesResponse> GetEmploymentContractRates(ApiEmployees.GetEmploymentContractRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmploymentContractId, "employment_contract_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetEmploymentContractRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract", request.EmploymentContractId,
            "GetEmploymentContractRates", "accepted", token);
        var input0 = new ApiLedger.ExplainLedgerAccountDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.ExplainLedgerAccountDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                ExternalAccountId = (request.EmploymentContractId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _ledgerLedgerAccountValidationService.ExplainLedgerAccountDecisionAsync);
        var response = new ApiEmployees.GetEmploymentContractRatesResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.LedgerAccount?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.QuoteEmploymentContractResponse> QuoteEmploymentContract(ApiEmployees.QuoteEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.LintCalibrationsRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendQuoteEmploymentContract0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.SummarizeShiftsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.SummarizeShiftsRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var named1 = _registry.Get<ApiScheduling.ShiftQueryService.ShiftQueryServiceClient>("acme.scheduling.v1.ShiftQueryService");
        using var rpc1 = named1.SummarizeShiftsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLedger.LintLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiLedger.LintLedgerAccountsRequest, ApiLedger.LintLedgerAccountsResponse>(
            MethodType.Unary, "acme.ledger.v1" + ".LedgerAccountValidationService", "LintLedgerAccounts",
            Wire.Marshaller<ApiLedger.LintLedgerAccountsRequest>(), Wire.Marshaller<ApiLedger.LintLedgerAccountsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiOnboarding.UpdateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Calibration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Bank = new ApiOnboarding.OnboardingPlanBankSettlement
                {
                    Iban = (request.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Bank => (request.EmploymentContract?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc3 = _onboardingOnboardingPlanService.UpdateOnboardingPlanAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.QuoteEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Calibration?.Audit?.UpdatedBy ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Total = (request.EmploymentContract?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.EmploymentContract?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "QuoteEmploymentContract", (request.EmploymentContract?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.RecalculateEmploymentContractsResponse> RecalculateEmploymentContracts(ApiEmployees.RecalculateEmploymentContractsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateEmploymentContracts for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiConcessions.WatchConcessionStandsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiConcessions.WatchConcessionStandsRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
            },
        };
        using var stream0 = _concessionsConcessionStandService.WatchConcessionStands(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchConcessionStands frame {Type}", change0.GetType().Name);
        }
        var reply1 = new ApiDocuments.SyncDocumentTemplatesResponse();
        foreach (var item in (request.EmploymentContract?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.EmploymentContractAttachment>()))
        {
            var input1 = new ApiDocuments.SyncDocumentTemplatesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.SyncDocumentTemplatesRequest.Types.Details
                {
                    Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                },
                DocumentTemplate = new ApiDocuments.DocumentTemplate
                {
                    Attachments = { new ApiDocuments.DocumentTemplateAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc1 = _documentsDocumentTemplateSyncService.SyncDocumentTemplatesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = request.RecalculateEmploymentContractsToSimulateBuilding2();
        using var rpc2 = _workplacesBuildingCalculationService.SimulateBuildingAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.RecalculateEmploymentContractsResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.Building?.Location?.PostalCode ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Notes = (request.EmploymentContract?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "RecalculateEmploymentContracts", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.SimulateEmploymentContractResponse> SimulateEmploymentContract(ApiEmployees.SimulateEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.EmploymentContract?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.EmploymentContractLine>()).Select(async item =>
        {
            var input0 = new ApiBenefits.ExplainLifeEventCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LifeEvent = new ApiBenefits.LifeEvent
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiRecruiting.CandidateRef
                    {
                        Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
                    },
                },
            };
            using var rpc0 = _benefitsLifeEventCalculationService.ExplainLifeEventCalculationAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiBenefits.ExplainLifeEventCalculationResponse();
        var input1 = new ApiShows.SuggestShowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiShows.SuggestShowsRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _showsShowQueryService.SuggestShowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiAudit.ExplainAuditEventDecisionRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    Number = (request.EmploymentContract?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ExplainAuditEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _auditAuditEventValidationService.ExplainAuditEventDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiOnboarding.SearchOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.SearchOnboardingPlansRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply2.AuditEvent?.AuditEventId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiOnboarding.SearchOnboardingPlansRequest>(json3);
        using var rpc3 = _onboardingOnboardingPlanService.SearchOnboardingPlansAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.SimulateEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                IssuedAt = (request.EmploymentContract?.IssuedAt ?? new Timestamp()),
                Number = (reply2.AuditEvent?.Subject?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "SimulateEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
}
