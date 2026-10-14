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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Timekeeping;

public sealed class TimesheetWorkflowEndpointResumeTimesheetWorkflow0NotificationHandler(ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient client) : INotificationHandler
{
    public string Topic => "acme.timekeeping.v1.TimesheetWorkflowService/ResumeTimesheetWorkflow/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiAuthorization.BatchGetAccessPoliciesRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.BatchGetAccessPoliciesAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
