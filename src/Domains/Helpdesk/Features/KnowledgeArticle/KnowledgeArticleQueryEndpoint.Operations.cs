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
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class KnowledgeArticleQueryEndpoint
{
    /// <summary>
    /// Lists knowledge articles a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.BrowseKnowledgeArticlesResponse> BrowseKnowledgeArticles(ApiHelpdesk.BrowseKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseKnowledgeArticles for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", "batch",
            "BrowseKnowledgeArticles", "accepted", token);
        var input0 = request.BrowseKnowledgeArticlesToCheckEscalationEligibility0();
        using var rpc0 = _helpdeskEscalationValidationService.CheckEscalationEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.LintChangeRequestsRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Customer = new ApiItsm.IncidentRef
                {
                    Priority = (reply0.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply0.Escalation?.Customer?.Priority ?? 0),
                        _ => 0
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _itsmChangeRequestValidationService.LintChangeRequestsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiItsm.LintChangeRequestsRequest>("acme.helpdesk.v1.KnowledgeArticleQueryService/BrowseKnowledgeArticles/1", input1.Clone()), token);
        var response = new ApiHelpdesk.BrowseKnowledgeArticlesResponse
        {
            KnowledgeArticles = { new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "BrowseKnowledgeArticles", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts knowledge articles by status.
    /// </summary>
    public override async Task<ApiHelpdesk.CountKnowledgeArticlesResponse> CountKnowledgeArticles(ApiHelpdesk.CountKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountKnowledgeArticles for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", "batch",
            "CountKnowledgeArticles", "accepted", token);
        var input0 = new ApiAuthorization.UpdateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _authorizationAccessPolicyService.UpdateAccessPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.CountKnowledgeArticlesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "CountKnowledgeArticles", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching knowledge article.
    /// </summary>
    public override async Task ExportKnowledgeArticles(ApiHelpdesk.ExportKnowledgeArticlesRequest request, IServerStreamWriter<ApiHelpdesk.ExportKnowledgeArticlesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportKnowledgeArticles for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", "batch",
            "ExportKnowledgeArticles", "accepted", token);
        var input0 = new ApiLicensing.ValidateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ValidateLicenseAssignmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiLicensing.ValidateLicenseAssignmentRequest>();
        using var rpc0 = _licensingLicenseAssignmentValidationService.ValidateLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ValidateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                    PostalCode = (reply0.LicenseAssignment?.BillingAddress?.PostalCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.LicenseAssignment?.Audit?.Etag ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiDevices.ValidateManagedDeviceRequest>(json1);
        using var rpc1 = _devicesManagedDeviceValidationService.ValidateManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ExportKnowledgeArticlesResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Metadata = { (reply0.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ExportKnowledgeArticles", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists knowledge articles a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.FindKnowledgeArticlesResponse> FindKnowledgeArticles(ApiHelpdesk.FindKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindKnowledgeArticles for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", "batch",
            "FindKnowledgeArticles", "accepted", token);
        var response = new ApiHelpdesk.FindKnowledgeArticlesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "FindKnowledgeArticles", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.LookupKnowledgeArticleResponse> LookupKnowledgeArticle(ApiHelpdesk.LookupKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        _logger.LogInformation("Handling LookupKnowledgeArticle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "LookupKnowledgeArticle", "accepted", token);
        var input0 = new ApiHelpdesk.ExportTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ExportTicketsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
            },
        };
        using var stream0 = _helpdeskTicketQueryService.ExportTickets(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportTickets frame {Type}", change0.GetType().Name);
        }
        var response = new ApiHelpdesk.LookupKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.PeekKnowledgeArticleResponse> PeekKnowledgeArticle(ApiHelpdesk.PeekKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.KnowledgeArticleId, "knowledge_article_id");
        _logger.LogInformation("Handling PeekKnowledgeArticle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "PeekKnowledgeArticle", "accepted", token);
        var input0 = new ApiHelpdesk.SimulateSlaPolicyRequest
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
        using var rpc0 = _helpdeskSlaPolicyCalculationService.SimulateSlaPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.PeekKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                    {
                        Amount = (reply0.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists knowledge articles a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.SuggestKnowledgeArticlesResponse> SuggestKnowledgeArticles(ApiHelpdesk.SuggestKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestKnowledgeArticles for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", "batch",
            "SuggestKnowledgeArticles", "accepted", token);
        var input0 = new ApiHelpdesk.RecalculateKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RecalculateKnowledgeArticlesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskKnowledgeArticleCalculationService.RecalculateKnowledgeArticlesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.SuggestKnowledgeArticlesResponse
        {
            KnowledgeArticles = { new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.KnowledgeArticle?.BillingAddress?.Region ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "SuggestKnowledgeArticles", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.SummarizeKnowledgeArticlesResponse> SummarizeKnowledgeArticles(ApiHelpdesk.SummarizeKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeKnowledgeArticles for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article", request.KnowledgeArticleId,
            "SummarizeKnowledgeArticles", "accepted", token);
        var input0 = new ApiAuthorization.BatchGetAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _authorizationAccessPolicyService.BatchGetAccessPoliciesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiItsm.ExplainServiceAssetDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainServiceAssetDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _itsmServiceAssetValidationService.ExplainServiceAssetDecisionAsync);
        var input2 = new ApiItsm.LintServiceAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (reply1.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply1.ServiceAsset?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _itsmServiceAssetValidationService.LintServiceAssetsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiSearch.SummarizeSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.SummarizeSearchIndexesRequest.Types.Details
            {
                Cron = (request.Details?.Cron ?? ""),
                Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var named3 = _registry.Get<ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient>("acme.search.v1.SearchIndexQueryService");
        using var rpc3 = named3.SummarizeSearchIndexesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.SummarizeKnowledgeArticlesResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.KnowledgeArticleId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "SummarizeKnowledgeArticles", (request.KnowledgeArticleId ?? ""), token);
        return response;
    }
}
