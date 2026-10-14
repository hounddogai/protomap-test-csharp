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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Devices;

public sealed class DevicePolicyCalculationEndpointCompareDevicePolicyOptions0NotificationHandler(ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.devices.v1.DevicePolicyCalculationService/CompareDevicePolicyOptions/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDevices.SimulateManagedDeviceRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SimulateManagedDeviceAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
