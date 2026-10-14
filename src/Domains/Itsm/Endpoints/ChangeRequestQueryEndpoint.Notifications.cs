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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Itsm;

public sealed class ChangeRequestQueryEndpointExportChangeRequests3NotificationHandler(ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.itsm.v1.ChangeRequestQueryService/ExportChangeRequests/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.ResolveSoftwareLicensesRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ResolveSoftwareLicensesAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
