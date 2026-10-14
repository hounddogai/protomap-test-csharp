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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

public sealed class InterviewReportingEndpointScheduleInterviewReport2NotificationHandler(ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient client) : INotificationHandler
{
    public string Topic => "acme.recruiting.v1.InterviewReportingService/ScheduleInterviewReport/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.SyncInterviewsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SyncInterviewsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
