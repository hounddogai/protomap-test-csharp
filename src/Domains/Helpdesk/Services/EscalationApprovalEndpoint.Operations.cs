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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class EscalationApprovalEndpoint
{
    /// <summary>
    /// Approves an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ApproveEscalationResponse> ApproveEscalation(ApiHelpdesk.ApproveEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ApproveEscalationToLintKnowledgeArticles0();
        using var rpc0 = _helpdeskKnowledgeArticleValidationService.LintKnowledgeArticlesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiHelpdesk.LintKnowledgeArticlesRequest>("acme.helpdesk.v1.EscalationApprovalService/ApproveEscalation/0", input0.Clone()), token);
        var reply1 = new ApiHelpdesk.DelegateTicketApprovalResponse();
        foreach (var item in (request.Escalation?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationComment>()))
        {
            var input1 = new ApiHelpdesk.DelegateTicketApprovalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Ticket = new ApiHelpdesk.Ticket
                {
                    Comments = { new ApiHelpdesk.TicketComment
                    {
                        CommentId = (item.CommentId ?? ""),
                    } },
                },
            };
            using var rpc1 = _helpdeskTicketApprovalService.DelegateTicketApprovalAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiHelpdesk.ApproveEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
                CustomFields = { (request.Escalation?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EscalationId = (request.EscalationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ApproveEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.CommentEscalationApprovalResponse> CommentEscalationApproval(ApiHelpdesk.CommentEscalationApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentEscalationApproval for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.EscalateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EscalateSlaPolicyRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Escalation?.Assignee?.DisplayName ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _helpdeskSlaPolicyApprovalService.EscalateSlaPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.SuggestTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.SuggestTicketsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                {
                    ApiHelpdesk.Escalation.OriginOneofCase.Customer => (request.Escalation?.Customer?.TicketId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _helpdeskTicketQueryService.SuggestTicketsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.CommentEscalationApprovalResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Subject = (request.Escalation?.Subject ?? ""),
            },
            Outcome = new ApiHelpdesk.CommentEscalationApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "CommentEscalationApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.DelegateEscalationApprovalResponse> DelegateEscalationApproval(ApiHelpdesk.DelegateEscalationApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateEscalationApproval for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.RenewEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RenewEscalationRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            EscalationId = (request.Escalation?.EscalationId ?? ""),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiHelpdesk.RenewEscalationRequest>(json0);
        using var rpc0 = _helpdeskEscalationLifecycleService.RenewEscalationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.CloneNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Escalation?.Assignee?.DisplayName ?? ""),
                },
                Customer = new ApiNotifications.NotificationTemplateRef
                {
                    DisplayName = (reply0.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply0.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Escalation?.Requester?.PreferredChannel ?? ""),
                },
                Sla = new ApiNotifications.NotificationSla
                {
                    PausedFor = (request.Escalation?.Sla?.PausedFor ?? new Duration()),
                },
            },
        };
        using var rpc1 = _notificationsNotificationService.CloneNotificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.SignalLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (request.Escalation?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _licensingLicenseAssignmentWorkflowService.SignalLicenseAssignmentWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.DelegateEscalationApprovalResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = (reply0.Escalation?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply2.LicenseAssignment?.Audit?.Etag ?? ""),
                },
                CustomFields = { (reply1.Notification?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EscalationId = (request.EscalationId ?? ""),
            },
            Outcome = new ApiHelpdesk.DelegateEscalationApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "DelegateEscalationApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.EscalateEscalationResponse> EscalateEscalation(ApiHelpdesk.EscalateEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeService.BatchGetEmployeesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiItsm.StreamServiceAssetMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
                Name = (request.Escalation?.Requester?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Escalation?.Requester?.Phone ?? ""),
                PreferredChannel = (request.Escalation?.Requester?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _itsmServiceAssetReportingService.StreamServiceAssetMetrics(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamServiceAssetMetrics frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiEmployees.CloneEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Escalation?.Assignee?.Email ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _employeesEmployeeService.CloneEmployeeAsync);
        var response = new ApiHelpdesk.EscalateEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Priority = (request.Escalation?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply2.Employee?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "EscalateEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists escalations a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.ListPendingEscalationsResponse> ListPendingEscalations(ApiHelpdesk.ListPendingEscalationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingEscalations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", "batch",
            "ListPendingEscalations", "accepted", token);
        var input0 = new ApiHelpdesk.CancelTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    AuthorEmail = (request.Context?.Actor?.Email ?? ""),
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
            },
        };
        using var rpc0 = _sendListPendingEscalations0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ListPendingEscalationsResponse
        {
            Escalations = { new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Ticket?.Audit?.UpdatedBy ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ListPendingEscalations", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.RecallEscalationResponse> RecallEscalation(ApiHelpdesk.RecallEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.SubmitTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Channel = new ApiHelpdesk.TicketChannelOrigin
                {
                    Channel = (request.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Channel => (request.Escalation?.Channel?.Channel ?? ""),
                        _ => ""
                    },
                },
                Sla = new ApiHelpdesk.TicketSla
                {
                    DueAt = (request.Escalation?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _helpdeskTicketApprovalService.SubmitTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ScheduleEscalationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EscalationId = (request.Escalation?.EscalationId ?? ""),
        };
        var descriptor1 = new Method<ApiHelpdesk.ScheduleEscalationReportRequest, ApiHelpdesk.ScheduleEscalationReportResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".EscalationReportingService", "ScheduleEscalationReport",
            Wire.Marshaller<ApiHelpdesk.ScheduleEscalationReportRequest>(), Wire.Marshaller<ApiHelpdesk.ScheduleEscalationReportResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.RecallEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = (request.Escalation?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EscalationId = (request.EscalationId ?? ""),
            },
            Outcome = new ApiHelpdesk.RecallEscalationResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "RecallEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.RejectEscalationResponse> RejectEscalation(ApiHelpdesk.RejectEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.AdvanceKnowledgeArticleWorkflowResponse();
        foreach (var item in (request.Escalation?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationComment>()))
        {
            var input0 = new ApiHelpdesk.AdvanceKnowledgeArticleWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.AdvanceKnowledgeArticleWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                        UpdatedBy = (request.Escalation?.Audit?.UpdatedBy ?? ""),
                    },
                },
            };
            using var rpc0 = _helpdeskKnowledgeArticleWorkflowService.AdvanceKnowledgeArticleWorkflowAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var reply1 = new ApiEmployees.RecalculateEmploymentContractsResponse();
        foreach (var item in (request.Escalation?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiEmployees.RecalculateEmploymentContractsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Bank = new ApiEmployees.EmploymentContractBankSettlement
                    {
                        Iban = (reply0.KnowledgeArticle?.Bank?.Iban ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiHelpdesk.RejectEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Requester = (request.Escalation?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "RejectEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.SubmitEscalationResponse> SubmitEscalation(ApiHelpdesk.SubmitEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Escalation?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationStatusChange>()).Select(async item =>
        {
            var input0 = new ApiEmployees.CreateEmployeeRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Employee = new ApiEmployees.Employee
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (item.ChangedAt ?? new Timestamp()),
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Escalation?.Assignee?.Email ?? ""),
                    },
                    RelationshipManager = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.Escalation?.Assignee?.DisplayName ?? ""),
                        UserId = (request.Details?.Approver?.UserId ?? ""),
                    },
                },
            };
            using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiEmployees.CreateEmployeeResponse();
        var input1 = request.SubmitEscalationToRejectTicket1();
        using var rpc1 = _helpdeskTicketApprovalService.RejectTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.SubmitEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Sla = new ApiHelpdesk.EscalationSla
                {
                    Breached = (reply1.Ticket?.Sla?.Breached ?? false),
                },
                Status = (request.Escalation?.Status ?? (ApiHelpdesk.EscalationStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "SubmitEscalation", (request.Reason ?? ""), token);
        return response;
    }
}
