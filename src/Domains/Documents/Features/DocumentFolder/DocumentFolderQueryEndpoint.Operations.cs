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

public sealed partial class DocumentFolderQueryEndpoint
{
    /// <summary>
    /// Lists document folders a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.BrowseDocumentFoldersResponse> BrowseDocumentFolders(ApiDocuments.BrowseDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseDocumentFolders for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "BrowseDocumentFolders", "accepted", token);
        var response = new ApiDocuments.BrowseDocumentFoldersResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "BrowseDocumentFolders", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts document folders by status.
    /// </summary>
    public override async Task<ApiDocuments.CountDocumentFoldersResponse> CountDocumentFolders(ApiDocuments.CountDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountDocumentFolders for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "CountDocumentFolders", "accepted", token);
        var response = new ApiDocuments.CountDocumentFoldersResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "CountDocumentFolders", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching document folder.
    /// </summary>
    public override async Task ExportDocumentFolders(ApiDocuments.ExportDocumentFoldersRequest request, IServerStreamWriter<ApiDocuments.ExportDocumentFoldersResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportDocumentFolders for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "ExportDocumentFolders", "accepted", token);
        var response = new ApiDocuments.ExportDocumentFoldersResponse
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
            "audit", "ExportDocumentFolders", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists document folders a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.FindDocumentFoldersResponse> FindDocumentFolders(ApiDocuments.FindDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindDocumentFolders for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "FindDocumentFolders", "accepted", token);
        var response = new ApiDocuments.FindDocumentFoldersResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "FindDocumentFolders", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.LookupDocumentFolderResponse> LookupDocumentFolder(ApiDocuments.LookupDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        _logger.LogInformation("Handling LookupDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "LookupDocumentFolder", "accepted", token);
        var response = new ApiDocuments.LookupDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a document folder.
    /// </summary>
    public override async Task<ApiDocuments.PeekDocumentFolderResponse> PeekDocumentFolder(ApiDocuments.PeekDocumentFolderRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentFolderId, "document_folder_id");
        _logger.LogInformation("Handling PeekDocumentFolder for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", request.DocumentFolderId,
            "PeekDocumentFolder", "accepted", token);
        var response = new ApiDocuments.PeekDocumentFolderResponse
        {
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                DocumentFolderId = (request.DocumentFolderId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several document folders at once.
    /// </summary>
    public override async Task<ApiDocuments.ResolveDocumentFoldersResponse> ResolveDocumentFolders(ApiDocuments.ResolveDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveDocumentFolders for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "ResolveDocumentFolders", "accepted", token);
        var response = new ApiDocuments.ResolveDocumentFoldersResponse();
        return response;
    }
    /// <summary>
    /// Lists document folders a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.SuggestDocumentFoldersResponse> SuggestDocumentFolders(ApiDocuments.SuggestDocumentFoldersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestDocumentFolders for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_folder", "batch",
            "SuggestDocumentFolders", "accepted", token);
        var response = new ApiDocuments.SuggestDocumentFoldersResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_folder",
            "audit", "SuggestDocumentFolders", (request.Page?.PageToken ?? ""), token);
        return response;
    }
}
