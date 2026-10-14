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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for GenerateTravelBookingReport.
/// </summary>
public interface IItinerarySyncEndpointSendSyncItineraries0Port
{
    Task<ApiTravel.GenerateTravelBookingReportResponse> SendAsync(ApiTravel.GenerateTravelBookingReportRequest request, CancellationToken token);
}

public sealed class ItinerarySyncEndpointSendSyncItineraries0PortAdapter(ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient client) : IItinerarySyncEndpointSendSyncItineraries0Port
{
    public async Task<ApiTravel.GenerateTravelBookingReportResponse> SendAsync(ApiTravel.GenerateTravelBookingReportRequest request, CancellationToken token)
    {
        using var rpc = client.GenerateTravelBookingReportAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ItinerarySyncEndpointRequests
{
    /// <summary>
    /// Builds the ResolveOrgUnits request from the current workflow state.
    /// </summary>
    public static ApiEmployees.ResolveOrgUnitsRequest StreamItineraryChangesToResolveOrgUnits1(this ApiTravel.StreamItineraryChangesRequest request)
    {
        return new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
