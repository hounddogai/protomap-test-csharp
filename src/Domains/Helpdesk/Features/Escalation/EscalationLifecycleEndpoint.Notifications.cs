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
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Helpdesk;

public sealed class EscalationLifecycleEndpointRenewEscalation2NotificationHandler(ApiEmployees.EmployeeService.EmployeeServiceClient client) : INotificationHandler
{
    public string Topic => "acme.helpdesk.v1.EscalationLifecycleService/RenewEscalation/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiEmployees.CreateEmployeeRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CreateEmployeeAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
