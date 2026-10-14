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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed class DeskCalculationEndpointExplainDeskCalculation2NotificationHandler(ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.workplaces.v1.DeskCalculationService/ExplainDeskCalculation/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiWorkplaces.SimulateBuildingRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SimulateBuildingAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
