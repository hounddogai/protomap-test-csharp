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

/// <summary>
/// The application port for PeekManagedDevice.
/// </summary>
public interface IManagedDeviceApprovalEndpointSendRejectManagedDevice2Port
{
    Task<ApiDevices.PeekManagedDeviceResponse> SendAsync(ApiDevices.PeekManagedDeviceRequest request, CancellationToken token);
}

public sealed class ManagedDeviceApprovalEndpointSendRejectManagedDevice2PortAdapter(ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient client) : IManagedDeviceApprovalEndpointSendRejectManagedDevice2Port
{
    public async Task<ApiDevices.PeekManagedDeviceResponse> SendAsync(ApiDevices.PeekManagedDeviceRequest request, CancellationToken token)
    {
        using var rpc = client.PeekManagedDeviceAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
