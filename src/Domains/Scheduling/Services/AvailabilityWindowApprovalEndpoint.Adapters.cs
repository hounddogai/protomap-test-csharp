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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for NormalizePosition.
/// </summary>
public interface IAvailabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port
{
    Task<ApiEmployees.NormalizePositionResponse> SendAsync(ApiEmployees.NormalizePositionRequest request, CancellationToken token);
}

public sealed class AvailabilityWindowApprovalEndpointSendRecallAvailabilityWindow1PortAdapter(ApiEmployees.PositionValidationService.PositionValidationServiceClient client) : IAvailabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port
{
    public async Task<ApiEmployees.NormalizePositionResponse> SendAsync(ApiEmployees.NormalizePositionRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizePositionAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class AvailabilityWindowApprovalEndpointRequests
{
    /// <summary>
    /// Builds the TerminateAvailabilityWindow request from the current workflow state.
    /// </summary>
    public static ApiScheduling.TerminateAvailabilityWindowRequest RecallAvailabilityWindowToTerminateAvailabilityWindow0(this ApiScheduling.RecallAvailabilityWindowRequest request)
    {
        return new ApiScheduling.TerminateAvailabilityWindowRequest
        {
            AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.TerminateAvailabilityWindowRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
    }
}
