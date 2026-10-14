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

/// <summary>
/// Computes the figures of a signature, such as totals, taxes, and estimates.
/// </summary>
public sealed class SignatureCalculationEndpoint : ApiDocuments.SignatureCalculationService.SignatureCalculationServiceBase
{
    private readonly ILogger<SignatureCalculationEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public SignatureCalculationEndpoint(
        ILogger<SignatureCalculationEndpoint> logger,
        IRecordRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Calculates a signature.
    /// </summary>
    public override async Task<ApiDocuments.CalculateSignatureResponse> CalculateSignature(ApiDocuments.CalculateSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CalculateSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "CalculateSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a signature.
    /// </summary>
    public override async Task<ApiDocuments.EstimateSignatureResponse> EstimateSignature(ApiDocuments.EstimateSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.EstimateSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
                Subject = (request.Signature?.Subject ?? new ApiDocuments.DocumentRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Source ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "EstimateSignature", (request.Signature?.Source ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a signature.
    /// </summary>
    public override async Task<ApiDocuments.ExplainSignatureCalculationResponse> ExplainSignatureCalculation(ApiDocuments.ExplainSignatureCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainSignatureCalculation for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ExplainSignatureCalculationResponse
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
            "audit", "ExplainSignatureCalculation", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a signature.
    /// </summary>
    public override async Task<ApiDocuments.GetSignatureRatesResponse> GetSignatureRates(ApiDocuments.GetSignatureRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SignatureId, "signature_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetSignatureRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "signature", request.SignatureId,
            "GetSignatureRates", "accepted", token);
        var response = new ApiDocuments.GetSignatureRatesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a signature.
    /// </summary>
    public override async Task<ApiDocuments.QuoteSignatureResponse> QuoteSignature(ApiDocuments.QuoteSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.QuoteSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                OccurredAt = (request.Signature?.OccurredAt ?? new Timestamp()),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "QuoteSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a signature.
    /// </summary>
    public override async Task<ApiDocuments.RecalculateSignaturesResponse> RecalculateSignatures(ApiDocuments.RecalculateSignaturesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateSignatures for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RecalculateSignaturesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                OccurredAt = (request.Signature?.OccurredAt ?? new Timestamp()),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "RecalculateSignatures", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a signature.
    /// </summary>
    public override async Task<ApiDocuments.SimulateSignatureResponse> SimulateSignature(ApiDocuments.SimulateSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.SimulateSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (request.Signature?.CorrelationId ?? ""),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "SimulateSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
}
