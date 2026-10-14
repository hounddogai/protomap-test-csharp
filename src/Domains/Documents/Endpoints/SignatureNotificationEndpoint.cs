using Acme.Workforce.Platform;
using Google.Protobuf;
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
/// Tells the people who follow a signature what changed.
/// </summary>
public sealed class SignatureNotificationEndpoint : ApiDocuments.SignatureNotificationService.SignatureNotificationServiceBase
{
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ILogger<SignatureNotificationEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public SignatureNotificationEndpoint(
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ILogger<SignatureNotificationEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Returns a signature.
    /// </summary>
    public override async Task<ApiDocuments.GetSignaturePreferencesResponse> GetSignaturePreferences(ApiDocuments.GetSignaturePreferencesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetSignaturePreferences for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "GetSignaturePreferences", "accepted", token);
        var response = new ApiDocuments.GetSignaturePreferencesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists signatures a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListSignatureSubscriptionsResponse> ListSignatureSubscriptions(ApiDocuments.ListSignatureSubscriptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListSignatureSubscriptions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", "batch",
            "ListSignatureSubscriptions", "accepted", token);
        var input0 = new ApiDocuments.RetryDocumentExportRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiDocuments.RetryDocumentExportRequest.Types.Details
        {
            Notify = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        input0.Document = new ApiDocuments.Document
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var rpc0 = _documentsDocumentExportService.RetryDocumentExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListSignatureSubscriptionsResponse
        {
            Signatures = { new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ListSignatureSubscriptions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Notifys a signature.
    /// </summary>
    public override async Task<ApiDocuments.NotifySignatureWatchersResponse> NotifySignatureWatchers(ApiDocuments.NotifySignatureWatchersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NotifySignatureWatchers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "NotifySignatureWatchers", "accepted", token);
        var input0 = new ApiDocuments.SyncDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Recipient?.Email ?? ""),
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentFolderSyncService.SyncDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.NotifySignatureWatchersResponse
        {
            Outcome = new ApiDocuments.NotifySignatureWatchersResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.TemplateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "NotifySignatureWatchers", (request.Details?.TemplateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Sends a signature.
    /// </summary>
    public override async Task<ApiDocuments.SendSignatureTestNoticeResponse> SendSignatureTestNotice(ApiDocuments.SendSignatureTestNoticeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SendSignatureTestNotice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "SendSignatureTestNotice", "accepted", token);
        var response = new ApiDocuments.SendSignatureTestNoticeResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "SendSignatureTestNotice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of signatures as they happen.
    /// </summary>
    public override async Task StreamSignatureNotices(ApiDocuments.StreamSignatureNoticesRequest request, IServerStreamWriter<ApiDocuments.StreamSignatureNoticesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StreamSignatureNotices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", "batch",
            "StreamSignatureNotices", "accepted", token);
        var response = new ApiDocuments.StreamSignatureNoticesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "StreamSignatureNotices", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Subscribes a signature.
    /// </summary>
    public override async Task<ApiDocuments.SubscribeSignatureUpdatesResponse> SubscribeSignatureUpdates(ApiDocuments.SubscribeSignatureUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubscribeSignatureUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "SubscribeSignatureUpdates", "accepted", token);
        var response = new ApiDocuments.SubscribeSignatureUpdatesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "SubscribeSignatureUpdates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unsubscribes a signature.
    /// </summary>
    public override async Task<ApiDocuments.UnsubscribeSignatureUpdatesResponse> UnsubscribeSignatureUpdates(ApiDocuments.UnsubscribeSignatureUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UnsubscribeSignatureUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "UnsubscribeSignatureUpdates", "accepted", token);
        var response = new ApiDocuments.UnsubscribeSignatureUpdatesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "UnsubscribeSignatureUpdates", (request.Reason ?? ""), token);
        return response;
    }
}
