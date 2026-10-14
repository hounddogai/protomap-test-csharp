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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for CompareTripOptions.
/// </summary>
public interface ITravelBookingCalculationEndpointSendEstimateTravelBooking0Port
{
    Task<ApiTravel.CompareTripOptionsResponse> SendAsync(ApiTravel.CompareTripOptionsRequest request, CancellationToken token);
}

public sealed class TravelBookingCalculationEndpointSendEstimateTravelBooking0PortAdapter(ApiTravel.TripCalculationService.TripCalculationServiceClient client) : ITravelBookingCalculationEndpointSendEstimateTravelBooking0Port
{
    public async Task<ApiTravel.CompareTripOptionsResponse> SendAsync(ApiTravel.CompareTripOptionsRequest request, CancellationToken token)
    {
        using var rpc = client.CompareTripOptionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
