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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Corporateevents;

public sealed class CorporateEventEndpointGetCorporateEvent0NotificationHandler(ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.corporateevents.v1.CorporateEventService/GetCorporateEvent/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiCorporateevents.CheckCorporateEventEligibilityRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CheckCorporateEventEligibilityAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
