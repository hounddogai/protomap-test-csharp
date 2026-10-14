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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public sealed class GoalQueryEndpointSummarizeGoals0NotificationHandler(ApiPerformance.ReviewQueryService.ReviewQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.performance.v1.GoalQueryService/SummarizeGoals/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiPerformance.FindReviewsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.FindReviewsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
