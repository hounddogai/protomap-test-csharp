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

namespace Acme.Workforce.Documents;

public sealed partial class SignatureSyncEndpoint
{
    /// <summary>
    /// Returns a signature.
    /// </summary>
    public override async Task<ApiDocuments.GetSignatureSyncStateResponse> GetSignatureSyncState(ApiDocuments.GetSignatureSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling GetSignatureSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "GetSignatureSyncState", "accepted", token);
        var response = new ApiDocuments.GetSignatureSyncStateResponse
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
    public override async Task<ApiDocuments.ListSignatureConflictsResponse> ListSignatureConflicts(ApiDocuments.ListSignatureConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListSignatureConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", "batch",
            "ListSignatureConflicts", "accepted", token);
        var response = new ApiDocuments.ListSignatureConflictsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ListSignatureConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls a signature.
    /// </summary>
    public override async Task<ApiDocuments.PullSignaturesResponse> PullSignatures(ApiDocuments.PullSignaturesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PullSignatures for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PullSignaturesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Sequence = (request.Signature?.Sequence ?? 0L),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "PullSignatures", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs a signature.
    /// </summary>
    public override async Task<ApiDocuments.PushSignatureResponse> PushSignature(ApiDocuments.PushSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PushSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PushSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Kind = (request.Signature?.Kind ?? ""),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "PushSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles a signature.
    /// </summary>
    public override async Task<ApiDocuments.ReconcileSignaturesResponse> ReconcileSignatures(ApiDocuments.ReconcileSignaturesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReconcileSignatures for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ReconcileSignaturesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
                Subject = (request.Signature?.Subject ?? new ApiDocuments.DocumentRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Source ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ReconcileSignatures", (request.Signature?.Source ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves a signature.
    /// </summary>
    public override async Task<ApiDocuments.ResolveSignatureConflictResponse> ResolveSignatureConflict(ApiDocuments.ResolveSignatureConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveSignatureConflict for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ResolveSignatureConflictResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.Signature?.CorrelationId ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ResolveSignatureConflict", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Syncs a signature.
    /// </summary>
    public override async Task<ApiDocuments.SyncSignaturesResponse> SyncSignatures(ApiDocuments.SyncSignaturesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SyncSignatures for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.SyncSignaturesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Kind = (request.Signature?.Kind ?? ""),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "SyncSignatures", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
}
