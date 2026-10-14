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
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentTemplateNotificationEndpoint
{
    /// <summary>
    /// Returns a document template.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentTemplatePreferencesResponse> GetDocumentTemplatePreferences(ApiDocuments.GetDocumentTemplatePreferencesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        _logger.LogInformation("Handling GetDocumentTemplatePreferences for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "GetDocumentTemplatePreferences", "accepted", token);
        var input0 = new ApiDocuments.RetryDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentTemplateWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateWorkflowService.RetryDocumentTemplateWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDocuments.GetDocumentTemplatePreferencesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DocumentTemplate?.Audit?.Etag ?? ""),
                },
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists document templates a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentTemplateSubscriptionsResponse> ListDocumentTemplateSubscriptions(ApiDocuments.ListDocumentTemplateSubscriptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentTemplateSubscriptions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", "batch",
            "ListDocumentTemplateSubscriptions", "accepted", token);
        var response = new ApiDocuments.ListDocumentTemplateSubscriptionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ListDocumentTemplateSubscriptions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Notifys a document template.
    /// </summary>
    public override async Task<ApiDocuments.NotifyDocumentTemplateWatchersResponse> NotifyDocumentTemplateWatchers(ApiDocuments.NotifyDocumentTemplateWatchersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NotifyDocumentTemplateWatchers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "NotifyDocumentTemplateWatchers", "accepted", token);
        var response = new ApiDocuments.NotifyDocumentTemplateWatchersResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "NotifyDocumentTemplateWatchers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Sends a document template.
    /// </summary>
    public override async Task<ApiDocuments.SendDocumentTemplateTestNoticeResponse> SendDocumentTemplateTestNotice(ApiDocuments.SendDocumentTemplateTestNoticeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SendDocumentTemplateTestNotice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "SendDocumentTemplateTestNotice", "accepted", token);
        var response = new ApiDocuments.SendDocumentTemplateTestNoticeResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "SendDocumentTemplateTestNotice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of document templates as they happen.
    /// </summary>
    public override async Task StreamDocumentTemplateNotices(ApiDocuments.StreamDocumentTemplateNoticesRequest request, IServerStreamWriter<ApiDocuments.StreamDocumentTemplateNoticesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamDocumentTemplateNotices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", "batch",
            "StreamDocumentTemplateNotices", "accepted", token);
        var response = new ApiDocuments.StreamDocumentTemplateNoticesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "StreamDocumentTemplateNotices", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Subscribes a document template.
    /// </summary>
    public override async Task<ApiDocuments.SubscribeDocumentTemplateUpdatesResponse> SubscribeDocumentTemplateUpdates(ApiDocuments.SubscribeDocumentTemplateUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubscribeDocumentTemplateUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "SubscribeDocumentTemplateUpdates", "accepted", token);
        var input0 = new ApiDocuments.CompareDocumentFolderVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentFolderVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.Recipient?.Email ?? ""),
                        UserId = (request.Context?.Actor?.UserId ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderHistoryService.CompareDocumentFolderVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.SubscribeDocumentTemplateUpdatesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "SubscribeDocumentTemplateUpdates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unsubscribes a document template.
    /// </summary>
    public override async Task<ApiDocuments.UnsubscribeDocumentTemplateUpdatesResponse> UnsubscribeDocumentTemplateUpdates(ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UnsubscribeDocumentTemplateUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "UnsubscribeDocumentTemplateUpdates", "accepted", token);
        var input0 = new ApiMetrics.SearchMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.SearchMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _metricsMetricSeriesService.SearchMetricSeriesesAsync);
        var response = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Outcome = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "UnsubscribeDocumentTemplateUpdates", (request.Reason ?? ""), token);
        return response;
    }
}
