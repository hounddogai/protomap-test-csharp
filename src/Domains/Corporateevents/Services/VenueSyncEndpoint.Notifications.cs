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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

public sealed class VenueSyncEndpointPushVenue3NotificationHandler(ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.corporateevents.v1.VenueSyncService/PushVenue/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiFeatureflags.SuggestFeatureFlagsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SuggestFeatureFlagsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
