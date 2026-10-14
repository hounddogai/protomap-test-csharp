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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The application port for BrowseDevicePolicies.
/// </summary>
public interface IDevicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port
{
    Task<ApiDevices.BrowseDevicePoliciesResponse> SendAsync(ApiDevices.BrowseDevicePoliciesRequest request, CancellationToken token);
}

public sealed class DevicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0PortAdapter(ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient client) : IDevicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port
{
    public async Task<ApiDevices.BrowseDevicePoliciesResponse> SendAsync(ApiDevices.BrowseDevicePoliciesRequest request, CancellationToken token)
    {
        using var rpc = client.BrowseDevicePoliciesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class DevicePolicyLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the PeekDevicePolicy request from the current workflow state.
    /// </summary>
    public static ApiDevices.PeekDevicePolicyRequest RenewDevicePolicyToPeekDevicePolicy1(this ApiDevices.RenewDevicePolicyRequest request)
    {
        return new ApiDevices.PeekDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicyId = (request.DevicePolicyId ?? ""),
        };
    }
}
