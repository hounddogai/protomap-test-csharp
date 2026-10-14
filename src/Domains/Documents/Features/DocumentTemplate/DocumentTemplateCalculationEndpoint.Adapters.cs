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

namespace Acme.Workforce.Documents;

/// <summary>
/// The application port for ListDocumentFolderConflicts.
/// </summary>
public interface IDocumentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port
{
    Task<ApiDocuments.ListDocumentFolderConflictsResponse> SendAsync(ApiDocuments.ListDocumentFolderConflictsRequest request, CancellationToken token);
}

public sealed class DocumentTemplateCalculationEndpointSendEstimateDocumentTemplate0PortAdapter(ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient client) : IDocumentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port
{
    public async Task<ApiDocuments.ListDocumentFolderConflictsResponse> SendAsync(ApiDocuments.ListDocumentFolderConflictsRequest request, CancellationToken token)
    {
        using var rpc = client.ListDocumentFolderConflictsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
