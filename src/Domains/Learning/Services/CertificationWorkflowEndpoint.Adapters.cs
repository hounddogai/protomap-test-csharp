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
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for BrowseDocumentFolders.
/// </summary>
public interface ICertificationWorkflowEndpointSendSignalCertificationWorkflow0Port
{
    Task<ApiDocuments.BrowseDocumentFoldersResponse> SendAsync(ApiDocuments.BrowseDocumentFoldersRequest request, CancellationToken token);
}

public sealed class CertificationWorkflowEndpointSendSignalCertificationWorkflow0PortAdapter(ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient client) : ICertificationWorkflowEndpointSendSignalCertificationWorkflow0Port
{
    public async Task<ApiDocuments.BrowseDocumentFoldersResponse> SendAsync(ApiDocuments.BrowseDocumentFoldersRequest request, CancellationToken token)
    {
        using var rpc = client.BrowseDocumentFoldersAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class CertificationWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the LintCandidates request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.LintCandidatesRequest RetryCertificationWorkflowToLintCandidates0(this ApiLearning.RetryCertificationWorkflowRequest request)
    {
        return new ApiRecruiting.LintCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.LintCandidatesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
    }
}
