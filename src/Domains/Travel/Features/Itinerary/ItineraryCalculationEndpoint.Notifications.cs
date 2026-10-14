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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTours = Acme.Tours.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed class ItineraryCalculationEndpointRecalculateItineraries1NotificationHandler(ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.travel.v1.ItineraryCalculationService/RecalculateItineraries/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiTravel.CheckTravelBookingEligibilityRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CheckTravelBookingEligibilityAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
