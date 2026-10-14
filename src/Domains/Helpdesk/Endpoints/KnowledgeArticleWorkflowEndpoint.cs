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
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Runs the long-lived workflow that takes a knowledge article from start to finish.
/// </summary>
public sealed class KnowledgeArticleWorkflowEndpoint : ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient _itsmChangeRequestApprovalService;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _itsmServiceAssetReportingService;
    private readonly ILogger<KnowledgeArticleWorkflowEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiPatients.PatientQueryService.PatientQueryServiceClient _patientsPatientQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.NormalizePositionRequest, CallOptions, AsyncUnaryCall<ApiEmployees.NormalizePositionResponse>> _sendAdvanceKnowledgeArticleWorkflow0;
    private readonly IOptions<MeshOptions> _settings;

    public KnowledgeArticleWorkflowEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient itsmChangeRequestApprovalService,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient itsmServiceAssetReportingService,
        ILogger<KnowledgeArticleWorkflowEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiPatients.PatientQueryService.PatientQueryServiceClient patientsPatientQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _itsmChangeRequestApprovalService = itsmChangeRequestApprovalService;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _itsmServiceAssetReportingService = itsmServiceAssetReportingService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _patientsPatientQueryService = patientsPatientQueryService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendAdvanceKnowledgeArticleWorkflow0 = _employeesPositionValidationService.NormalizePositionAsync;
    }
    /// <summary>
    /// Advances a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.AdvanceKnowledgeArticleWorkflowResponse> AdvanceKnowledgeArticleWorkflow(ApiHelpdesk.AdvanceKnowledgeArticleWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceKnowledgeArticleWorkflow for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _sendAdvanceKnowledgeArticleWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CountServiceAssetTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.CountServiceAssetTotalsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Position?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Position?.PositionId ?? ""),
                Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var named1 = _registry.Get<ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient>("acme.itsm.v1.ServiceAssetReportingService");
        using var rpc1 = named1.CountServiceAssetTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.KnowledgeArticle?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.KnowledgeArticleAttachment>()).Select(async item =>
        {
            var input2 = new ApiHelpdesk.PreviewKnowledgeArticleRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Attachments = { new ApiHelpdesk.KnowledgeArticleAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                        FileName = (item.FileName ?? ""),
                        SizeBytes = item.SizeBytes,
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc2 = _helpdeskKnowledgeArticleValidationService.PreviewKnowledgeArticleAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiHelpdesk.PreviewKnowledgeArticleResponse();
        var response = new ApiHelpdesk.AdvanceKnowledgeArticleWorkflowResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                IssuedAt = (request.KnowledgeArticle?.IssuedAt ?? new Timestamp()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "AdvanceKnowledgeArticleWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.CancelKnowledgeArticleWorkflowResponse> CancelKnowledgeArticleWorkflow(ApiHelpdesk.CancelKnowledgeArticleWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelKnowledgeArticleWorkflow for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.CreateEmployeeRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.CancelKnowledgeArticleWorkflowResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = (request.KnowledgeArticle?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Outcome = new ApiHelpdesk.CancelKnowledgeArticleWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "CancelKnowledgeArticleWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.InspectKnowledgeArticleWorkflowResponse> InspectKnowledgeArticleWorkflow(ApiHelpdesk.InspectKnowledgeArticleWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectKnowledgeArticleWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "InspectKnowledgeArticleWorkflow", "accepted", token);
        var response = new ApiHelpdesk.InspectKnowledgeArticleWorkflowResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists knowledge articles a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.ListKnowledgeArticleWorkflowsResponse> ListKnowledgeArticleWorkflows(ApiHelpdesk.ListKnowledgeArticleWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListKnowledgeArticleWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", "batch",
            "ListKnowledgeArticleWorkflows", "accepted", token);
        var input0 = new ApiAudit.ValidateAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ValidateAuditEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ValidateAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CompareTicketOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CompareTicketOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _helpdeskTicketCalculationService.CompareTicketOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ListKnowledgeArticleWorkflowsResponse
        {
            KnowledgeArticles = { new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply1.Ticket?.Requester?.Email ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ListKnowledgeArticleWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ResumeKnowledgeArticleWorkflowResponse> ResumeKnowledgeArticleWorkflow(ApiHelpdesk.ResumeKnowledgeArticleWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeKnowledgeArticleWorkflow for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPatients.ResolvePatientsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _patientsPatientQueryService.ResolvePatientsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ListEscalationReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ListEscalationReportsRequest.Types.Filter
            {
                CreatedAfter = (request.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.KnowledgeArticleId ?? ""),
                Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _helpdeskEscalationReportingService.ListEscalationReportsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var batch2 = (request.KnowledgeArticle?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.KnowledgeArticleLine>()).Select(async item =>
        {
            var input2 = new ApiHelpdesk.ListTicketWorkflowsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiHelpdesk.ListTicketWorkflowsRequest.Types.Filter
                {
                    CreatedAfter = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.LineId ?? ""),
                    Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc2 = _helpdeskTicketWorkflowService.ListTicketWorkflowsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiHelpdesk.ListTicketWorkflowsResponse();
        var response = new ApiHelpdesk.ResumeKnowledgeArticleWorkflowResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                DueAt = (request.KnowledgeArticle?.DueAt ?? new Timestamp()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Outcome = new ApiHelpdesk.ResumeKnowledgeArticleWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ResumeKnowledgeArticleWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.SignalKnowledgeArticleWorkflowResponse> SignalKnowledgeArticleWorkflow(ApiHelpdesk.SignalKnowledgeArticleWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalKnowledgeArticleWorkflow for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiMetrics.CountMetricSeriesesRequest>();
        using var rpc0 = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewPositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.KnowledgeArticle?.Audit?.Etag ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.PreviewPositionRequest>(json1);
        using var rpc1 = _employeesPositionValidationService.PreviewPositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.SubmitChangeRequestRequest();
        input2.ChangeRequest = new ApiItsm.ChangeRequest
        {
            Comments = { new ApiItsm.ChangeRequestComment
            {
                CreatedAt = (reply1.Position?.Audit?.CreatedAt ?? new Timestamp()),
            } },
        };
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Details = new ApiItsm.SubmitChangeRequestRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                UserId = (reply1.Position?.Owner?.UserId ?? ""),
            },
        };
        using var rpc2 = _itsmChangeRequestApprovalService.SubmitChangeRequestAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.SignalKnowledgeArticleWorkflowResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply2.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Total = (request.KnowledgeArticle?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiHelpdesk.SignalKnowledgeArticleWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "SignalKnowledgeArticleWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.StartKnowledgeArticleWorkflowResponse> StartKnowledgeArticleWorkflow(ApiHelpdesk.StartKnowledgeArticleWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartKnowledgeArticleWorkflow for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.NormalizeTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.KnowledgeArticle?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _helpdeskTicketValidationService.NormalizeTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiItsm.EscalateServiceAssetResponse();
        foreach (var item in (request.KnowledgeArticle?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.KnowledgeArticleLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiItsm.EscalateServiceAssetRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                ServiceAsset = new ApiItsm.ServiceAsset
                {
                    Components = { { "primary", new ApiItsm.ServiceAssetComponent
                    {
                        Quantity = item.Quantity,
                    } } },
                },
            };
            using var rpc1 = _itsmServiceAssetApprovalService.EscalateServiceAssetAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiHelpdesk.StartKnowledgeArticleWorkflowResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Ticket?.Audit?.Etag ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Total = (request.KnowledgeArticle?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "StartKnowledgeArticleWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
