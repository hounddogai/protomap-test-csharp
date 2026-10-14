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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for SuggestOrgUnits.
/// </summary>
public interface IItineraryLifecycleEndpointSendSuspendItinerary0Port
{
    Task<ApiEmployees.SuggestOrgUnitsResponse> SendAsync(ApiEmployees.SuggestOrgUnitsRequest request, CancellationToken token);
}

public sealed class ItineraryLifecycleEndpointSendSuspendItinerary0PortAdapter(ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient client) : IItineraryLifecycleEndpointSendSuspendItinerary0Port
{
    public async Task<ApiEmployees.SuggestOrgUnitsResponse> SendAsync(ApiEmployees.SuggestOrgUnitsRequest request, CancellationToken token)
    {
        using var rpc = client.SuggestOrgUnitsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
