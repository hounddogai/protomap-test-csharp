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

namespace Acme.Workforce.Documents;

public sealed partial class SignatureAdminEndpoint
{
    /// <summary>
    /// Archives a signature.
    /// </summary>
    public override async Task<ApiDocuments.ArchiveSignatureResponse> ArchiveSignature(ApiDocuments.ArchiveSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling ArchiveSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "ArchiveSignature", "accepted", token);
        var response = new ApiDocuments.ArchiveSignatureResponse
        {
            Outcome = new ApiDocuments.ArchiveSignatureResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ArchiveSignature", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a signature.
    /// </summary>
    public override async Task<ApiDocuments.InspectSignatureResponse> InspectSignature(ApiDocuments.InspectSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling InspectSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "InspectSignature", "accepted", token);
        var response = new ApiDocuments.InspectSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Locks a signature.
    /// </summary>
    public override async Task<ApiDocuments.LockSignatureResponse> LockSignature(ApiDocuments.LockSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling LockSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "LockSignature", "accepted", token);
        var response = new ApiDocuments.LockSignatureResponse
        {
            Outcome = new ApiDocuments.LockSignatureResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "LockSignature", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a signature.
    /// </summary>
    public override async Task<ApiDocuments.PurgeSignatureResponse> PurgeSignature(ApiDocuments.PurgeSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling PurgeSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "PurgeSignature", "accepted", token);
        var input0 = new ApiDocuments.ReconcileDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentSyncService.ReconcileDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.PurgeSignatureResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SignatureId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "PurgeSignature", (request.SignatureId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reassigns a signature.
    /// </summary>
    public override async Task<ApiDocuments.ReassignSignatureResponse> ReassignSignature(ApiDocuments.ReassignSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling ReassignSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "ReassignSignature", "accepted", token);
        var response = new ApiDocuments.ReassignSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ReassignSignature", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Repairs a signature.
    /// </summary>
    public override async Task<ApiDocuments.RepairSignatureResponse> RepairSignature(ApiDocuments.RepairSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RepairSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RepairSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Sequence = (request.Signature?.Sequence ?? 0L),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "RepairSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Restores a signature.
    /// </summary>
    public override async Task<ApiDocuments.RestoreSignatureResponse> RestoreSignature(ApiDocuments.RestoreSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling RestoreSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "RestoreSignature", "accepted", token);
        var response = new ApiDocuments.RestoreSignatureResponse
        {
            Outcome = new ApiDocuments.RestoreSignatureResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "RestoreSignature", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Transfers a signature.
    /// </summary>
    public override async Task<ApiDocuments.TransferSignatureResponse> TransferSignature(ApiDocuments.TransferSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling TransferSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "TransferSignature", "accepted", token);
        var response = new ApiDocuments.TransferSignatureResponse
        {
            Outcome = new ApiDocuments.TransferSignatureResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "TransferSignature", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unlocks a signature.
    /// </summary>
    public override async Task<ApiDocuments.UnlockSignatureResponse> UnlockSignature(ApiDocuments.UnlockSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling UnlockSignature for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "UnlockSignature", "accepted", token);
        var response = new ApiDocuments.UnlockSignatureResponse
        {
            Outcome = new ApiDocuments.UnlockSignatureResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "UnlockSignature", (request.Reason ?? ""), token);
        return response;
    }
}
