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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

public sealed class SwapRequestCalculationEndpointQuoteSwapRequest0NotificationHandler(ApiScheduling.RosterCalculationService.RosterCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.scheduling.v1.SwapRequestCalculationService/QuoteSwapRequest/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiScheduling.SimulateRosterRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SimulateRosterAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
