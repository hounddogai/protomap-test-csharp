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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Routes tickets through approval, with escalation and delegation.
/// </summary>
public sealed class TicketApprovalEndpoint : ApiHelpdesk.TicketApprovalService.TicketApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient _helpdeskKnowledgeArticleQueryService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly ApiHelpdesk.TicketService.TicketServiceClient _helpdeskTicketService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient _itsmChangeRequestLifecycleService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<TicketApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRatelimiting.RateLimitService.RateLimitServiceClient _ratelimitingRateLimitService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.ScheduleChangeRequestExpiryRequest, CallOptions, AsyncUnaryCall<ApiItsm.ScheduleChangeRequestExpiryResponse>> _sendApproveTicket2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ITicketApprovalEndpointSendGetTicketApprovalStatus0Port _ticketApprovalEndpointSendGetTicketApprovalStatus0Port;

    public TicketApprovalEndpoint(
        NotificationBus bus,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient helpdeskKnowledgeArticleQueryService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        ApiHelpdesk.TicketService.TicketServiceClient helpdeskTicketService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient itsmChangeRequestLifecycleService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<TicketApprovalEndpoint> logger,
        RetryPolicy policy,
        ApiRatelimiting.RateLimitService.RateLimitServiceClient ratelimitingRateLimitService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ITicketApprovalEndpointSendGetTicketApprovalStatus0Port ticketApprovalEndpointSendGetTicketApprovalStatus0PortAdapter)
    {
        _bus = bus;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskKnowledgeArticleQueryService = helpdeskKnowledgeArticleQueryService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _helpdeskTicketService = helpdeskTicketService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _invoker = invoker;
        _itsmChangeRequestLifecycleService = itsmChangeRequestLifecycleService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _policy = policy;
        _ratelimitingRateLimitService = ratelimitingRateLimitService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _ticketApprovalEndpointSendGetTicketApprovalStatus0Port = ticketApprovalEndpointSendGetTicketApprovalStatus0PortAdapter;
        _sendApproveTicket2 = _itsmChangeRequestLifecycleService.ScheduleChangeRequestExpiryAsync;
    }
    /// <summary>
    /// Approves a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.ApproveTicketResponse> ApproveTicket(ApiHelpdesk.ApproveTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.NormalizeSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.NormalizeSlaPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                    PreferredChannel = (request.Ticket?.Requester?.PreferredChannel ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Ticket?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyValidationService.NormalizeSlaPolicyAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLicensing.GenerateLicenseAssignmentReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.GenerateLicenseAssignmentReportRequest.Types.Details
            {
                Period = (reply0.SlaPolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _licensingLicenseAssignmentReportingService.GenerateLicenseAssignmentReportAsync);
        var input2 = new ApiItsm.ScheduleChangeRequestExpiryRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScheduleChangeRequestExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.SlaPolicy?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _sendApproveTicket2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.NormalizeTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.NormalizeTicketRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply2.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Ticket?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Ticket?.Requester?.Phone ?? ""),
                },
                Sla = new ApiHelpdesk.TicketSla
                {
                    Breached = (reply2.ChangeRequest?.Sla?.Breached ?? false),
                },
            },
        };
        var named3 = _registry.Get<ApiHelpdesk.TicketValidationService.TicketValidationServiceClient>("acme.helpdesk.v1.TicketValidationService");
        using var rpc3 = named3.NormalizeTicketAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.ApproveTicketResponse
        {
            Outcome = new ApiHelpdesk.ApproveTicketResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply3.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply2.ChangeRequest?.Assignee?.Email ?? ""),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (reply0.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Sla = new ApiHelpdesk.TicketSla
                {
                    DueAt = (reply3.Ticket?.Sla?.DueAt ?? new Timestamp()),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ApproveTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.DelegateTicketApprovalResponse> DelegateTicketApproval(ApiHelpdesk.DelegateTicketApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateTicketApproval for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.LookupTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TicketId = (request.TicketId ?? ""),
        };
        var descriptor0 = new Method<ApiHelpdesk.LookupTicketRequest, ApiHelpdesk.LookupTicketResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".TicketQueryService", "LookupTicket",
            Wire.Marshaller<ApiHelpdesk.LookupTicketRequest>(), Wire.Marshaller<ApiHelpdesk.LookupTicketResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.LintFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Ticket?.Audit?.Etag ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiFeatureflags.LintFeatureFlagsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _featureflagsFeatureFlagValidationService.LintFeatureFlagsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (reply1.FeatureFlag?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Ticket?.Assignee?.UserId ?? ""),
            },
        };
        using var stream2 = _employeesOrgUnitQueryService.ExportOrgUnits(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change2.GetType().Name);
        }
        var response = new ApiHelpdesk.DelegateTicketApprovalResponse
        {
            Outcome = new ApiHelpdesk.DelegateTicketApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = (request.Ticket?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Ticket?.Audit?.CreatedBy ?? ""),
                    Etag = (reply1.FeatureFlag?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Ticket?.Requester?.PreferredChannel ?? ""),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "DelegateTicketApproval", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.EscalateTicketResponse> EscalateTicket(ApiHelpdesk.EscalateTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.BrowseKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.BrowseKnowledgeArticlesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _helpdeskKnowledgeArticleQueryService.BrowseKnowledgeArticlesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CloneTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                Channel = new ApiHelpdesk.TicketChannelOrigin
                {
                    Address = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Channel => (request.Ticket?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _helpdeskTicketService.CloneTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.EscalateTicketResponse
        {
            Outcome = new ApiHelpdesk.EscalateTicketResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "EscalateTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.GetTicketApprovalStatusResponse> GetTicketApprovalStatus(ApiHelpdesk.GetTicketApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTicketApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "GetTicketApprovalStatus", "accepted", token);
        var input0 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidateEmploymentContractRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendGetTicketApprovalStatus0Async(input0, token);
        var response = new ApiHelpdesk.GetTicketApprovalStatusResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                TicketId = (request.TicketId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists tickets a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.ListPendingTicketsResponse> ListPendingTickets(ApiHelpdesk.ListPendingTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "ListPendingTickets", "accepted", token);
        var response = new ApiHelpdesk.ListPendingTicketsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ListPendingTickets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.RecallTicketResponse> RecallTicket(ApiHelpdesk.RecallTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SuggestEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Ticket?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Ticket?.Assignee?.UserId ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeesEmployeeQueryService.SuggestEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployees.SuggestEmployeesRequest>("acme.helpdesk.v1.TicketApprovalService/RecallTicket/0", input0.Clone()), token);
        var response = new ApiHelpdesk.RecallTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Status = (request.Ticket?.Status ?? (ApiHelpdesk.TicketStatus)0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "RecallTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.RejectTicketResponse> RejectTicket(ApiHelpdesk.RejectTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Ticket?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.NormalizePositionRequest>();
        using var rpc0 = _employeesPositionValidationService.NormalizePositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.PreviewTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Position?.Audit?.Etag ?? ""),
                },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _helpdeskTicketValidationService.PreviewTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.RejectTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Ticket?.Assignee?.DisplayName ?? ""),
                },
                Priority = (request.Ticket?.Priority ?? 0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "RejectTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.SubmitTicketResponse> SubmitTicket(ApiHelpdesk.SubmitTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CompareTicketOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Ticket?.Assignee?.DisplayName ?? ""),
                },
                Channel = new ApiHelpdesk.TicketChannelOrigin
                {
                    Address = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Channel => (request.Ticket?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                },
                Sla = new ApiHelpdesk.TicketSla
                {
                    Breached = (request.Ticket?.Sla?.Breached ?? false),
                },
            },
        };
        using var rpc0 = _helpdeskTicketCalculationService.CompareTicketOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        foreach (var item in (request.Ticket?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.TicketComment>()))
        {
            var input1 = new ApiRatelimiting.WatchRateLimitsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Subscriber = new ApiCommon.ContactInfo
                {
                    Email = (item.AuthorEmail ?? ""),
                    Name = (reply0.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply0.Ticket?.Requester?.Phone ?? ""),
                },
            };
            using var stream1 = _ratelimitingRateLimitService.WatchRateLimits(input1, cancellationToken: token);
            await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received WatchRateLimits frame {Type}", change1.GetType().Name);
            }
        }
        var response = new ApiHelpdesk.SubmitTicketResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Ticket?.Audit?.CreatedBy ?? ""),
                },
                Status = (request.Ticket?.Status ?? (ApiHelpdesk.TicketStatus)0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "SubmitTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.ValidateEmploymentContractResponse> SendGetTicketApprovalStatus0Async(ApiEmployees.ValidateEmploymentContractRequest request, CancellationToken token)
    {
        return await _ticketApprovalEndpointSendGetTicketApprovalStatus0Port.SendAsync(request, token);
    }
}
