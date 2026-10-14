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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPatientapp = Acme.Patientapp.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The application port for PreviewSearchIndex.
/// </summary>
public interface IPortalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port
{
    Task<ApiSearch.PreviewSearchIndexResponse> SendAsync(ApiSearch.PreviewSearchIndexRequest request, CancellationToken token);
}

public sealed class PortalHomeNotificationEndpointSendSubscribePortalHomeUpdates1PortAdapter(ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient client) : IPortalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port
{
    public async Task<ApiSearch.PreviewSearchIndexResponse> SendAsync(ApiSearch.PreviewSearchIndexRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewSearchIndexAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
