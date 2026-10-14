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
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class TicketWorkflowEndpoint
{
    /// <summary>
    /// Advances a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.AdvanceTicketWorkflowResponse> AdvanceTicketWorkflow(ApiHelpdesk.AdvanceTicketWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceTicketWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.AdvanceTicketWorkflowToScheduleSlaPolicyExpiry0();
        using var rpc0 = _helpdeskSlaPolicyLifecycleService.ScheduleSlaPolicyExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ResumeEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ResumeEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        var reply1 = await SendAdvanceTicketWorkflow1Async(input1, token);
        var reply2 = new ApiHelpdesk.SimulateTicketResponse();
        foreach (var item in (request.Ticket?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.TicketStatusChange>()))
        {
            var input2 = new ApiHelpdesk.SimulateTicketRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Ticket = new ApiHelpdesk.Ticket
                {
                    History = { new ApiHelpdesk.TicketStatusChange
                    {
                        ChangedAt = (item.ChangedAt ?? new Timestamp()),
                        ChangedBy = (item.ChangedBy ?? ""),
                    } },
                },
            };
            using var rpc2 = _helpdeskTicketCalculationService.SimulateTicketAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiHelpdesk.AdvanceTicketWorkflowResponse
        {
            Outcome = new ApiHelpdesk.AdvanceTicketWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = (request.Ticket?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "AdvanceTicketWorkflow", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.CancelTicketWorkflowResponse> CancelTicketWorkflow(ApiHelpdesk.CancelTicketWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelTicketWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExportKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ExportKnowledgeArticlesRequest.Types.Filter
            {
                CreatedAfter = (request.Ticket?.Sla?.DueAt ?? new Timestamp()),
                OwnerId = (request.Ticket?.Sla?.PolicyId ?? ""),
            },
        };
        using var stream0 = _helpdeskKnowledgeArticleQueryService.ExportKnowledgeArticles(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportKnowledgeArticles frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiDocuments.RetryDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentTemplateExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiDocuments.RetryDocumentTemplateExportRequest>(json1);
        using var rpc1 = _documentsDocumentTemplateExportService.RetryDocumentTemplateExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.CancelTicketWorkflowResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.DocumentTemplate?.Audit?.Etag ?? ""),
                },
                Sla = new ApiHelpdesk.TicketSla
                {
                    DueAt = (reply1.DocumentTemplate?.DueAt ?? new Timestamp()),
                },
                Subject = (request.Ticket?.Subject ?? ""),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "CancelTicketWorkflow", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.InspectTicketWorkflowResponse> InspectTicketWorkflow(ApiHelpdesk.InspectTicketWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        _logger.LogInformation("Handling InspectTicketWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "InspectTicketWorkflow", "accepted", token);
        var input0 = new ApiEmployees.PreviewPositionRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiEmployees.PreviewPositionRequest.Types.Details
        {
            Locale = (request.Context?.Locale ?? ""),
        };
        input0.Position = new ApiEmployees.Position
        {
            Owner = new ApiIdentity.UserRef
            {
                Email = (request.Context?.Actor?.Email ?? ""),
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        using var rpc0 = _employeesPositionValidationService.PreviewPositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.SimulateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Position?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Position?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.Position?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskTicketCalculationService.SimulateTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.InspectTicketWorkflowResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Ticket?.Customer?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Position?.Audit?.UpdatedBy ?? ""),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists tickets a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.ListTicketWorkflowsResponse> ListTicketWorkflows(ApiHelpdesk.ListTicketWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListTicketWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "ListTicketWorkflows", "accepted", token);
        var input0 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.ExportDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ExportDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (reply0.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _documentsDocumentFolderQueryService.ExportDocumentFolders(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDocumentFolders frame {Type}", change1.GetType().Name);
        }
        var response = new ApiHelpdesk.ListTicketWorkflowsResponse
        {
            Tickets = { new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ListTicketWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.ResumeTicketWorkflowResponse> ResumeTicketWorkflow(ApiHelpdesk.ResumeTicketWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeTicketWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Ticket?.Requester?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingLicenseAssignmentApprovalService.DelegateLicenseAssignmentApprovalAsync);
        var input1 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details
            {
                Filters = { (reply0.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _sendResumeTicketWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var named2 = _registry.Get<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>("acme.employees.v1.EmploymentContractCalculationService");
        using var rpc2 = named2.QuoteEmploymentContractAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.ResumeTicketWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                },
                CustomFields = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ResumeTicketWorkflow", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.SignalTicketWorkflowResponse> SignalTicketWorkflow(ApiHelpdesk.SignalTicketWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalTicketWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.LintNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var descriptor0 = new Method<ApiNotifications.LintNotificationsRequest, ApiNotifications.LintNotificationsResponse>(
            MethodType.Unary, "acme.notifications.v1" + ".NotificationValidationService", "LintNotifications",
            Wire.Marshaller<ApiNotifications.LintNotificationsRequest>(), Wire.Marshaller<ApiNotifications.LintNotificationsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Notification?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.ExplainKnowledgeArticleCalculationAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.SignalTicketWorkflowResponse
        {
            Outcome = new ApiHelpdesk.SignalTicketWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                CustomFields = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Requester = (reply0.Notification?.Requester ?? new ApiCommon.ContactInfo()),
                Sla = new ApiHelpdesk.TicketSla
                {
                    Breached = (reply0.Notification?.Sla?.Breached ?? false),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "SignalTicketWorkflow", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.StartTicketWorkflowResponse> StartTicketWorkflow(ApiHelpdesk.StartTicketWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartTicketWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.SummarizeKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticleId = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
            {
                ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.KnowledgeArticleId ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleQueryService.SummarizeKnowledgeArticlesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ScheduleDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentTemplateExportService.ScheduleDocumentTemplateExportAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDevices.DelegateManagedDeviceApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Ticket?.Customer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _devicesManagedDeviceApprovalService.DelegateManagedDeviceApprovalAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiHelpdesk.ApproveTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply2.ManagedDevice?.Owner?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    KnowledgeArticleId = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.KnowledgeArticleId ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _helpdeskTicketApprovalService.ApproveTicketAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.StartTicketWorkflowResponse
        {
            Outcome = new ApiHelpdesk.StartTicketWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                CustomFields = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Sla = new ApiHelpdesk.TicketSla
                {
                    PausedFor = (reply3.Ticket?.Sla?.PausedFor ?? new Duration()),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "StartTicketWorkflow", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.ResumeEntitlementWorkflowResponse> SendAdvanceTicketWorkflow1Async(ApiLicensing.ResumeEntitlementWorkflowRequest request, CancellationToken token)
    {
        return await _ticketWorkflowEndpointSendAdvanceTicketWorkflow1Port.SendAsync(request, token);
    }
}
