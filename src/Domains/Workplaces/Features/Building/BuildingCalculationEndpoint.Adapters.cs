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
using ApiAudiences = Acme.Audiences.V1;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for LintItineraries.
/// </summary>
public interface IBuildingCalculationEndpointSendSimulateBuilding1Port
{
    Task<ApiTravel.LintItinerariesResponse> SendAsync(ApiTravel.LintItinerariesRequest request, CancellationToken token);
}

public sealed class BuildingCalculationEndpointSendSimulateBuilding1PortAdapter(ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient client) : IBuildingCalculationEndpointSendSimulateBuilding1Port
{
    public async Task<ApiTravel.LintItinerariesResponse> SendAsync(ApiTravel.LintItinerariesRequest request, CancellationToken token)
    {
        using var rpc = client.LintItinerariesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class BuildingCalculationEndpointRequests
{
    /// <summary>
    /// Builds the SuggestAudiences request from the current workflow state.
    /// </summary>
    public static ApiAudiences.SuggestAudiencesRequest SimulateBuildingToSuggestAudiences0(this ApiWorkplaces.SimulateBuildingRequest request)
    {
        return new ApiAudiences.SuggestAudiencesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudiences.SuggestAudiencesRequest.Types.Filter
            {
                CreatedAfter = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Building?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Building?.Manager?.UserId ?? ""),
                Region = (request.Building?.Location?.Region ?? ""),
            },
        };
    }
}
