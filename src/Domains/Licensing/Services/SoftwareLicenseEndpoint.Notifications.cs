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
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Licensing;

public sealed class SoftwareLicenseEndpointWatchSoftwareLicenses0NotificationHandler(ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.licensing.v1.SoftwareLicenseService/WatchSoftwareLicenses/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.PreviewSoftwareLicenseRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.PreviewSoftwareLicenseAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
