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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for CountVenues.
/// </summary>
public interface IAvailabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port
{
    Task<ApiVenues.CountVenuesResponse> SendAsync(ApiVenues.CountVenuesRequest request, CancellationToken token);
}

public sealed class AvailabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1PortAdapter(ApiVenues.VenueQueryService.VenueQueryServiceClient client) : IAvailabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port
{
    public async Task<ApiVenues.CountVenuesResponse> SendAsync(ApiVenues.CountVenuesRequest request, CancellationToken token)
    {
        using var rpc = client.CountVenuesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
