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
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiSponsorships = Acme.Sponsorships.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for GetIncidentRates.
/// </summary>
public interface IIncidentLifecycleEndpointSendReinstateIncident1Port
{
    Task<ApiItsm.GetIncidentRatesResponse> SendAsync(ApiItsm.GetIncidentRatesRequest request, CancellationToken token);
}

public sealed class IncidentLifecycleEndpointSendReinstateIncident1PortAdapter(ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient client) : IIncidentLifecycleEndpointSendReinstateIncident1Port
{
    public async Task<ApiItsm.GetIncidentRatesResponse> SendAsync(ApiItsm.GetIncidentRatesRequest request, CancellationToken token)
    {
        using var rpc = client.GetIncidentRatesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class IncidentLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the SearchShipments request from the current workflow state.
    /// </summary>
    public static ApiShipping.SearchShipmentsRequest ReinstateIncidentToSearchShipments0(this ApiItsm.ReinstateIncidentRequest request)
    {
        return new ApiShipping.SearchShipmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiShipping.SearchShipmentsRequest.Types.Filter
            {
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
    }
}
