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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for ExplainDocumentFolderDecision.
/// </summary>
public interface IVenueReportingEndpointSendShareVenueReport1Port
{
    Task<ApiDocuments.ExplainDocumentFolderDecisionResponse> SendAsync(ApiDocuments.ExplainDocumentFolderDecisionRequest request, CancellationToken token);
}

public sealed class VenueReportingEndpointSendShareVenueReport1PortAdapter(ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient client) : IVenueReportingEndpointSendShareVenueReport1Port
{
    public async Task<ApiDocuments.ExplainDocumentFolderDecisionResponse> SendAsync(ApiDocuments.ExplainDocumentFolderDecisionRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainDocumentFolderDecisionAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
