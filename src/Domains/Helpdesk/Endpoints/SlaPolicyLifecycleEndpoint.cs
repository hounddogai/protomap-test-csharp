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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRisk = Acme.Risk.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Moves a sla policy between the states of its lifecycle.
/// </summary>
public sealed class SlaPolicyLifecycleEndpoint : ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCustomers.CustomerService.CustomerServiceClient _customersCustomerService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<SlaPolicyLifecycleEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiRisk.RiskRegisterService.RiskRegisterServiceClient _riskRiskRegisterService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ISlaPolicyLifecycleEndpointSendRenewSlaPolicy0Port _slaPolicyLifecycleEndpointSendRenewSlaPolicy0Port;

    public SlaPolicyLifecycleEndpoint(
        NotificationBus bus,
        ApiCustomers.CustomerService.CustomerServiceClient customersCustomerService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<SlaPolicyLifecycleEndpoint> logger,
        IRecordRepository repository,
        ApiRisk.RiskRegisterService.RiskRegisterServiceClient riskRiskRegisterService,
        IOptions<MeshOptions> settings,
        ISlaPolicyLifecycleEndpointSendRenewSlaPolicy0Port slaPolicyLifecycleEndpointSendRenewSlaPolicy0PortAdapter)
    {
        _bus = bus;
        _customersCustomerService = customersCustomerService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _repository = repository;
        _riskRiskRegisterService = riskRiskRegisterService;
        _settings = settings;
        _slaPolicyLifecycleEndpointSendRenewSlaPolicy0Port = slaPolicyLifecycleEndpointSendRenewSlaPolicy0PortAdapter;
    }
    /// <summary>
    /// Activates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ActivateSlaPolicyResponse> ActivateSlaPolicy(ApiHelpdesk.ActivateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateSlaPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "ActivateSlaPolicy", "accepted", token);
        var input0 = new ApiEmployees.SuggestEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.SuggestEmployeesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmployeeQueryService.SuggestEmployeesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ActivateSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.ActivateSlaPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ActivateSlaPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ExpireSlaPolicyResponse> ExpireSlaPolicy(ApiHelpdesk.ExpireSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireSlaPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "ExpireSlaPolicy", "accepted", token);
        var profile0 = new MappingProfile<ApiHelpdesk.ExpireSlaPolicyRequest, ApiDocuments.CheckDocumentEligibilityRequest>(value => new ApiDocuments.CheckDocumentEligibilityRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CheckDocumentEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (value.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (value.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (value.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _documentsDocumentValidationService.CheckDocumentEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ExpireSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.ExpireSlaPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ExpireSlaPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.GetSlaPolicyLifecycleResponse> GetSlaPolicyLifecycle(ApiHelpdesk.GetSlaPolicyLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetSlaPolicyLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "GetSlaPolicyLifecycle", "accepted", token);
        var response = new ApiHelpdesk.GetSlaPolicyLifecycleResponse
        {
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ReinstateSlaPolicyResponse> ReinstateSlaPolicy(ApiHelpdesk.ReinstateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateSlaPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "ReinstateSlaPolicy", "accepted", token);
        var input0 = new ApiLicensing.StreamLicenseAssignmentMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
                Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _licensingLicenseAssignmentReportingService.StreamLicenseAssignmentMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamLicenseAssignmentMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = request.ReinstateSlaPolicyToExplainDocumentTemplateCalculation1();
        using var rpc1 = _documentsDocumentTemplateCalculationService.ExplainDocumentTemplateCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ReinstateSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.ReinstateSlaPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ReinstateSlaPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.RenewSlaPolicyResponse> RenewSlaPolicy(ApiHelpdesk.RenewSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewSlaPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "RenewSlaPolicy", "accepted", token);
        var input0 = new ApiHelpdesk.PreviewTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.PreviewTicketRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendRenewSlaPolicy0Async(input0, token);
        var input1 = new ApiRisk.ListRiskRegistersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRisk.ListRiskRegistersRequest.Types.Filter
            {
                CreatedAfter = (reply0.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Ticket?.Assignee?.UserId ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _riskRiskRegisterService.ListRiskRegistersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.RenewSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.RenewSlaPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Ticket?.Assignee?.UserId ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "RenewSlaPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ScheduleSlaPolicyExpiryResponse> ScheduleSlaPolicyExpiry(ApiHelpdesk.ScheduleSlaPolicyExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleSlaPolicyExpiry for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.EstimateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.EstimateTicketRequest>();
        using var rpc0 = _helpdeskTicketCalculationService.EstimateTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ScheduleSlaPolicyExpiryResponse
        {
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Ticket?.Audit?.Etag ?? ""),
                },
                Parameters = { (request.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ScheduleSlaPolicyExpiry", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.SuspendSlaPolicyResponse> SuspendSlaPolicy(ApiHelpdesk.SuspendSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendSlaPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "SuspendSlaPolicy", "accepted", token);
        var input0 = new ApiHelpdesk.ShareEscalationReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ShareEscalationReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _helpdeskEscalationReportingService.ShareEscalationReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCustomers.ListCustomersRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiCustomers.ListCustomersRequest.Types.Filter
        {
            CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
        };
        using var rpc1 = _customersCustomerService.ListCustomersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply0.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.SuspendSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.SuspendSlaPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Priority = (reply0.Escalation?.Customer?.Priority ?? 0),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "SuspendSlaPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.TerminateSlaPolicyResponse> TerminateSlaPolicy(ApiHelpdesk.TerminateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateSlaPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "TerminateSlaPolicy", "accepted", token);
        var input0 = new ApiLicensing.EscalateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EscalateLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentApprovalService.EscalateLicenseAssignmentAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.EscalateLicenseAssignmentRequest>("acme.helpdesk.v1.SlaPolicyLifecycleService/TerminateSlaPolicy/0", input0.Clone()), token);
        var response = new ApiHelpdesk.TerminateSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.TerminateSlaPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "TerminateSlaPolicy", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiHelpdesk.PreviewTicketResponse> SendRenewSlaPolicy0Async(ApiHelpdesk.PreviewTicketRequest request, CancellationToken token)
    {
        return await _slaPolicyLifecycleEndpointSendRenewSlaPolicy0Port.SendAsync(request, token);
    }
}
