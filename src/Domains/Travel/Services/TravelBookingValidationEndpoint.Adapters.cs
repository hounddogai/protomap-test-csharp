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
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for CompareTripOptions.
/// </summary>
public interface ITravelBookingValidationEndpointSendScoreTravelBooking0Port
{
    Task<ApiTravel.CompareTripOptionsResponse> SendAsync(ApiTravel.CompareTripOptionsRequest request, CancellationToken token);
}

public sealed class TravelBookingValidationEndpointSendScoreTravelBooking0PortAdapter(ApiTravel.TripCalculationService.TripCalculationServiceClient client) : ITravelBookingValidationEndpointSendScoreTravelBooking0Port
{
    public async Task<ApiTravel.CompareTripOptionsResponse> SendAsync(ApiTravel.CompareTripOptionsRequest request, CancellationToken token)
    {
        using var rpc = client.CompareTripOptionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
