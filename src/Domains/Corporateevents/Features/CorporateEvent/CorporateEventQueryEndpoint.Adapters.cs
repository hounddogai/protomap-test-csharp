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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for EstimateVenue.
/// </summary>
public interface ICorporateEventQueryEndpointSendExportCorporateEvents2Port
{
    Task<ApiCorporateevents.EstimateVenueResponse> SendAsync(ApiCorporateevents.EstimateVenueRequest request, CancellationToken token);
}

public sealed class CorporateEventQueryEndpointSendExportCorporateEvents2PortAdapter(ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient client) : ICorporateEventQueryEndpointSendExportCorporateEvents2Port
{
    public async Task<ApiCorporateevents.EstimateVenueResponse> SendAsync(ApiCorporateevents.EstimateVenueRequest request, CancellationToken token)
    {
        using var rpc = client.EstimateVenueAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
