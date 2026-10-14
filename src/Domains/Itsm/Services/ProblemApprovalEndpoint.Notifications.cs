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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Itsm;

public sealed class ProblemApprovalEndpointApproveProblem3NotificationHandler(ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient client) : INotificationHandler
{
    public string Topic => "acme.itsm.v1.ProblemApprovalService/ApproveProblem/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiItsm.ReinstateProblemRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ReinstateProblemAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
