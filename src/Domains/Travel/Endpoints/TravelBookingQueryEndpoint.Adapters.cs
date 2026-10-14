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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for EstimateRegistration.
/// </summary>
public interface ITravelBookingQueryEndpointSendExportTravelBookings0Port
{
    Task<ApiCorporateevents.EstimateRegistrationResponse> SendAsync(ApiCorporateevents.EstimateRegistrationRequest request, CancellationToken token);
}

public sealed class TravelBookingQueryEndpointSendExportTravelBookings0PortAdapter(ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient client) : ITravelBookingQueryEndpointSendExportTravelBookings0Port
{
    public async Task<ApiCorporateevents.EstimateRegistrationResponse> SendAsync(ApiCorporateevents.EstimateRegistrationRequest request, CancellationToken token)
    {
        using var rpc = client.EstimateRegistrationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
