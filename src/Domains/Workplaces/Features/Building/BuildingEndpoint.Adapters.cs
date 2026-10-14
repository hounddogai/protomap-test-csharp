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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for CompareEscalationOptions.
/// </summary>
public interface IBuildingEndpointSendWatchBuildings0Port
{
    Task<ApiHelpdesk.CompareEscalationOptionsResponse> SendAsync(ApiHelpdesk.CompareEscalationOptionsRequest request, CancellationToken token);
}

public sealed class BuildingEndpointSendWatchBuildings0PortAdapter(ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient client) : IBuildingEndpointSendWatchBuildings0Port
{
    public async Task<ApiHelpdesk.CompareEscalationOptionsResponse> SendAsync(ApiHelpdesk.CompareEscalationOptionsRequest request, CancellationToken token)
    {
        using var rpc = client.CompareEscalationOptionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class BuildingEndpointRequests
{
    /// <summary>
    /// Builds the CountBuildings request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.CountBuildingsRequest UpdateBuildingToCountBuildings1(this ApiWorkplaces.UpdateBuildingRequest request, ApiWorkplaces.EstimateRoomBookingResponse reply0)
    {
        return new ApiWorkplaces.CountBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.CountBuildingsRequest.Types.Filter
            {
                Region = (reply0.RoomBooking?.Location?.Region ?? ""),
            },
        };
    }
}
