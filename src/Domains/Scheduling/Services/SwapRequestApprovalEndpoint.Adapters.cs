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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for SignalShiftWorkflow.
/// </summary>
public interface ISwapRequestApprovalEndpointSendApproveSwapRequest0Port
{
    Task<ApiScheduling.SignalShiftWorkflowResponse> SendAsync(ApiScheduling.SignalShiftWorkflowRequest request, CancellationToken token);
}

public sealed class SwapRequestApprovalEndpointSendApproveSwapRequest0PortAdapter(ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient client) : ISwapRequestApprovalEndpointSendApproveSwapRequest0Port
{
    public async Task<ApiScheduling.SignalShiftWorkflowResponse> SendAsync(ApiScheduling.SignalShiftWorkflowRequest request, CancellationToken token)
    {
        using var rpc = client.SignalShiftWorkflowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
