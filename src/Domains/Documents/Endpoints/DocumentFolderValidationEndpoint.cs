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
/// Checks document folders against the rules before anything acts on them.
/// </summary>
public sealed class DocumentFolderValidationEndpoint : ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceBase
{
    private readonly ILogger<DocumentFolderValidationEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentFolderValidationEndpoint(
        ILogger<DocumentFolderValidationEndpoint> logger,
        IRecordRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Explains a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ExplainDocumentFolderDecisionResponse> ExplainDocumentFolderDecision(ApiDocuments.ExplainDocumentFolderDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainDocumentFolderDecision for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ExplainDocumentFolderDecisionResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                ParentId = (request.DocumentFolder?.ParentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ExplainDocumentFolderDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentFolderRulesResponse> GetDocumentFolderRules(ApiDocuments.GetDocumentFolderRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetDocumentFolderRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "GetDocumentFolderRules", "accepted", token);
        var response = new ApiDocuments.GetDocumentFolderRulesResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a document folder.
    /// </summary>
    public override async Task<ApiDocuments.LintDocumentFoldersResponse> LintDocumentFolders(ApiDocuments.LintDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintDocumentFolders for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.LintDocumentFoldersResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Depth = (request.DocumentFolder?.Depth ?? 0),
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "LintDocumentFolders", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a document folder.
    /// </summary>
    public override async Task<ApiDocuments.NormalizeDocumentFolderResponse> NormalizeDocumentFolder(ApiDocuments.NormalizeDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeDocumentFolder for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.NormalizeDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Location = (request.DocumentFolder?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "NormalizeDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a document folder.
    /// </summary>
    public override async Task<ApiDocuments.PreviewDocumentFolderResponse> PreviewDocumentFolder(ApiDocuments.PreviewDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewDocumentFolder for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.PreviewDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = (request.DocumentFolder?.Manager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "PreviewDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ValidateDocumentFolderResponse> ValidateDocumentFolder(ApiDocuments.ValidateDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateDocumentFolder for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ValidateDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = (request.DocumentFolder?.Manager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ValidateDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
}
