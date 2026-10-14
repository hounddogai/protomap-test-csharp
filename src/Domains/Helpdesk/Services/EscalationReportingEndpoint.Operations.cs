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
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class EscalationReportingEndpoint
{
    /// <summary>
    /// Counts escalations by status.
    /// </summary>
    public override async Task<ApiHelpdesk.CountEscalationTotalsResponse> CountEscalationTotals(ApiHelpdesk.CountEscalationTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountEscalationTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", "batch",
            "CountEscalationTotals", "accepted", token);
        var response = new ApiHelpdesk.CountEscalationTotalsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "CountEscalationTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.DeleteEscalationReportResponse> DeleteEscalationReport(ApiHelpdesk.DeleteEscalationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        _logger.LogInformation("Handling DeleteEscalationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "DeleteEscalationReport", "accepted", token);
        var input0 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.QuoteEmploymentContractRequest>(json0);
        using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFuel.BatchGetFuelCardsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc1 = _fuelFuelCardService.BatchGetFuelCardsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.GetEscalationRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EscalationId = (request.EscalationId ?? ""),
        };
        using var rpc2 = _helpdeskEscalationValidationService.GetEscalationRulesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.DeleteEscalationReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "DeleteEscalationReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.FetchEscalationReportResponse> FetchEscalationReport(ApiHelpdesk.FetchEscalationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        _logger.LogInformation("Handling FetchEscalationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "FetchEscalationReport", "accepted", token);
        var input0 = new ApiHelpdesk.ExplainSlaPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyCalculationService.ExplainSlaPolicyCalculationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.CloneEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.SlaPolicy?.Owner?.Email ?? ""),
                    UserId = (reply0.SlaPolicy?.Owner?.UserId ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeService.CloneEmployeeAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.FetchEscalationReportResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.SlaPolicy?.Owner?.UserId ?? ""),
                },
                Comments = { new ApiHelpdesk.EscalationComment
                {
                    CreatedAt = (reply0.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                EscalationId = (request.EscalationId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.Employee?.Contact?.Phone ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Generates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.GenerateEscalationReportResponse> GenerateEscalationReport(ApiHelpdesk.GenerateEscalationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateEscalationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "GenerateEscalationReport", "accepted", token);
        var input0 = new ApiHelpdesk.CountKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.CountKnowledgeArticlesRequest.Types.Filter
            {
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.EscalationId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _helpdeskKnowledgeArticleQueryService.CountKnowledgeArticlesAsync);
        var input1 = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.UnsubscribeDocumentTemplateUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.GenerateEscalationReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "GenerateEscalationReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists escalations a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.ListEscalationReportsResponse> ListEscalationReports(ApiHelpdesk.ListEscalationReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListEscalationReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", "batch",
            "ListEscalationReports", "accepted", token);
        var input0 = new ApiEmployees.CloneEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiEmployees.EmployeeService.EmployeeServiceClient>("acme.employees.v1.EmployeeService");
        using var rpc0 = named0.CloneEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ExplainTicketDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainTicketDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Employee?.Contact?.Phone ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Employee?.RelationshipManager?.Email ?? ""),
                    PreferredChannel = (reply0.Employee?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiHelpdesk.ExplainTicketDecisionRequest, ApiHelpdesk.ExplainTicketDecisionResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".TicketValidationService", "ExplainTicketDecision",
            Wire.Marshaller<ApiHelpdesk.ExplainTicketDecisionRequest>(), Wire.Marshaller<ApiHelpdesk.ExplainTicketDecisionResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ListEscalationReportsResponse
        {
            Escalations = { new ApiHelpdesk.Escalation
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply0.Employee?.Contact?.Phone ?? ""),
                },
                Tags = { (reply1.Ticket?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ListEscalationReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ScheduleEscalationReportResponse> ScheduleEscalationReport(ApiHelpdesk.ScheduleEscalationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleEscalationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "ScheduleEscalationReport", "accepted", token);
        var input0 = new ApiDocuments.StreamDocumentTemplateNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateNoticesRequest.Types.Filter
            {
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _documentsDocumentTemplateNotificationService.StreamDocumentTemplateNotices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateNotices frame {Type}", change0.GetType().Name);
        }
        var profile1 = new MappingProfile<ApiHelpdesk.ScheduleEscalationReportRequest, ApiHelpdesk.CalculateSlaPolicyRequest>(value => new ApiHelpdesk.CalculateSlaPolicyRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Block = new ApiHelpdesk.SlaPolicyBlockAction
                {
                    NotifyEmail = (value.Context?.Actor?.Email ?? ""),
                    Reason = (value.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _helpdeskSlaPolicyCalculationService.CalculateSlaPolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ScheduleEscalationReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.EscalationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ScheduleEscalationReport", (request.EscalationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ShareEscalationReportResponse> ShareEscalationReport(ApiHelpdesk.ShareEscalationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareEscalationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "ShareEscalationReport", "accepted", token);
        var input0 = new ApiSurveys.ListSurveysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSurveys.ListSurveysRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _surveysSurveyService.ListSurveysAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ShareEscalationReportToCloneTicket1();
        using var rpc1 = _helpdeskTicketService.CloneTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMetrics.BrowseMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.BrowseMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        var reply2 = await SendShareEscalationReport2Async(input2, token);
        var response = new ApiHelpdesk.ShareEscalationReportResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Ticket?.Requester?.Email ?? ""),
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ShareEscalationReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of escalations as they happen.
    /// </summary>
    public override async Task StreamEscalationMetrics(ApiHelpdesk.StreamEscalationMetricsRequest request, IServerStreamWriter<ApiHelpdesk.StreamEscalationMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamEscalationMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", "batch",
            "StreamEscalationMetrics", "accepted", token);
        var input0 = new ApiHelpdesk.ScoreTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScoreTicketRequest.Types.Details
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
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _helpdeskTicketValidationService.ScoreTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExplainEmploymentContractDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Ticket?.Assignee?.Email ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiEmployees.ExplainEmploymentContractDecisionRequest>();
        using var rpc1 = _employeesEmploymentContractValidationService.ExplainEmploymentContractDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.QuoteSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.QuoteSlaPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiHelpdesk.QuoteSlaPolicyRequest>(json2);
        using var rpc2 = _helpdeskSlaPolicyCalculationService.QuoteSlaPolicyAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.LintKnowledgeArticlesRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedBy = (reply0.Ticket?.Audit?.CreatedBy ?? ""),
            },
        };
        using var rpc3 = _helpdeskKnowledgeArticleValidationService.LintKnowledgeArticlesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.StreamEscalationMetricsResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.EmploymentContract?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply3.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
                Sla = new ApiHelpdesk.EscalationSla
                {
                    Breached = (reply0.Ticket?.Sla?.Breached ?? false),
                    DueAt = (reply1.EmploymentContract?.DueAt ?? new Timestamp()),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.Query ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "StreamEscalationMetrics", (request.Filter?.Query ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiMetrics.BrowseMetricSeriesesResponse> SendShareEscalationReport2Async(ApiMetrics.BrowseMetricSeriesesRequest request, CancellationToken token)
    {
        return await _escalationReportingEndpointSendShareEscalationReport2Port.SendAsync(request, token);
    }
}
