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
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPipelines = Acme.Pipelines.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class TicketValidationEndpoint
{
    /// <summary>
    /// Checks a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.CheckTicketEligibilityResponse> CheckTicketEligibility(ApiHelpdesk.CheckTicketEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckTicketEligibility for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.ExplainKnowledgeArticleCalculationResponse();
        foreach (var item in (request.Ticket?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.TicketComment>()))
        {
            var input0 = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc0 = _helpdeskKnowledgeArticleCalculationService.ExplainKnowledgeArticleCalculationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiPipelines.ComparePipelineOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Pipeline = new ApiPipelines.Pipeline
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.KnowledgeArticle?.Audit?.CreatedBy ?? ""),
                },
                Participants = { new ApiPipelines.PipelineParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Ticket?.Requester?.Phone ?? ""),
                    },
                } },
            },
        };
        var reply1 = await SendCheckTicketEligibility1Async(input1, token);
        var response = new ApiHelpdesk.CheckTicketEligibilityResponse
        {
            Outcome = new ApiHelpdesk.CheckTicketEligibilityResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Pipeline?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Subject = (request.Ticket?.Subject ?? ""),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "CheckTicketEligibility", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.ExplainTicketDecisionResponse> ExplainTicketDecision(ApiHelpdesk.ExplainTicketDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainTicketDecision for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.ExplainLedgerAccountDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.ExplainLedgerAccountDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _ledgerLedgerAccountValidationService.ExplainLedgerAccountDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ExplainTicketDecisionResponse
        {
            Outcome = new ApiHelpdesk.ExplainTicketDecisionResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.LedgerAccount?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CustomFields = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ExplainTicketDecision", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.GetTicketRulesResponse> GetTicketRules(ApiHelpdesk.GetTicketRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TicketId, "ticket_id");
        _logger.LogInformation("Handling GetTicketRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "ticket", request.TicketId,
            "GetTicketRules", "accepted", token);
        var input0 = new ApiHelpdesk.SimulateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.SimulateKnowledgeArticleRequest>();
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.SimulateKnowledgeArticleAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.GetTicketRulesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.LintTicketsResponse> LintTickets(ApiHelpdesk.LintTicketsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintTickets for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.ExplainTicketCalculationResponse();
        foreach (var item in (request.Ticket?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.TicketComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiHelpdesk.ExplainTicketCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Ticket = new ApiHelpdesk.Ticket
                {
                    Comments = { new ApiHelpdesk.TicketComment
                    {
                        AuthorEmail = (item.AuthorEmail ?? ""),
                        Body = (item.Body ?? ""),
                    } },
                },
            };
            using var rpc0 = _helpdeskTicketCalculationService.ExplainTicketCalculationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiHelpdesk.EstimateKnowledgeArticleRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (request.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
            },
            Counterparty = new ApiEmployees.EmployeeRef
            {
                Email = (reply0.Ticket?.Requester?.Email ?? ""),
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.LintTicketsResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Requester = (request.Ticket?.Requester ?? new ApiCommon.ContactInfo()),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "LintTickets", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.NormalizeTicketResponse> NormalizeTicket(ApiHelpdesk.NormalizeTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ResolveEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _licensingEntitlementQueryService.ResolveEntitlementsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ValidateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ValidateEntitlementRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Ticket?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Ticket?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingEntitlementValidationService.ValidateEntitlementAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.NormalizeTicketResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Priority = (request.Ticket?.Priority ?? 0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "NormalizeTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.PreviewTicketResponse> PreviewTicket(ApiHelpdesk.PreviewTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiKeymanagement.SimulateCryptoKeyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CryptoKey = new ApiKeymanagement.CryptoKey
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _keymanagementCryptoKeyCalculationService.SimulateCryptoKeyAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiHelpdesk.SimulateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Ticket?.Audit?.CreatedBy ?? ""),
                },
                Card = new ApiHelpdesk.KnowledgeArticleCardSettlement
                {
                    Last4 = (reply0.CryptoKey ?? new ApiKeymanagement.CryptoKey()).PaymentCase switch
                    {
                        ApiKeymanagement.CryptoKey.FundingOneofCase.Card => (reply0.CryptoKey?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.SimulateKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiHelpdesk.CompareKnowledgeArticleOptionsResponse();
        foreach (var item in (request.Ticket?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.TicketComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiHelpdesk.CompareKnowledgeArticleOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Card = new ApiHelpdesk.KnowledgeArticleCardSettlement
                    {
                        Last4 = (reply1.KnowledgeArticle?.Card?.Last4 ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc2 = _helpdeskKnowledgeArticleCalculationService.CompareKnowledgeArticleOptionsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EstimateKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply1.KnowledgeArticle?.BillingAddress?.PostalCode ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.PreviewTicketResponse
        {
            Outcome = new ApiHelpdesk.PreviewTicketResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.CryptoKey?.Holder?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply3.KnowledgeArticle?.Counterparty?.Email ?? ""),
                    PreferredChannel = (reply0.CryptoKey?.Contact?.PreferredChannel ?? ""),
                },
                Status = (request.Ticket?.Status ?? (ApiHelpdesk.TicketStatus)0),
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "PreviewTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.ScoreTicketResponse> ScoreTicket(ApiHelpdesk.ScoreTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.RecalculateKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Ticket?.Assignee?.DisplayName ?? ""),
                    Email = (request.Ticket?.Assignee?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.RecalculateKnowledgeArticlesRequest, ApiHelpdesk.RecalculateKnowledgeArticlesResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".KnowledgeArticleCalculationService", "RecalculateKnowledgeArticles",
            Wire.Marshaller<ApiHelpdesk.RecalculateKnowledgeArticlesRequest>(), Wire.Marshaller<ApiHelpdesk.RecalculateKnowledgeArticlesResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.SimulateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.SimulateKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.KnowledgeArticle?.BillingAddress?.Locality ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Card = new ApiHelpdesk.KnowledgeArticleCardSettlement
                {
                    Last4 = (reply0.KnowledgeArticle ?? new ApiHelpdesk.KnowledgeArticle()).PaymentCase switch
                    {
                        ApiHelpdesk.KnowledgeArticle.SettlementOneofCase.Card => (reply0.KnowledgeArticle?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.SimulateKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ScoreTicketResponse
        {
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.KnowledgeArticle?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CustomFields = { (request.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ScoreTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a ticket.
    /// </summary>
    public override async Task<ApiHelpdesk.ValidateTicketResponse> ValidateTicket(ApiHelpdesk.ValidateTicketRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateTicket for {Tenant}", requestContext.TenantId);
        if (request.Ticket is { } record)
        {
            var snapshot = TicketMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiHelpdesk.ValidateTicketRequest, ApiFeatureflags.SummarizeFeatureFlagsRequest>(value => new ApiFeatureflags.SummarizeFeatureFlagsRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Reason = (value.Reason ?? ""),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _featureflagsFeatureFlagQueryService.SummarizeFeatureFlagsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.RecalculateKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                KnowledgeArticleId = (request.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                {
                    ApiHelpdesk.Ticket.OriginOneofCase.Customer => (request.Ticket?.Customer?.KnowledgeArticleId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.RecalculateKnowledgeArticlesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ValidateTicketResponse
        {
            Outcome = new ApiHelpdesk.ValidateTicketResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = (request.Ticket?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.KnowledgeArticle?.Audit?.Etag ?? ""),
                },
                TicketId = (request.TicketId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Ticket?.TicketId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "ticket",
            "audit", "ValidateTicket", (request.Ticket?.TicketId ?? ""), token);
        return response;
    }
    private async Task<ApiPipelines.ComparePipelineOptionsResponse> SendCheckTicketEligibility1Async(ApiPipelines.ComparePipelineOptionsRequest request, CancellationToken token)
    {
        return await _ticketValidationEndpointSendCheckTicketEligibility1Port.SendAsync(request, token);
    }
}
