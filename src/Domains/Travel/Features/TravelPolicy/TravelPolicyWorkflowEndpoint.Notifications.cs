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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed class TravelPolicyWorkflowEndpointCancelTravelPolicyWorkflow1NotificationHandler(ApiDocuments.SignatureSyncService.SignatureSyncServiceClient client) : INotificationHandler
{
    public string Topic => "acme.travel.v1.TravelPolicyWorkflowService/CancelTravelPolicyWorkflow/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDocuments.SyncSignaturesRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SyncSignaturesAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
