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

public sealed partial class DocumentFolderExportEndpoint
{
    /// <summary>
    /// Cancels a document folder.
    /// </summary>
    public override async Task<ApiDocuments.CancelDocumentFolderExportResponse> CancelDocumentFolderExport(ApiDocuments.CancelDocumentFolderExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        _logger.LogInformation("Handling CancelDocumentFolderExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "CancelDocumentFolderExport", "accepted", token);
        var response = new ApiDocuments.CancelDocumentFolderExportResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
            Outcome = new ApiDocuments.CancelDocumentFolderExportResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "CancelDocumentFolderExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentFolderExportResponse> GetDocumentFolderExport(ApiDocuments.GetDocumentFolderExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        _logger.LogInformation("Handling GetDocumentFolderExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "GetDocumentFolderExport", "accepted", token);
        var response = new ApiDocuments.GetDocumentFolderExportResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists document folders a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentFolderExportsResponse> ListDocumentFolderExports(ApiDocuments.ListDocumentFolderExportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListDocumentFolderExports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "ListDocumentFolderExports", "accepted", token);
        var input0 = new ApiDocuments.RecalculateSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecalculateSignaturesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Source = (request.Filter?.OwnerId ?? ""),
            },
        };
        using var rpc0 = _documentsSignatureCalculationService.RecalculateSignaturesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListDocumentFolderExportsResponse
        {
            DocumentFolders = { new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Children = { new ApiDocuments.DocumentFolder
                        {
                            Attributes = { (reply0.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                        } },
                    } },
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ListDocumentFolderExports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a document folder.
    /// </summary>
    public override async Task<ApiDocuments.RetryDocumentFolderExportResponse> RetryDocumentFolderExport(ApiDocuments.RetryDocumentFolderExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryDocumentFolderExport for {Tenant}", requestContext.TenantId);
        if (request.DocumentFolder is { } record)
        {
            var snapshot = DocumentFolderMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RetryDocumentFolderExportResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = (request.DocumentFolder?.Manager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "RetryDocumentFolderExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ScheduleDocumentFolderExportResponse> ScheduleDocumentFolderExport(ApiDocuments.ScheduleDocumentFolderExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleDocumentFolderExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "ScheduleDocumentFolderExport", "accepted", token);
        var response = new ApiDocuments.ScheduleDocumentFolderExportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ScheduleDocumentFolderExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a document folder.
    /// </summary>
    public override async Task<ApiDocuments.ShareDocumentFolderExportResponse> ShareDocumentFolderExport(ApiDocuments.ShareDocumentFolderExportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareDocumentFolderExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "ShareDocumentFolderExport", "accepted", token);
        var response = new ApiDocuments.ShareDocumentFolderExportResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "ShareDocumentFolderExport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching document folder.
    /// </summary>
    public override async Task StreamDocumentFolderExport(ApiDocuments.StreamDocumentFolderExportRequest request, IServerStreamWriter<ApiDocuments.StreamDocumentFolderExportResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamDocumentFolderExport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "StreamDocumentFolderExport", "accepted", token);
        var response = new ApiDocuments.StreamDocumentFolderExportResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "StreamDocumentFolderExport", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
