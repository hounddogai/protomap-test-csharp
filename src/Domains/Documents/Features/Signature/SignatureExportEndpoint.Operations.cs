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
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Documents;

public sealed partial class SignatureExportEndpoint
{
    /// <summary>
    /// Cancels a signature.
    /// </summary>
    public override async Task<ApiDocuments.CancelSignatureExportResponse> CancelSignatureExport(ApiDocuments.CancelSignatureExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling CancelSignatureExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "CancelSignatureExport", "accepted", token);
        var input0 = new ApiDocuments.GetSignatureRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SignatureId = (request.SignatureId ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _documentsSignatureCalculationService.GetSignatureRatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.CancelSignatureExportResponse
        {
            Outcome = new ApiDocuments.CancelSignatureExportResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
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
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "CancelSignatureExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a signature.
    /// </summary>
    public override async Task<ApiDocuments.GetSignatureExportResponse> GetSignatureExport(ApiDocuments.GetSignatureExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling GetSignatureExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "GetSignatureExport", "accepted", token);
        var response = new ApiDocuments.GetSignatureExportResponse
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
    public override async Task<ApiDocuments.ListSignatureExportsResponse> ListSignatureExports(ApiDocuments.ListSignatureExportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListSignatureExports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", "batch",
            "ListSignatureExports", "accepted", token);
        var response = new ApiDocuments.ListSignatureExportsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ListSignatureExports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a signature.
    /// </summary>
    public override async Task<ApiDocuments.RetrySignatureExportResponse> RetrySignatureExport(ApiDocuments.RetrySignatureExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetrySignatureExport for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RetrySignatureExportResponse
        {
            Signature = new ApiDocuments.Signature
            {
                OccurredAt = (request.Signature?.OccurredAt ?? new Timestamp()),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "RetrySignatureExport", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a signature.
    /// </summary>
    public override async Task<ApiDocuments.ScheduleSignatureExportResponse> ScheduleSignatureExport(ApiDocuments.ScheduleSignatureExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleSignatureExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "ScheduleSignatureExport", "accepted", token);
        var response = new ApiDocuments.ScheduleSignatureExportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SignatureId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ScheduleSignatureExport", (request.SignatureId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a signature.
    /// </summary>
    public override async Task<ApiDocuments.ShareSignatureExportResponse> ShareSignatureExport(ApiDocuments.ShareSignatureExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareSignatureExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "ShareSignatureExport", "accepted", token);
        var input0 = new ApiNotifications.ValidateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.ValidateNotificationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
            Notification = new ApiNotifications.Notification
            {
                Channel = new ApiNotifications.NotificationChannelOrigin
                {
                    Channel = (request.Details?.Channel ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiNotifications.ValidateNotificationRequest>();
        using var rpc0 = _notificationsNotificationValidationService.ValidateNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ShareSignatureExportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ShareSignatureExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching signature.
    /// </summary>
    public override async Task StreamSignatureExport(ApiDocuments.StreamSignatureExportRequest request, IServerStreamWriter<ApiDocuments.StreamSignatureExportResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamSignatureExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", "batch",
            "StreamSignatureExport", "accepted", token);
        var response = new ApiDocuments.StreamSignatureExportResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "StreamSignatureExport", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
