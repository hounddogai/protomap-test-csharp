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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Corporateevents;

public sealed class VenueCalculationEndpointQuoteVenue0NotificationHandler(ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.corporateevents.v1.VenueCalculationService/QuoteVenue/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiCorporateevents.SimulateCorporateEventRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SimulateCorporateEventAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
