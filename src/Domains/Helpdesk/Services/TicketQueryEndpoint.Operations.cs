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
using ApiCertificates = Acme.Certificates.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class TicketQueryEndpoint
{
    /// <summary>
    /// Lists tickets a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.BrowseTicketsResponse> BrowseTickets(ApiHelpdesk.BrowseTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "BrowseTickets", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentFolderConflictsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiDocuments.ListDocumentFolderConflictsRequest.Types.Filter
        {
            Region = (request.Filter?.Region ?? ""),
        };
        input0.Page = new ApiCommon.PageRequest
        {
            PageToken = (request.Page?.PageToken ?? ""),
        };
        using var rpc0 = _documentsDocumentFolderSyncService.ListDocumentFolderConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.BrowseTicketsResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                TotalSize = (reply0.PageInfo?.TotalSize ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "BrowseTickets", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts tickets by status.
    /// </summary>
    public override async Task<ApiHelpdesk.CountTicketsResponse> CountTickets(ApiHelpdesk.CountTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "CountTickets", "accepted", token);
        var input0 = new ApiFeatureflags.ListFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ListFeatureFlagsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        using var rpc0 = _featureflagsFeatureFlagService.ListFeatureFlagsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.CountTicketsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "CountTickets", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching ticket.
    /// </summary>
    public override async Task ExportTickets(ApiHelpdesk.ExportTicketsRequest request, IServerStreamWriter<ApiHelpdesk.ExportTicketsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "ExportTickets", "accepted", token);
        var input0 = new ApiIdentity.ResolveUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _identityUserQueryService.ResolveUsersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.ShareDocumentTemplateExportRequest
        {
            Annotations = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareDocumentTemplateExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc1 = _documentsDocumentTemplateExportService.ShareDocumentTemplateExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.RecalculateEscalationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RecalculateEscalationsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.DocumentTemplate?.BillingAddress?.CountryCode ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.DocumentTemplate?.Audit?.CreatedBy ?? ""),
                },
                Sla = new ApiHelpdesk.EscalationSla
                {
                    DueAt = (reply1.DocumentTemplate?.DueAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _helpdeskEscalationCalculationService.RecalculateEscalationsAsync);
        var input3 = new ApiHelpdesk.CalculateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Escalation?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _sendExportTickets3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.ExportTicketsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Escalation?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply3.Ticket?.Requester?.PreferredChannel ?? ""),
                },
                Subject = (reply2.Escalation?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ExportTickets", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists tickets a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.FindTicketsResponse> FindTickets(ApiHelpdesk.FindTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "FindTickets", "accepted", token);
        var input0 = new ApiHelpdesk.ExplainSlaPolicyDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainSlaPolicyDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient>("acme.helpdesk.v1.SlaPolicyValidationService");
        using var rpc0 = named0.ExplainSlaPolicyDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.FindTicketsResponse
        {
            Tickets = { new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.SlaPolicy?.Owner?.UserId ?? ""),
                },
                Priority = (reply0.SlaPolicy?.Priority ?? 0),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "FindTickets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.LookupTicketResponse> LookupTicket(ApiHelpdesk.LookupTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        _logger.LogInformation("Handling LookupTicket for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "LookupTicket", "accepted", token);
        var input0 = new ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                DueAt = (request.Context?.Deadline ?? new Timestamp()),
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest, ApiHelpdesk.CheckKnowledgeArticleEligibilityResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".KnowledgeArticleValidationService", "CheckKnowledgeArticleEligibility",
            Wire.Marshaller<ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest>(), Wire.Marshaller<ApiHelpdesk.CheckKnowledgeArticleEligibilityResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.CloneFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiFeatureflags.CloneFeatureFlagRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _featureflagsFeatureFlagService.CloneFeatureFlagAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.LookupTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply1.FeatureFlag?.Owner?.UserId ?? ""),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (reply1.FeatureFlag?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                TicketId = (request.TicketId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.PeekTicketResponse> PeekTicket(ApiHelpdesk.PeekTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        _logger.LogInformation("Handling PeekTicket for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "PeekTicket", "accepted", token);
        var profile0 = new MappingProfile<ApiHelpdesk.PeekTicketRequest, ApiHelpdesk.SimulateSlaPolicyRequest>(value => new ApiHelpdesk.SimulateSlaPolicyRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.Context?.Deadline ?? new Timestamp()),
                },
                Block = new ApiHelpdesk.SlaPolicyBlockAction
                {
                    NotifyEmail = (value.Context?.Actor?.Email ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskSlaPolicyCalculationService.SimulateSlaPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ExplainSignatureDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainSignatureDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.SlaPolicy?.Owner?.Email ?? ""),
                },
            },
            Reason = (reply0.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
            {
                ApiHelpdesk.SlaPolicy.ActionOneofCase.Block => (reply0.SlaPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureValidationService.ExplainSignatureDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.PeekTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.SlaPolicy?.Owner?.Email ?? ""),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several tickets at once.
    /// </summary>
    public override async Task<ApiHelpdesk.ResolveTicketsResponse> ResolveTickets(ApiHelpdesk.ResolveTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "ResolveTickets", "accepted", token);
        var response = new ApiHelpdesk.ResolveTicketsResponse();
        return response;
    }
    /// <summary>
    /// Lists tickets a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.SuggestTicketsResponse> SuggestTickets(ApiHelpdesk.SuggestTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", "batch",
            "SuggestTickets", "accepted", token);
        var input0 = request.SuggestTicketsToCompareCertificateOptions0();
        using var rpc0 = _certificatesCertificateCalculationService.CompareCertificateOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.SuggestTicketsResponse
        {
            Tickets = { new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Certificate?.Counterparty?.DisplayName ?? ""),
                },
                Priority = (reply0.Certificate?.Counterparty?.Priority ?? 0),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "SuggestTickets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.SummarizeTicketsResponse> SummarizeTickets(ApiHelpdesk.SummarizeTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeTickets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "SummarizeTickets", "accepted", token);
        var input0 = new ApiDevices.SimulateDevicePolicyRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Block = new ApiDevices.DevicePolicyBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
            },
        };
        var reply0 = await SendSummarizeTickets0Async(input0, token);
        var response = new ApiHelpdesk.SummarizeTicketsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "SummarizeTickets", (request.TicketId ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.SimulateDevicePolicyResponse> SendSummarizeTickets0Async(ApiDevices.SimulateDevicePolicyRequest request, CancellationToken token)
    {
        return await _ticketQueryEndpointSendSummarizeTickets0Port.SendAsync(request, token);
    }
}
