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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for SuggestDocumentFolders.
/// </summary>
public interface IKnowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port
{
    Task<ApiDocuments.SuggestDocumentFoldersResponse> SendAsync(ApiDocuments.SuggestDocumentFoldersRequest request, CancellationToken token);
}

public sealed class KnowledgeArticleValidationEndpointSendLintKnowledgeArticles1PortAdapter(ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient client) : IKnowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port
{
    public async Task<ApiDocuments.SuggestDocumentFoldersResponse> SendAsync(ApiDocuments.SuggestDocumentFoldersRequest request, CancellationToken token)
    {
        using var rpc = client.SuggestDocumentFoldersAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class KnowledgeArticleValidationEndpointRequests
{
    /// <summary>
    /// Builds the ResolveWebhookEndpoints request from the current workflow state.
    /// </summary>
    public static ApiWebhooks.ResolveWebhookEndpointsRequest LintKnowledgeArticlesToResolveWebhookEndpoints0(this ApiHelpdesk.LintKnowledgeArticlesRequest request)
    {
        return new ApiWebhooks.ResolveWebhookEndpointsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
