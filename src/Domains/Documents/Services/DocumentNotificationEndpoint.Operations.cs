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
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentNotificationEndpoint
{
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentPreferencesResponse> GetDocumentPreferences(ApiDocuments.GetDocumentPreferencesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling GetDocumentPreferences for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "GetDocumentPreferences", "accepted", token);
        var input0 = new ApiDocuments.PauseDocumentTemplateWorkflowRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiDocuments.PauseDocumentTemplateWorkflowRequest.Types.Details
        {
            Notify = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        input0.DocumentTemplate = new ApiDocuments.DocumentTemplate
        {
            IssuedAt = (request.Context?.Deadline ?? new Timestamp()),
        };
        using var rpc0 = _documentsDocumentTemplateWorkflowService.PauseDocumentTemplateWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.GetDocumentPreferencesResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.DocumentTemplate?.BillingAddress?.CountryCode ?? ""),
                },
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists documents a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentSubscriptionsResponse> ListDocumentSubscriptions(ApiDocuments.ListDocumentSubscriptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentSubscriptions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "ListDocumentSubscriptions", "accepted", token);
        var input0 = new ApiAuthorization.ExplainAccessPolicyDecisionRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.ExplainAccessPolicyDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _authorizationAccessPolicyValidationService.ExplainAccessPolicyDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListDocumentSubscriptionsResponse
        {
            Documents = { new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.AccessPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ListDocumentSubscriptions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Notifys a document.
    /// </summary>
    public override async Task<ApiDocuments.NotifyDocumentWatchersResponse> NotifyDocumentWatchers(ApiDocuments.NotifyDocumentWatchersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NotifyDocumentWatchers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "NotifyDocumentWatchers", "accepted", token);
        var response = new ApiDocuments.NotifyDocumentWatchersResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "NotifyDocumentWatchers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Sends a document.
    /// </summary>
    public override async Task<ApiDocuments.SendDocumentTestNoticeResponse> SendDocumentTestNotice(ApiDocuments.SendDocumentTestNoticeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SendDocumentTestNotice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "SendDocumentTestNotice", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentExportService.ListDocumentExportsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.ListDocumentExportsRequest>("acme.documents.v1.DocumentNotificationService/SendDocumentTestNotice/0", input0.Clone()), token);
        var response = new ApiDocuments.SendDocumentTestNoticeResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "SendDocumentTestNotice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of documents as they happen.
    /// </summary>
    public override async Task StreamDocumentNotices(ApiDocuments.StreamDocumentNoticesRequest request, IServerStreamWriter<ApiDocuments.StreamDocumentNoticesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamDocumentNotices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", "batch",
            "StreamDocumentNotices", "accepted", token);
        var response = new ApiDocuments.StreamDocumentNoticesResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "StreamDocumentNotices", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Subscribes a document.
    /// </summary>
    public override async Task<ApiDocuments.SubscribeDocumentUpdatesResponse> SubscribeDocumentUpdates(ApiDocuments.SubscribeDocumentUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubscribeDocumentUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "SubscribeDocumentUpdates", "accepted", token);
        var input0 = new ApiDocuments.QuoteDocumentTemplateRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                IssuedAt = (request.Context?.Deadline ?? new Timestamp()),
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Attributes = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateCalculationService.QuoteDocumentTemplateAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDocuments.SubscribeDocumentUpdatesResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.DocumentTemplate?.BillingAddress?.CountryCode ?? ""),
                },
                DocumentId = (request.DocumentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.TemplateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "SubscribeDocumentUpdates", (request.Details?.TemplateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unsubscribes a document.
    /// </summary>
    public override async Task<ApiDocuments.UnsubscribeDocumentUpdatesResponse> UnsubscribeDocumentUpdates(ApiDocuments.UnsubscribeDocumentUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UnsubscribeDocumentUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "UnsubscribeDocumentUpdates", "accepted", token);
        var response = new ApiDocuments.UnsubscribeDocumentUpdatesResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "UnsubscribeDocumentUpdates", (request.Reason ?? ""), token);
        return response;
    }
}
