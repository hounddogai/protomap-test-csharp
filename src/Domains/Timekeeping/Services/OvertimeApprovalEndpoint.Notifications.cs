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
using ApiContent = Acme.Content.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

public sealed class OvertimeApprovalEndpointSubmitOvertime1NotificationHandler(ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient client) : INotificationHandler
{
    public string Topic => "acme.timekeeping.v1.OvertimeApprovalService/SubmitOvertime/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiTimekeeping.SubmitClockEventRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SubmitClockEventAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
