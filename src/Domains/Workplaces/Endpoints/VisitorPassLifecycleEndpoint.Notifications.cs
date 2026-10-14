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
using ApiBanking = Acme.Banking.V1;
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSearch = Acme.Search.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed class VisitorPassLifecycleEndpointSuspendVisitorPass1NotificationHandler(ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.workplaces.v1.VisitorPassLifecycleService/SuspendVisitorPass/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiChat.CalculateChatRoomRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CalculateChatRoomAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
