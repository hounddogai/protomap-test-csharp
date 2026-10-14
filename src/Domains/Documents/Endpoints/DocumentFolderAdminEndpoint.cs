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
/// Operator actions on document folders that support staff run by hand.
/// </summary>
public sealed class DocumentFolderAdminEndpoint : ApiDocuments.DocumentFolderAdminService.DocumentFolderAdminServiceBase
{
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ILogger<DocumentFolderAdminEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentFolderAdminEndpoint(
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ILogger<DocumentFolderAdminEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Archives a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ArchiveDocumentFolderResponse> ArchiveDocumentFolder(ApiDocuments.ArchiveDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ArchiveDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "ArchiveDocumentFolder", "accepted", token);
        var response = new ApiDocuments.ArchiveDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
            Outcome = new ApiDocuments.ArchiveDocumentFolderResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ArchiveDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.InspectDocumentFolderResponse> InspectDocumentFolder(ApiDocuments.InspectDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "InspectDocumentFolder", "accepted", token);
        var response = new ApiDocuments.InspectDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Locks a document folder.
    /// </summary>
    public override async Task<ApiDocuments.LockDocumentFolderResponse> LockDocumentFolder(ApiDocuments.LockDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LockDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "LockDocumentFolder", "accepted", token);
        var response = new ApiDocuments.LockDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
            Outcome = new ApiDocuments.LockDocumentFolderResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "LockDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a document folder.
    /// </summary>
    public override async Task<ApiDocuments.PurgeDocumentFolderResponse> PurgeDocumentFolder(ApiDocuments.PurgeDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PurgeDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "PurgeDocumentFolder", "accepted", token);
        var response = new ApiDocuments.PurgeDocumentFolderResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "PurgeDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reassigns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ReassignDocumentFolderResponse> ReassignDocumentFolder(ApiDocuments.ReassignDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReassignDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "ReassignDocumentFolder", "accepted", token);
        var input0 = new ApiDocuments.EstimateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureCalculationService.EstimateSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ReassignDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                        },
                    } },
                } },
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Assignee?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ReassignDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Restores a document folder.
    /// </summary>
    public override async Task<ApiDocuments.RestoreDocumentFolderResponse> RestoreDocumentFolder(ApiDocuments.RestoreDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RestoreDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "RestoreDocumentFolder", "accepted", token);
        var response = new ApiDocuments.RestoreDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
            Outcome = new ApiDocuments.RestoreDocumentFolderResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "RestoreDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Transfers a document folder.
    /// </summary>
    public override async Task<ApiDocuments.TransferDocumentFolderResponse> TransferDocumentFolder(ApiDocuments.TransferDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TransferDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "TransferDocumentFolder", "accepted", token);
        var input0 = new ApiDocuments.CompareDocumentTemplateOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    DocumentFolderId = (request.DocumentFolderId ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _documentsDocumentTemplateCalculationService.CompareDocumentTemplateOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.TransferDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (reply0.DocumentTemplate?.Audit?.Etag ?? ""),
                    },
                } },
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
            Outcome = new ApiDocuments.TransferDocumentFolderResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "TransferDocumentFolder", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unlocks a document folder.
    /// </summary>
    public override async Task<ApiDocuments.UnlockDocumentFolderResponse> UnlockDocumentFolder(ApiDocuments.UnlockDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UnlockDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "UnlockDocumentFolder", "accepted", token);
        var response = new ApiDocuments.UnlockDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
            Outcome = new ApiDocuments.UnlockDocumentFolderResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "UnlockDocumentFolder", (request.Reason ?? ""), token);
        return response;
    }
}
