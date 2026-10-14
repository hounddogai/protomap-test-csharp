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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPatients = Acme.Patients.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Timekeeping;

public sealed class ClockEventApprovalEndpointDelegateClockEventApproval0NotificationHandler(ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.timekeeping.v1.ClockEventApprovalService/DelegateClockEventApproval/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiEmployees.BrowseEmployeesRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.BrowseEmployeesAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
