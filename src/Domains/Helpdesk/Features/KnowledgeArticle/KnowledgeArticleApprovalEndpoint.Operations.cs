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
using ApiAuthorization = Acme.Authorization.V1;
using ApiBanking = Acme.Banking.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class KnowledgeArticleApprovalEndpoint
{
    /// <summary>
    /// Approves a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ApproveKnowledgeArticleResponse> ApproveKnowledgeArticle(ApiHelpdesk.ApproveKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NormalizeSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NormalizeSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureValidationService.NormalizeSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.SummarizeAccessPoliciesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.SummarizeAccessPoliciesRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _authorizationAccessPolicyQueryService.SummarizeAccessPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ApproveKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Number = (reply0.Signature?.Subject?.Number ?? ""),
                Total = (request.KnowledgeArticle?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.KnowledgeArticle?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ApproveKnowledgeArticle", (request.KnowledgeArticle?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.CommentKnowledgeArticleApprovalResponse> CommentKnowledgeArticleApproval(ApiHelpdesk.CommentKnowledgeArticleApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentKnowledgeArticleApproval for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.KnowledgeArticle?.BillingAddress?.PostalCode ?? ""),
                },
                Card = new ApiHelpdesk.KnowledgeArticleCardSettlement
                {
                    Network = (request.KnowledgeArticle ?? new ApiHelpdesk.KnowledgeArticle()).PaymentCase switch
                    {
                        ApiHelpdesk.KnowledgeArticle.SettlementOneofCase.Card => (request.KnowledgeArticle?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleValidationService.ExplainKnowledgeArticleDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.EscalateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.KnowledgeArticle?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (reply0.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiHelpdesk.EscalateTicketRequest>(json1);
        using var rpc1 = _helpdeskTicketApprovalService.EscalateTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.StreamEscalationMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply1.Ticket?.Assignee?.Email ?? ""),
            },
        };
        using var stream2 = _helpdeskEscalationReportingService.StreamEscalationMetrics(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamEscalationMetrics frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiDocuments.ValidateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Ticket?.Assignee?.Email ?? ""),
                    Name = (reply1.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _documentsSignatureValidationService.ValidateSignatureAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.ValidateSignatureRequest>("acme.helpdesk.v1.KnowledgeArticleApprovalService/CommentKnowledgeArticleApproval/3", input3.Clone()), token);
        var response = new ApiHelpdesk.CommentKnowledgeArticleApprovalResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.KnowledgeArticle?.BillingAddress?.CountryCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Ticket?.Customer?.DisplayName ?? ""),
                },
                IssuedAt = (request.KnowledgeArticle?.IssuedAt ?? new Timestamp()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Outcome = new ApiHelpdesk.CommentKnowledgeArticleApprovalResponse.Types.Outcome
            {
                Status = (reply3.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "CommentKnowledgeArticleApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.DelegateKnowledgeArticleApprovalResponse> DelegateKnowledgeArticleApproval(ApiHelpdesk.DelegateKnowledgeArticleApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateKnowledgeArticleApproval for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ScheduleEscalationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScheduleEscalationReportRequest.Types.Details
            {
                Filters = { (request.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskEscalationReportingService.ScheduleEscalationReportAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiBanking.ResolveBankAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _bankingBankAccountQueryService.ResolveBankAccountsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.DelegateKnowledgeArticleApprovalResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Notes = (request.KnowledgeArticle?.Notes ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.KnowledgeArticle?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "DelegateKnowledgeArticleApproval", (request.KnowledgeArticle?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.EscalateKnowledgeArticleResponse> EscalateKnowledgeArticle(ApiHelpdesk.EscalateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.QuoteKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.KnowledgeArticle?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _helpdeskKnowledgeArticleCalculationService.QuoteKnowledgeArticleAsync);
        var input1 = new ApiHelpdesk.RenewKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RenewKnowledgeArticleRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
        };
        using var rpc1 = _sendEscalateKnowledgeArticle1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.KnowledgeArticle?.Counterparty?.EmployeeId ?? ""),
                Region = (reply1.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var named2 = _registry.Get<ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient>("acme.employees.v1.OrgUnitQueryService");
        using var rpc2 = named2.BrowseOrgUnitsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.EscalateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (reply0.KnowledgeArticle?.BillingAddress?.Locality ?? ""),
                },
                IssuedAt = (request.KnowledgeArticle?.IssuedAt ?? new Timestamp()),
                KnowledgeArticleId = (reply1.KnowledgeArticle?.KnowledgeArticleId ?? ""),
            },
            Outcome = new ApiHelpdesk.EscalateKnowledgeArticleResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "EscalateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists knowledge articles a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.ListPendingKnowledgeArticlesResponse> ListPendingKnowledgeArticles(ApiHelpdesk.ListPendingKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingKnowledgeArticles for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", "batch",
            "ListPendingKnowledgeArticles", "accepted", token);
        var input0 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiEmployees.BatchGetEmployeesRequest, ApiEmployees.BatchGetEmployeesResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeService", "BatchGetEmployees",
            Wire.Marshaller<ApiEmployees.BatchGetEmployeesRequest>(), Wire.Marshaller<ApiEmployees.BatchGetEmployeesResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ListPendingKnowledgeArticlesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ListPendingKnowledgeArticles", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.RecallKnowledgeArticleResponse> RecallKnowledgeArticle(ApiHelpdesk.RecallKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SignalLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (request.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLicensing.SignalLicenseAssignmentWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingLicenseAssignmentWorkflowService.SignalLicenseAssignmentWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiHelpdesk.RecallKnowledgeArticleRequest, ApiHelpdesk.ExplainTicketCalculationRequest>(value => new ApiHelpdesk.ExplainTicketCalculationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainTicketCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (value.Details?.Approver?.DisplayName ?? ""),
                    Email = (value.Details?.Approver?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (value.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _helpdeskTicketCalculationService.ExplainTicketCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiAuthorization.SummarizeAccessPoliciesResponse();
        foreach (var item in (request.KnowledgeArticle?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.KnowledgeArticleLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiAuthorization.SummarizeAccessPoliciesRequest
            {
                Annotations = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiAuthorization.SummarizeAccessPoliciesRequest.Types.Details
                {
                    Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
                Reason = (request.Reason ?? ""),
            };
            using var rpc2 = _authorizationAccessPolicyQueryService.SummarizeAccessPoliciesAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiHelpdesk.RecallKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply1.Ticket?.Assignee?.Email ?? ""),
                },
                IssuedAt = (reply0.LicenseAssignment?.IssuedAt ?? new Timestamp()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Metadata = { (request.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiHelpdesk.RecallKnowledgeArticleResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.KnowledgeArticle?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "RecallKnowledgeArticle", (request.KnowledgeArticle?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.SubmitKnowledgeArticleResponse> SubmitKnowledgeArticle(ApiHelpdesk.SubmitKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.SubmitKnowledgeArticleToDelegateIncidentApproval0();
        using var rpc0 = _itsmIncidentApprovalService.DelegateIncidentApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.SummarizeAccessPoliciesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await SendSubmitKnowledgeArticle1Async(input1, token);
        var response = new ApiHelpdesk.SubmitKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Status = (request.KnowledgeArticle?.Status ?? (ApiHelpdesk.KnowledgeArticleStatus)0),
            },
            Outcome = new ApiHelpdesk.SubmitKnowledgeArticleResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.KnowledgeArticle?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "SubmitKnowledgeArticle", (request.KnowledgeArticle?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiAuthorization.SummarizeAccessPoliciesResponse> SendSubmitKnowledgeArticle1Async(ApiAuthorization.SummarizeAccessPoliciesRequest request, CancellationToken token)
    {
        return await _knowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port.SendAsync(request, token);
    }
}
