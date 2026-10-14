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
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Documents;

public sealed partial class SignatureHistoryEndpoint
{
    /// <summary>
    /// Annotates a signature.
    /// </summary>
    public override async Task<ApiDocuments.AnnotateSignatureVersionResponse> AnnotateSignatureVersion(ApiDocuments.AnnotateSignatureVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AnnotateSignatureVersion for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.AnnotateSignatureVersionResponse
        {
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (request.Signature?.CorrelationId ?? ""),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "AnnotateSignatureVersion", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a signature.
    /// </summary>
    public override async Task<ApiDocuments.CompareSignatureVersionsResponse> CompareSignatureVersions(ApiDocuments.CompareSignatureVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareSignatureVersions for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CompareSignatureVersionsResponse
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
            "audit", "CompareSignatureVersions", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a signature.
    /// </summary>
    public override async Task<ApiDocuments.GetSignatureVersionResponse> GetSignatureVersion(ApiDocuments.GetSignatureVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling GetSignatureVersion for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "GetSignatureVersion", "accepted", token);
        var response = new ApiDocuments.GetSignatureVersionResponse
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
    public override async Task<ApiDocuments.ListSignatureVersionsResponse> ListSignatureVersions(ApiDocuments.ListSignatureVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListSignatureVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", "batch",
            "ListSignatureVersions", "accepted", token);
        var input0 = new ApiVenues.CountVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OrderBy = (request.OrderBy ?? ""),
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiVenues.CountVenuesRequest>(json0);
        using var rpc0 = _venuesVenueQueryService.CountVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListSignatureVersionsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ListSignatureVersions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a signature.
    /// </summary>
    public override async Task<ApiDocuments.PruneSignatureVersionsResponse> PruneSignatureVersions(ApiDocuments.PruneSignatureVersionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        _logger.LogInformation("Handling PruneSignatureVersions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "PruneSignatureVersions", "accepted", token);
        var response = new ApiDocuments.PruneSignatureVersionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.SignatureId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "PruneSignatureVersions", (request.SignatureId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Records a signature.
    /// </summary>
    public override async Task<ApiDocuments.RecordSignatureVersionResponse> RecordSignatureVersion(ApiDocuments.RecordSignatureVersionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecordSignatureVersion for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RecordSignatureVersionResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Sequence = (request.Signature?.Sequence ?? 0L),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "RecordSignatureVersion", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
}
