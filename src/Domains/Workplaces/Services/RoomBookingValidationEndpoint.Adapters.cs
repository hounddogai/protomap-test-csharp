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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for CheckDeskEligibility.
/// </summary>
public interface IRoomBookingValidationEndpointSendCheckRoomBookingEligibility2Port
{
    Task<ApiWorkplaces.CheckDeskEligibilityResponse> SendAsync(ApiWorkplaces.CheckDeskEligibilityRequest request, CancellationToken token);
}

public sealed class RoomBookingValidationEndpointSendCheckRoomBookingEligibility2PortAdapter(ApiWorkplaces.DeskValidationService.DeskValidationServiceClient client) : IRoomBookingValidationEndpointSendCheckRoomBookingEligibility2Port
{
    public async Task<ApiWorkplaces.CheckDeskEligibilityResponse> SendAsync(ApiWorkplaces.CheckDeskEligibilityRequest request, CancellationToken token)
    {
        using var rpc = client.CheckDeskEligibilityAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

/// <summary>
/// The application port for LintDesks.
/// </summary>
public interface IRoomBookingValidationEndpointSendValidateRoomBooking2Port
{
    Task<ApiWorkplaces.LintDesksResponse> SendAsync(ApiWorkplaces.LintDesksRequest request, CancellationToken token);
}

public sealed class RoomBookingValidationEndpointSendValidateRoomBooking2PortAdapter(ApiWorkplaces.DeskValidationService.DeskValidationServiceClient client) : IRoomBookingValidationEndpointSendValidateRoomBooking2Port
{
    public async Task<ApiWorkplaces.LintDesksResponse> SendAsync(ApiWorkplaces.LintDesksRequest request, CancellationToken token)
    {
        using var rpc = client.LintDesksAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RoomBookingValidationEndpointRequests
{
    /// <summary>
    /// Builds the EstimateVisitorPass request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.EstimateVisitorPassRequest ValidateRoomBookingToEstimateVisitorPass1(this ApiWorkplaces.ValidateRoomBookingRequest request, ApiWorkplaces.ValidateBuildingResponse reply0)
    {
        return new ApiWorkplaces.EstimateVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.EstimateVisitorPassRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Building?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply0.Building?.Location?.PostalCode ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Building?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.RoomBooking?.Audit?.CreatedBy ?? ""),
                },
            },
        };
    }
}
