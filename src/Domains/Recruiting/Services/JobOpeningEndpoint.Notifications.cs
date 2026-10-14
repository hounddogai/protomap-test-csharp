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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWebhooks = Acme.Webhooks.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public sealed class JobOpeningEndpointWatchJobOpenings2NotificationHandler(ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.recruiting.v1.JobOpeningService/WatchJobOpenings/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.SuggestCandidatesRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SuggestCandidatesAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
