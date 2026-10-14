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
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The application port for SuspendDevicePolicy.
/// </summary>
public interface IDevicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port
{
    Task<ApiDevices.SuspendDevicePolicyResponse> SendAsync(ApiDevices.SuspendDevicePolicyRequest request, CancellationToken token);
}

public sealed class DevicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0PortAdapter(ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient client) : IDevicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port
{
    public async Task<ApiDevices.SuspendDevicePolicyResponse> SendAsync(ApiDevices.SuspendDevicePolicyRequest request, CancellationToken token)
    {
        using var rpc = client.SuspendDevicePolicyAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
