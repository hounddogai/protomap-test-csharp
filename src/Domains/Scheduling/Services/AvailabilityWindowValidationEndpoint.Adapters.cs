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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for ExplainRosterDecision.
/// </summary>
public interface IAvailabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port
{
    Task<ApiScheduling.ExplainRosterDecisionResponse> SendAsync(ApiScheduling.ExplainRosterDecisionRequest request, CancellationToken token);
}

public sealed class AvailabilityWindowValidationEndpointSendPreviewAvailabilityWindow1PortAdapter(ApiScheduling.RosterValidationService.RosterValidationServiceClient client) : IAvailabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port
{
    public async Task<ApiScheduling.ExplainRosterDecisionResponse> SendAsync(ApiScheduling.ExplainRosterDecisionRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainRosterDecisionAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class AvailabilityWindowValidationEndpointRequests
{
    /// <summary>
    /// Builds the GetAvailabilityWindowRates request from the current workflow state.
    /// </summary>
    public static ApiScheduling.GetAvailabilityWindowRatesRequest PreviewAvailabilityWindowToGetAvailabilityWindowRates0(this ApiScheduling.PreviewAvailabilityWindowRequest request)
    {
        return new ApiScheduling.GetAvailabilityWindowRatesRequest
        {
            AvailabilityWindowId = (request.AvailabilityWindow?.AvailabilityWindowId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
