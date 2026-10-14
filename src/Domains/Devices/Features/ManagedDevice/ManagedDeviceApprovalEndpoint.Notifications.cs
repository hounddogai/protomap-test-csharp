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
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Devices;

public sealed class ManagedDeviceApprovalEndpointRejectManagedDevice2NotificationHandler(ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.devices.v1.ManagedDeviceApprovalService/RejectManagedDevice/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDevices.PeekManagedDeviceRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.PeekManagedDeviceAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
