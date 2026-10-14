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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed class DeskValidationEndpointLintDesks0NotificationHandler(ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.workplaces.v1.DeskValidationService/LintDesks/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiWorkplaces.RecalculateRoomBookingsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.RecalculateRoomBookingsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
