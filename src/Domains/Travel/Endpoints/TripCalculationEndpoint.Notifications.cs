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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Travel;

public sealed class TripCalculationEndpointQuoteTrip0NotificationHandler(ApiIdentity.UserService.UserServiceClient client) : INotificationHandler
{
    public string Topic => "acme.travel.v1.TripCalculationService/QuoteTrip/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiIdentity.BatchGetUsersRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.BatchGetUsersAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
