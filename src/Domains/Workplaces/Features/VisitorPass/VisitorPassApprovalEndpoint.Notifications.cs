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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed class VisitorPassApprovalEndpointDelegateVisitorPassApproval1NotificationHandler(ApiIdentity.UserService.UserServiceClient client) : INotificationHandler
{
    public string Topic => "acme.workplaces.v1.VisitorPassApprovalService/DelegateVisitorPassApproval/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiIdentity.GetUserRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.GetUserAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
