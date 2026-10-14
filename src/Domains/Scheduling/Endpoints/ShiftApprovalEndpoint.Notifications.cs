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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTreasury = Acme.Treasury.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

public sealed class ShiftApprovalEndpointGetShiftApprovalStatus1NotificationHandler(ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient client) : INotificationHandler
{
    public string Topic => "acme.scheduling.v1.ShiftApprovalService/GetShiftApprovalStatus/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiPerformance.RejectFeedbackRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.RejectFeedbackAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
