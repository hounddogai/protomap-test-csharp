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
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class KnowledgeArticleValidationEndpoint
{
    /// <summary>
    /// Checks a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.CheckKnowledgeArticleEligibilityResponse> CheckKnowledgeArticleEligibility(ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckKnowledgeArticleEligibility for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.KnowledgeArticle?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
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
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.LintIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.KnowledgeArticle?.Audit?.CreatedBy ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiItsm.LintIncidentsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _itsmIncidentValidationService.LintIncidentsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.CheckKnowledgeArticleEligibilityResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply1.Incident?.Customer?.EmployeeId ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Metadata = { (request.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiHelpdesk.CheckKnowledgeArticleEligibilityResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "CheckKnowledgeArticleEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ExplainKnowledgeArticleDecisionResponse> ExplainKnowledgeArticleDecision(ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainKnowledgeArticleDecision for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest, ApiHelpdesk.ValidateTicketRequest>(value => new ApiHelpdesk.ValidateTicketRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ValidateTicketRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (value.Details?.Contact?.Phone ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (value.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (value.Details?.Contact?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskTicketValidationService.ValidateTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CompareKnowledgeArticleOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CompareKnowledgeArticleOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Bank = new ApiHelpdesk.KnowledgeArticleBankSettlement
                {
                    AccountHolder = (request.KnowledgeArticle ?? new ApiHelpdesk.KnowledgeArticle()).PaymentCase switch
                    {
                        ApiHelpdesk.KnowledgeArticle.SettlementOneofCase.Bank => (request.KnowledgeArticle?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.KnowledgeArticle?.BillingAddress?.PostalCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Ticket?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.CompareKnowledgeArticleOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ExplainKnowledgeArticleDecisionResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                DueAt = (reply1.KnowledgeArticle?.DueAt ?? new Timestamp()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Notes = (request.KnowledgeArticle?.Notes ?? ""),
                Number = (reply0.Ticket?.Customer?.Number ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ExplainKnowledgeArticleDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.GetKnowledgeArticleRulesResponse> GetKnowledgeArticleRules(ApiHelpdesk.GetKnowledgeArticleRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        _logger.LogInformation("Handling GetKnowledgeArticleRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "GetKnowledgeArticleRules", "accepted", token);
        var response = new ApiHelpdesk.GetKnowledgeArticleRulesResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.LintKnowledgeArticlesResponse> LintKnowledgeArticles(ApiHelpdesk.LintKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintKnowledgeArticles for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.LintKnowledgeArticlesToResolveWebhookEndpoints0();
        using var rpc0 = _webhooksWebhookEndpointQueryService.ResolveWebhookEndpointsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SuggestDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.KnowledgeArticle?.Counterparty?.EmployeeId ?? ""),
                Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await SendLintKnowledgeArticles1Async(input1, token);
        var input2 = new ApiDocuments.NotifyDocumentWatchersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentNotificationService.NotifyDocumentWatchersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.LintKnowledgeArticlesResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = (request.KnowledgeArticle?.BillingAddress ?? new ApiCommon.PostalAddress()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Metadata = { (reply2.Document?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiHelpdesk.LintKnowledgeArticlesResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "LintKnowledgeArticles", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.NormalizeKnowledgeArticleResponse> NormalizeKnowledgeArticle(ApiHelpdesk.NormalizeKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainSlaPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.ExplainSlaPolicyCalculationRequest>();
        using var rpc0 = _helpdeskSlaPolicyCalculationService.ExplainSlaPolicyCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ExplainDocumentTemplateCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply0.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Tax = new ApiDocuments.DocumentTemplateLineTax
                    {
                        Amount = (reply0.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                        {
                            ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (reply0.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _documentsDocumentTemplateCalculationService.ExplainDocumentTemplateCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SlaPolicy?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.DocumentTemplate?.BillingAddress?.Recipient ?? ""),
                },
                Card = new ApiHelpdesk.KnowledgeArticleCardSettlement
                {
                    HolderName = (reply1.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Card => (reply1.DocumentTemplate?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.NormalizeKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                    {
                        Amount = (reply0.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
                Total = (request.KnowledgeArticle?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "NormalizeKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.PreviewKnowledgeArticleResponse> PreviewKnowledgeArticle(ApiHelpdesk.PreviewKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.KnowledgeArticle?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.KnowledgeArticleLine>()).Select(async item =>
        {
            var input0 = new ApiHelpdesk.RecalculateKnowledgeArticlesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Lines = { new ApiHelpdesk.KnowledgeArticleLine
                    {
                        Sku = (item.Sku ?? ""),
                        Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                        {
                            Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc0 = _helpdeskKnowledgeArticleCalculationService.RecalculateKnowledgeArticlesAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiHelpdesk.RecalculateKnowledgeArticlesResponse();
        var response = new ApiHelpdesk.PreviewKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Total = (request.KnowledgeArticle?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "PreviewKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ScoreKnowledgeArticleResponse> ScoreKnowledgeArticle(ApiHelpdesk.ScoreKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.BatchGetAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _authorizationAccessPolicyService.BatchGetAccessPoliciesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.ScoreKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = (request.KnowledgeArticle?.BillingAddress ?? new ApiCommon.PostalAddress()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ScoreKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ValidateKnowledgeArticleResponse> ValidateKnowledgeArticle(ApiHelpdesk.ValidateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.SimulateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (request.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskTicketCalculationService.SimulateTicketAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiHelpdesk.NormalizeTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Ticket?.Assignee?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Ticket?.Requester?.Email ?? ""),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _helpdeskTicketValidationService.NormalizeTicketAsync);
        var response = new ApiHelpdesk.ValidateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Number = (reply0.Ticket?.Customer?.Number ?? ""),
                Status = (request.KnowledgeArticle?.Status ?? (ApiHelpdesk.KnowledgeArticleStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ValidateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.SuggestDocumentFoldersResponse> SendLintKnowledgeArticles1Async(ApiDocuments.SuggestDocumentFoldersRequest request, CancellationToken token)
    {
        return await _knowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port.SendAsync(request, token);
    }
}
