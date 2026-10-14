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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Moves a knowledge article between the states of its lifecycle.
/// </summary>
public sealed class KnowledgeArticleLifecycleEndpoint : ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceBase
{
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient _itsmChangeRequestLifecycleService;
    private readonly ApiItsm.ProblemReportingService.ProblemReportingServiceClient _itsmProblemReportingService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ILogger<KnowledgeArticleLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.CheckPositionEligibilityRequest, CallOptions, AsyncUnaryCall<ApiEmployees.CheckPositionEligibilityResponse>> _sendSuspendKnowledgeArticle0;

    public KnowledgeArticleLifecycleEndpoint(
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient itsmChangeRequestLifecycleService,
        ApiItsm.ProblemReportingService.ProblemReportingServiceClient itsmProblemReportingService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ILogger<KnowledgeArticleLifecycleEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _invoker = invoker;
        _itsmChangeRequestLifecycleService = itsmChangeRequestLifecycleService;
        _itsmProblemReportingService = itsmProblemReportingService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _sendSuspendKnowledgeArticle0 = _employeesPositionValidationService.CheckPositionEligibilityAsync;
    }
    /// <summary>
    /// Activates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ActivateKnowledgeArticleResponse> ActivateKnowledgeArticle(ApiHelpdesk.ActivateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "ActivateKnowledgeArticle", "accepted", token);
        var input0 = request.ActivateKnowledgeArticleToAdvanceEntitlementWorkflow0();
        using var rpc0 = _licensingEntitlementWorkflowService.AdvanceEntitlementWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Entitlement?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Amount = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply0.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.ExplainKnowledgeArticleCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ActivateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply1.KnowledgeArticle?.Counterparty?.EmployeeId ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Outcome = new ApiHelpdesk.ActivateKnowledgeArticleResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ActivateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.GetKnowledgeArticleLifecycleResponse> GetKnowledgeArticleLifecycle(ApiHelpdesk.GetKnowledgeArticleLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetKnowledgeArticleLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "GetKnowledgeArticleLifecycle", "accepted", token);
        var response = new ApiHelpdesk.GetKnowledgeArticleLifecycleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ReinstateKnowledgeArticleResponse> ReinstateKnowledgeArticle(ApiHelpdesk.ReinstateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "ReinstateKnowledgeArticle", "accepted", token);
        var input0 = new ApiDocuments.RetrySignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetrySignatureExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureExportService.RetrySignatureExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.QuoteTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    Number = (reply0.Signature?.Subject?.Number ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiHelpdesk.QuoteTicketRequest>();
        using var rpc1 = _helpdeskTicketCalculationService.QuoteTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.ScoreKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Ticket?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                    Email = (reply1.Ticket?.Assignee?.Email ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiHelpdesk.ScoreKnowledgeArticleRequest>(json2);
        using var rpc2 = _helpdeskKnowledgeArticleValidationService.ScoreKnowledgeArticleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.ReinstateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Number = (reply2.KnowledgeArticle?.Number ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiHelpdesk.ReinstateKnowledgeArticleResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ReinstateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.RenewKnowledgeArticleResponse> RenewKnowledgeArticle(ApiHelpdesk.RenewKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewKnowledgeArticle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "RenewKnowledgeArticle", "accepted", token);
        var input0 = new ApiHelpdesk.LintEscalationsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Escalation = new ApiHelpdesk.Escalation
        {
            Assignee = new ApiIdentity.UserRef
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
            },
            Requester = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NotifyContact?.Email ?? ""),
            },
        };
        using var rpc0 = _helpdeskEscalationValidationService.LintEscalationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.EstimateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Escalation?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Escalation?.Assignee?.DisplayName ?? ""),
                    Email = (reply0.Escalation?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskSlaPolicyCalculationService.EstimateSlaPolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.RenewKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Amount = (reply1.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            Outcome = new ApiHelpdesk.RenewKnowledgeArticleResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "RenewKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ScheduleKnowledgeArticleExpiryResponse> ScheduleKnowledgeArticleExpiry(ApiHelpdesk.ScheduleKnowledgeArticleExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleKnowledgeArticleExpiry for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.StreamDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateExportRequest.Types.Filter
            {
                Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream0 = _documentsDocumentTemplateExportService.StreamDocumentTemplateExport(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateExport frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiItsm.ListProblemReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListProblemReportsRequest.Types.Filter
            {
                CreatedAfter = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmProblemReportingService.ListProblemReportsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply2 = new ApiHelpdesk.SummarizeTicketsResponse();
        foreach (var item in (request.KnowledgeArticle?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.KnowledgeArticleLine>()))
        {
            var input2 = new ApiHelpdesk.SummarizeTicketsRequest
            {
                Annotations = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.SummarizeTicketsRequest.Types.Details
                {
                    Filters = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
                Reason = (request.Reason ?? ""),
            };
            using var rpc2 = _helpdeskTicketQueryService.SummarizeTicketsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiHelpdesk.ScheduleKnowledgeArticleExpiryResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Metadata = { (request.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.KnowledgeArticle?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ScheduleKnowledgeArticleExpiry", (request.KnowledgeArticle?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.SuspendKnowledgeArticleResponse> SuspendKnowledgeArticle(ApiHelpdesk.SuspendKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendKnowledgeArticle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "SuspendKnowledgeArticle", "accepted", token);
        var input0 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CheckPositionEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendSuspendKnowledgeArticle0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListDocumentFolderConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderConflictsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Position?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        var named1 = _registry.Get<ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient>("acme.documents.v1.DocumentFolderSyncService");
        using var rpc1 = named1.ListDocumentFolderConflictsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.EscalateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EscalateManagedDeviceRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Position?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor2 = new Method<ApiDevices.EscalateManagedDeviceRequest, ApiDevices.EscalateManagedDeviceResponse>(
            MethodType.Unary, "acme.devices.v1" + ".ManagedDeviceApprovalService", "EscalateManagedDevice",
            Wire.Marshaller<ApiDevices.EscalateManagedDeviceRequest>(), Wire.Marshaller<ApiDevices.EscalateManagedDeviceResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.ExplainSlaPolicyDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainSlaPolicyDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Block = new ApiHelpdesk.SlaPolicyBlockAction
                {
                    Reason = (reply0.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Block => (reply0.Position?.Block?.Reason ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _helpdeskSlaPolicyValidationService.ExplainSlaPolicyDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.SuspendKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply3.SlaPolicy?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply2.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Outcome = new ApiHelpdesk.SuspendKnowledgeArticleResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "SuspendKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.TerminateKnowledgeArticleResponse> TerminateKnowledgeArticle(ApiHelpdesk.TerminateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "TerminateKnowledgeArticle", "accepted", token);
        var profile0 = new MappingProfile<ApiHelpdesk.TerminateKnowledgeArticleRequest, ApiHelpdesk.EstimateEscalationRequest>(value => new ApiHelpdesk.EstimateEscalationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (value.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskEscalationCalculationService.EstimateEscalationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ScheduleChangeRequestExpiryRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScheduleChangeRequestExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmChangeRequestLifecycleService.ScheduleChangeRequestExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.TerminateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Escalation?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Outcome = new ApiHelpdesk.TerminateKnowledgeArticleResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "TerminateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
}
