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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

public sealed class TimesheetQueryEndpointSummarizeTimesheets0NotificationHandler(ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.timekeeping.v1.TimesheetQueryService/SummarizeTimesheets/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiTimekeeping.ScoreClockEventRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ScoreClockEventAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
