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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Helpdesk;

public sealed class SlaPolicyApprovalEndpointCommentSlaPolicyApproval2NotificationHandler(ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient client) : INotificationHandler
{
    public string Topic => "acme.helpdesk.v1.SlaPolicyApprovalService/CommentSlaPolicyApproval/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiHelpdesk.ScheduleEscalationReportRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ScheduleEscalationReportAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
