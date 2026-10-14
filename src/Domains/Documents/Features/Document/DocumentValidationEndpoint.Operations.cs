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

public sealed partial class DocumentValidationEndpoint
{
    /// <summary>
    /// Checks a document.
    /// </summary>
    public override async Task<ApiDocuments.CheckDocumentEligibilityResponse> CheckDocumentEligibility(ApiDocuments.CheckDocumentEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckDocumentEligibility for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CheckDocumentEligibilityResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Notes = (request.Document?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "CheckDocumentEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentRulesResponse> GetDocumentRules(ApiDocuments.GetDocumentRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentId, "document_id");
        _logger.LogInformation("Handling GetDocumentRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document", request.DocumentId,
            "GetDocumentRules", "accepted", token);
        var input0 = new ApiDocuments.ReassignDocumentRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReassignDocumentRequest.Types.Details
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            DocumentId = (request.DocumentId ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _documentsDocumentAdminService.ReassignDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.GetDocumentRulesResponse
        {
            Document = new ApiDocuments.Document
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply0.Document?.Counterparty?.DisplayName ?? ""),
                },
                DocumentId = (request.DocumentId ?? ""),
                DueAt = (reply0.Document?.DueAt ?? new Timestamp()),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a document.
    /// </summary>
    public override async Task<ApiDocuments.LintDocumentsResponse> LintDocuments(ApiDocuments.LintDocumentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintDocuments for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.LintDocumentsResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = (request.Document?.BillingAddress ?? new ApiCommon.PostalAddress()),
                DocumentId = (request.DocumentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "LintDocuments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a document.
    /// </summary>
    public override async Task<ApiDocuments.NormalizeDocumentResponse> NormalizeDocument(ApiDocuments.NormalizeDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeDocument for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.NormalizeDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Notes = (request.Document?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "NormalizeDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a document.
    /// </summary>
    public override async Task<ApiDocuments.PreviewDocumentResponse> PreviewDocument(ApiDocuments.PreviewDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewDocument for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PushSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Document?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureSyncService.PushSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.PreviewDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                BillingAddress = (request.Document?.BillingAddress ?? new ApiCommon.PostalAddress()),
                DocumentId = (request.DocumentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Signature?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "PreviewDocument", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a document.
    /// </summary>
    public override async Task<ApiDocuments.ValidateDocumentResponse> ValidateDocument(ApiDocuments.ValidateDocumentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateDocument for {Tenant}", requestContext.TenantId);
        if (request.Document is { } record)
        {
            var snapshot = DocumentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ValidateDocumentResponse
        {
            Document = new ApiDocuments.Document
            {
                DocumentId = (request.DocumentId ?? ""),
                Number = (request.Document?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document",
            "audit", "ValidateDocument", (request.Reason ?? ""), token);
        return response;
    }
}
