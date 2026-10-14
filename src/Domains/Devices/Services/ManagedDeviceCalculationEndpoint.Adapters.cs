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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The application port for ValidateSoftwareLicense.
/// </summary>
public interface IManagedDeviceCalculationEndpointSendSimulateManagedDevice0Port
{
    Task<ApiLicensing.ValidateSoftwareLicenseResponse> SendAsync(ApiLicensing.ValidateSoftwareLicenseRequest request, CancellationToken token);
}

public sealed class ManagedDeviceCalculationEndpointSendSimulateManagedDevice0PortAdapter(ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient client) : IManagedDeviceCalculationEndpointSendSimulateManagedDevice0Port
{
    public async Task<ApiLicensing.ValidateSoftwareLicenseResponse> SendAsync(ApiLicensing.ValidateSoftwareLicenseRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateSoftwareLicenseAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
