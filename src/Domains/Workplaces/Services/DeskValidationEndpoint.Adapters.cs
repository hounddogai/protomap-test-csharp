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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for ScoreTravelBooking.
/// </summary>
public interface IDeskValidationEndpointSendCheckDeskEligibility3Port
{
    Task<ApiTravel.ScoreTravelBookingResponse> SendAsync(ApiTravel.ScoreTravelBookingRequest request, CancellationToken token);
}

public sealed class DeskValidationEndpointSendCheckDeskEligibility3PortAdapter(ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient client) : IDeskValidationEndpointSendCheckDeskEligibility3Port
{
    public async Task<ApiTravel.ScoreTravelBookingResponse> SendAsync(ApiTravel.ScoreTravelBookingRequest request, CancellationToken token)
    {
        using var rpc = client.ScoreTravelBookingAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class DeskValidationEndpointRequests
{
    /// <summary>
    /// Builds the EstimateDesk request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.EstimateDeskRequest ValidateDeskToEstimateDesk3(this ApiWorkplaces.ValidateDeskRequest request, ApiDocuments.PushDocumentFolderResponse reply0)
    {
        return new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Desk?.Owner?.UserId ?? ""),
                },
            },
            Details = new ApiWorkplaces.EstimateDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.DocumentFolder?.Location?.Recipient ?? ""),
                },
            },
        };
    }
}
