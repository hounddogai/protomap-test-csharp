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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Licensing;

public sealed class SoftwareLicenseApprovalEndpointSubmitSoftwareLicense0NotificationHandler(ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.licensing.v1.SoftwareLicenseApprovalService/SubmitSoftwareLicense/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiTelematics.BrowseTelemetryReadingsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.BrowseTelemetryReadingsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
