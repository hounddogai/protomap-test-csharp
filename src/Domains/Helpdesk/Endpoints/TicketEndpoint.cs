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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Manages tickets: creates, reads, changes, and lists them.
/// </summary>
public sealed class TicketEndpoint : ApiHelpdesk.TicketService.TicketServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient _helpdeskKnowledgeArticleQueryService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ILogger<TicketEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public TicketEndpoint(
        NotificationBus bus,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient helpdeskKnowledgeArticleQueryService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        CallInvoker invoker,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ILogger<TicketEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _bus = bus;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _helpdeskKnowledgeArticleQueryService = helpdeskKnowledgeArticleQueryService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _invoker = invoker;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
    }
    /// <summary>
    /// Takes a stream of tickets and answers once at the end.
    /// </summary>
    public override async Task<ApiHelpdesk.BatchCreateTicketsResponse> BatchCreateTickets(IAsyncStreamReader<ApiHelpdesk.BatchCreateTicketsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiHelpdesk.BatchCreateTicketsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling BatchCreateTickets for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiItsm.SimulateProblemRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Problem = new ApiItsm.Problem
                {
                    Assignee = new ApiIdentity.UserRef
                    {
                        Email = request.ChunkCase switch
                        {
                            ApiHelpdesk.BatchCreateTicketsRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Email ?? ""),
                            _ => ""
                        },
                    },
                    Customer = new ApiEmployees.EmployeeRef
                    {
                        Email = request.ChunkCase switch
                        {
                            ApiHelpdesk.BatchCreateTicketsRequest.ChunkOneofCase.Record => (request.Record?.Requester?.Email ?? ""),
                            _ => ""
                        },
                    },
                    Requester = new ApiCommon.ContactInfo
                    {
                        Name = request.ChunkCase switch
                        {
                            ApiHelpdesk.BatchCreateTicketsRequest.ChunkOneofCase.Record => (request.Record?.Requester?.Name ?? new ApiCommon.PersonName()),
                            _ => new ApiCommon.PersonName()
                        },
                    },
                    Sla = new ApiItsm.ProblemSla
                    {
                        Breached = request.ChunkCase switch
                        {
                            ApiHelpdesk.BatchCreateTicketsRequest.ChunkOneofCase.Record => (request.Record?.Sla?.Breached ?? false),
                            _ => false
                        },
                    },
                },
            };
            using var rpc0 = _itsmProblemCalculationService.SimulateProblemAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var response = new ApiHelpdesk.BatchCreateTicketsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Message = (reply0.Result?.Message ?? ""),
                    Status = (reply0.Result?.Status ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.TicketId ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "ticket",
                "audit", "BatchCreateTickets", (request.Record?.TicketId ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Creates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.CloneTicketResponse> CloneTicket(ApiHelpdesk.CloneTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.LintKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.LintKnowledgeArticlesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Ticket?.Assignee?.Email ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.LintKnowledgeArticlesRequest>();
        using var rpc0 = _helpdeskKnowledgeArticleValidationService.LintKnowledgeArticlesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.CloneTicketResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = (request.Ticket?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CustomFields = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "CloneTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.CreateTicketResponse> CreateTicket(ApiHelpdesk.CreateTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.FindKnowledgeArticlesResponse();
        foreach (var item in (request.Ticket?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.TicketComment>()))
        {
            var input0 = new ApiHelpdesk.FindKnowledgeArticlesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiHelpdesk.FindKnowledgeArticlesRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                },
            };
            using var rpc0 = _helpdeskKnowledgeArticleQueryService.FindKnowledgeArticlesAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiLicensing.CompareLicenseAssignmentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Ticket?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentCalculationService.CompareLicenseAssignmentOptionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.ScoreKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleValidationService.ScoreKnowledgeArticleAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiHelpdesk.CreateTicketResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply2.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Priority = (request.Ticket?.Priority ?? 0),
                Requester = (request.Ticket?.Requester ?? new ApiCommon.ContactInfo()),
                Sla = new ApiHelpdesk.TicketSla
                {
                    DueAt = (reply2.KnowledgeArticle?.DueAt ?? new Timestamp()),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "CreateTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.DeleteTicketResponse> DeleteTicket(ApiHelpdesk.DeleteTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteTicket for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "DeleteTicket", "accepted", token);
        var input0 = new ApiLicensing.FindEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.FindEntitlementsRequest.Types.Filter
            {
                OwnerId = (request.TicketId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingEntitlementQueryService.FindEntitlementsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiHelpdesk.ExplainTicketCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskTicketCalculationService.ExplainTicketCalculationAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiHelpdesk.RecalculateEscalationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _helpdeskEscalationCalculationService.RecalculateEscalationsAsync);
        var response = new ApiHelpdesk.DeleteTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "DeleteTicket", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.GetTicketResponse> GetTicket(ApiHelpdesk.GetTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTicket for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "GetTicket", "accepted", token);
        var input0 = new ApiDocuments.CheckSignatureEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CheckSignatureEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.TicketId ?? ""),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.CheckSignatureEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.GetTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
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
    public override async Task<ApiHelpdesk.ListTicketsResponse> ListTickets(ApiHelpdesk.ListTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "ListTickets", "accepted", token);
        var input0 = new ApiHelpdesk.ValidateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiHelpdesk.TicketValidationService.TicketValidationServiceClient>("acme.helpdesk.v1.TicketValidationService");
        using var rpc0 = named0.ValidateTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.CountEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.CountEntitlementsRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        var descriptor1 = new Method<ApiLicensing.CountEntitlementsRequest, ApiLicensing.CountEntitlementsResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementQueryService", "CountEntitlements",
            Wire.Marshaller<ApiLicensing.CountEntitlementsRequest>(), Wire.Marshaller<ApiLicensing.CountEntitlementsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ListTicketsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ListTickets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.UpdateTicketResponse> UpdateTicket(ApiHelpdesk.UpdateTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainTicketDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Ticket?.Assignee?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Ticket?.Audit?.Etag ?? ""),
                },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    KnowledgeArticleId = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.KnowledgeArticleId ?? ""),
                        _ => ""
                    },
                },
                Sla = new ApiHelpdesk.TicketSla
                {
                    PausedFor = (request.Ticket?.Sla?.PausedFor ?? new Duration()),
                },
            },
        };
        using var rpc0 = _helpdeskTicketValidationService.ExplainTicketDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiHelpdesk.ExplainTicketDecisionRequest>("acme.helpdesk.v1.TicketService/UpdateTicket/0", input0.Clone()), token);
        var input1 = new ApiDocuments.ExplainSignatureDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainSignatureDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                    Name = (reply0.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Ticket?.Requester?.Phone ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    Number = (reply0.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (reply0.Ticket?.Customer?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsSignatureValidationService.ExplainSignatureDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.UpdateTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = (request.Ticket?.Assignee ?? new ApiIdentity.UserRef()),
                Priority = (request.Ticket?.Priority ?? 0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "UpdateTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of tickets as they happen.
    /// </summary>
    public override async Task WatchTickets(ApiHelpdesk.WatchTicketsRequest request, IServerStreamWriter<ApiHelpdesk.WatchTicketsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "WatchTickets", "accepted", token);
        var input0 = new ApiHelpdesk.RecalculateEscalationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RecalculateEscalationsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _helpdeskEscalationCalculationService.RecalculateEscalationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.WatchTicketsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (reply0.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "WatchTickets", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
