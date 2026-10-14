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
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Employeeportal;

public sealed class SelfServiceRequestNotificationEndpointNotifySelfServiceRequestWatchers0NotificationHandler(ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.employeeportal.v1.SelfServiceRequestNotificationService/NotifySelfServiceRequestWatchers/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiEmployeeportal.ReplayPortalHomeEventsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ReplayPortalHomeEventsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
