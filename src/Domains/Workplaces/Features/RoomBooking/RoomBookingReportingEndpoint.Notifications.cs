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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed class RoomBookingReportingEndpointStreamRoomBookingMetrics0NotificationHandler(ApiCustomers.CustomerService.CustomerServiceClient client) : INotificationHandler
{
    public string Topic => "acme.workplaces.v1.RoomBookingReportingService/StreamRoomBookingMetrics/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiCustomers.SearchCustomersRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SearchCustomersAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
