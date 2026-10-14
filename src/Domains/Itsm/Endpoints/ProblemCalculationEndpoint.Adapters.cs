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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for EstimateIncident.
/// </summary>
public interface IProblemCalculationEndpointSendEstimateProblem1Port
{
    Task<ApiItsm.EstimateIncidentResponse> SendAsync(ApiItsm.EstimateIncidentRequest request, CancellationToken token);
}

public sealed class ProblemCalculationEndpointSendEstimateProblem1PortAdapter(ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient client) : IProblemCalculationEndpointSendEstimateProblem1Port
{
    public async Task<ApiItsm.EstimateIncidentResponse> SendAsync(ApiItsm.EstimateIncidentRequest request, CancellationToken token)
    {
        using var rpc = client.EstimateIncidentAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ProblemCalculationEndpointRequests
{
    /// <summary>
    /// Builds the CalculateManagedDevice request from the current workflow state.
    /// </summary>
    public static ApiDevices.CalculateManagedDeviceRequest EstimateProblemToCalculateManagedDevice0(this ApiItsm.EstimateProblemRequest request)
    {
        return new ApiDevices.CalculateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
    }
}
