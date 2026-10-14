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
using ApiAppointments = Acme.Appointments.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

public sealed class ClockEventValidationEndpointNormalizeClockEvent3NotificationHandler(ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.timekeeping.v1.ClockEventValidationService/NormalizeClockEvent/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiTimekeeping.ValidateTimesheetRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ValidateTimesheetAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
