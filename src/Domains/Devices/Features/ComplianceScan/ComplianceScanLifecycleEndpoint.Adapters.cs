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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// The application port for CreateManagedDevice.
/// </summary>
public interface IComplianceScanLifecycleEndpointSendTerminateComplianceScan0Port
{
    Task<ApiDevices.CreateManagedDeviceResponse> SendAsync(ApiDevices.CreateManagedDeviceRequest request, CancellationToken token);
}

public sealed class ComplianceScanLifecycleEndpointSendTerminateComplianceScan0PortAdapter(ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient client) : IComplianceScanLifecycleEndpointSendTerminateComplianceScan0Port
{
    public async Task<ApiDevices.CreateManagedDeviceResponse> SendAsync(ApiDevices.CreateManagedDeviceRequest request, CancellationToken token)
    {
        using var rpc = client.CreateManagedDeviceAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ComplianceScanLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the SearchJobOpenings request from the current workflow state.
    /// </summary>
    public static ApiRecruiting.SearchJobOpeningsRequest SuspendComplianceScanToSearchJobOpenings3(this ApiDevices.SuspendComplianceScanRequest request, ApiRecruiting.CountCandidatesResponse reply0, ApiRecruiting.RecalculateCandidatesResponse reply2)
    {
        return new ApiRecruiting.SearchJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SearchJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (reply2.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (reply2.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
    }
}
