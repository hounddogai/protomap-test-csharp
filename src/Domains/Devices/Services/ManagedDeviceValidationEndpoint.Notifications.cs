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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Devices;

public sealed class ManagedDeviceValidationEndpointCheckManagedDeviceEligibility1NotificationHandler(ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.devices.v1.ManagedDeviceValidationService/CheckManagedDeviceEligibility/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.ExplainEntitlementCalculationRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ExplainEntitlementCalculationAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
