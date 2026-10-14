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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public sealed class InterviewWorkflowEndpointPauseInterviewWorkflow1NotificationHandler(ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient client) : INotificationHandler
{
    public string Topic => "acme.recruiting.v1.InterviewWorkflowService/PauseInterviewWorkflow/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiWorkplaces.StartRoomBookingWorkflowRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.StartRoomBookingWorkflowAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
