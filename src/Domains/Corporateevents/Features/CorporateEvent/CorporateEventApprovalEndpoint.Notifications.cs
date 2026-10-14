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
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

public sealed class CorporateEventApprovalEndpointGetCorporateEventApprovalStatus0NotificationHandler(ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient client) : INotificationHandler
{
    public string Topic => "acme.corporateevents.v1.CorporateEventApprovalService/GetCorporateEventApprovalStatus/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiCorporateevents.ScheduleRegistrationExpiryRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ScheduleRegistrationExpiryAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
