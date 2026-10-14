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

public sealed partial class SignatureValidationEndpoint
{
    /// <summary>
    /// Checks a signature.
    /// </summary>
    public override async Task<ApiDocuments.CheckSignatureEligibilityResponse> CheckSignatureEligibility(ApiDocuments.CheckSignatureEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckSignatureEligibility for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CheckSignatureEligibilityResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Kind = (request.Signature?.Kind ?? ""),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "CheckSignatureEligibility", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a signature.
    /// </summary>
    public override async Task<ApiDocuments.ExplainSignatureDecisionResponse> ExplainSignatureDecision(ApiDocuments.ExplainSignatureDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainSignatureDecision for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ExplainSignatureDecisionResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Kind = (request.Signature?.Kind ?? ""),
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ExplainSignatureDecision", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints a signature.
    /// </summary>
    public override async Task<ApiDocuments.LintSignaturesResponse> LintSignatures(ApiDocuments.LintSignaturesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintSignatures for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.LintSignaturesResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "LintSignatures", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a signature.
    /// </summary>
    public override async Task<ApiDocuments.NormalizeSignatureResponse> NormalizeSignature(ApiDocuments.NormalizeSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.NormalizeSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "NormalizeSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a signature.
    /// </summary>
    public override async Task<ApiDocuments.PreviewSignatureResponse> PreviewSignature(ApiDocuments.PreviewSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Signature?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiDocuments.SignatureMeasurement>()).Select(async item =>
        {
            var input0 = new ApiDocuments.AnnotateSignatureVersionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.AnnotateSignatureVersionRequest.Types.Details
                {
                    Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                },
                Signature = new ApiDocuments.Signature
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDocuments.SignatureMeasurement
                    {
                        Metric = (item.Metric ?? ""),
                        ObservedAt = (item.ObservedAt ?? new Timestamp()),
                        Unit = (item.Unit ?? ""),
                        Value = item.Value,
                    } },
                },
            };
            using var rpc0 = _documentsSignatureHistoryService.AnnotateSignatureVersionAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDocuments.AnnotateSignatureVersionResponse();
        var response = new ApiDocuments.PreviewSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
                Source = (request.Signature?.Source ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "PreviewSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a signature.
    /// </summary>
    public override async Task<ApiDocuments.ScoreSignatureResponse> ScoreSignature(ApiDocuments.ScoreSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ScoreSignatureResponse
        {
            Breakdown = new ApiDocuments.ScoreSignatureResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Subject?.Number ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ScoreSignature", (request.Signature?.Subject?.Number ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a signature.
    /// </summary>
    public override async Task<ApiDocuments.ValidateSignatureResponse> ValidateSignature(ApiDocuments.ValidateSignatureRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateSignature for {Tenant}", requestContext.TenantId);
        if (request.Signature is { } record)
        {
            var snapshot = SignatureMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ValidateSignatureResponse
        {
            Signature = new ApiDocuments.Signature
            {
                SignatureId = (request.SignatureId ?? ""),
                Subject = (request.Signature?.Subject ?? new ApiDocuments.DocumentRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Signature?.Source ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "signature",
            "audit", "ValidateSignature", (request.Signature?.Source ?? ""), token);
        return response;
    }
}
