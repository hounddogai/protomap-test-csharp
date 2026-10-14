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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed class BuildingValidationEndpointScoreBuilding0NotificationHandler(ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.workplaces.v1.BuildingValidationService/ScoreBuilding/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiTravel.ExplainTravelPolicyDecisionRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ExplainTravelPolicyDecisionAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
