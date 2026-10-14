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

namespace Acme.Workforce.Documents;

/// <summary>
/// Tells the people who follow a document folder what changed.
/// </summary>
public sealed class DocumentFolderNotificationEndpoint : ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceBase
{
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ILogger<DocumentFolderNotificationEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentFolderNotificationEndpoint(
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ILogger<DocumentFolderNotificationEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentFolderPreferencesResponse> GetDocumentFolderPreferences(ApiDocuments.GetDocumentFolderPreferencesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetDocumentFolderPreferences for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "GetDocumentFolderPreferences", "accepted", token);
        var response = new ApiDocuments.GetDocumentFolderPreferencesResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists document folders a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentFolderSubscriptionsResponse> ListDocumentFolderSubscriptions(ApiDocuments.ListDocumentFolderSubscriptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListDocumentFolderSubscriptions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "ListDocumentFolderSubscriptions", "accepted", token);
        var input0 = new ApiDocuments.SyncDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.SyncDocumentTemplatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListDocumentFolderSubscriptionsResponse
        {
            DocumentFolders = { new ApiDocuments.DocumentFolder
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.DocumentTemplate?.BillingAddress?.Recipient ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ListDocumentFolderSubscriptions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Notifys a document folder.
    /// </summary>
    public override async Task<ApiDocuments.NotifyDocumentFolderWatchersResponse> NotifyDocumentFolderWatchers(ApiDocuments.NotifyDocumentFolderWatchersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NotifyDocumentFolderWatchers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "NotifyDocumentFolderWatchers", "accepted", token);
        var response = new ApiDocuments.NotifyDocumentFolderWatchersResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "NotifyDocumentFolderWatchers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Sends a document folder.
    /// </summary>
    public override async Task<ApiDocuments.SendDocumentFolderTestNoticeResponse> SendDocumentFolderTestNotice(ApiDocuments.SendDocumentFolderTestNoticeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SendDocumentFolderTestNotice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "SendDocumentFolderTestNotice", "accepted", token);
        var response = new ApiDocuments.SendDocumentFolderTestNoticeResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "SendDocumentFolderTestNotice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of document folders as they happen.
    /// </summary>
    public override async Task StreamDocumentFolderNotices(ApiDocuments.StreamDocumentFolderNoticesRequest request, IServerStreamWriter<ApiDocuments.StreamDocumentFolderNoticesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StreamDocumentFolderNotices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "StreamDocumentFolderNotices", "accepted", token);
        var response = new ApiDocuments.StreamDocumentFolderNoticesResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "StreamDocumentFolderNotices", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Subscribes a document folder.
    /// </summary>
    public override async Task<ApiDocuments.SubscribeDocumentFolderUpdatesResponse> SubscribeDocumentFolderUpdates(ApiDocuments.SubscribeDocumentFolderUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubscribeDocumentFolderUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "SubscribeDocumentFolderUpdates", "accepted", token);
        var response = new ApiDocuments.SubscribeDocumentFolderUpdatesResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "SubscribeDocumentFolderUpdates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unsubscribes a document folder.
    /// </summary>
    public override async Task<ApiDocuments.UnsubscribeDocumentFolderUpdatesResponse> UnsubscribeDocumentFolderUpdates(ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UnsubscribeDocumentFolderUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "UnsubscribeDocumentFolderUpdates", "accepted", token);
        var response = new ApiDocuments.UnsubscribeDocumentFolderUpdatesResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "UnsubscribeDocumentFolderUpdates", (request.Reason ?? ""), token);
        return response;
    }
}
