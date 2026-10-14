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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

public sealed class ComplianceScanLifecycleEndpointSuspendComplianceScan3NotificationHandler(ApiRecruiting.JobOpeningService.JobOpeningServiceClient client) : INotificationHandler
{
    public string Topic => "acme.devices.v1.ComplianceScanLifecycleService/SuspendComplianceScan/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.SearchJobOpeningsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SearchJobOpeningsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
