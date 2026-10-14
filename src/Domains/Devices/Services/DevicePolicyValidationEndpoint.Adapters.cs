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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The application port for NormalizeEntitlement.
/// </summary>
public interface IDevicePolicyValidationEndpointSendValidateDevicePolicy0Port
{
    Task<ApiLicensing.NormalizeEntitlementResponse> SendAsync(ApiLicensing.NormalizeEntitlementRequest request, CancellationToken token);
}

public sealed class DevicePolicyValidationEndpointSendValidateDevicePolicy0PortAdapter(ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient client) : IDevicePolicyValidationEndpointSendValidateDevicePolicy0Port
{
    public async Task<ApiLicensing.NormalizeEntitlementResponse> SendAsync(ApiLicensing.NormalizeEntitlementRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizeEntitlementAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
