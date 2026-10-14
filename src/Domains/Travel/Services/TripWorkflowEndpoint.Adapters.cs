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
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for CountItineraryTotals.
/// </summary>
public interface ITripWorkflowEndpointSendResumeTripWorkflow1Port
{
    Task<ApiTravel.CountItineraryTotalsResponse> SendAsync(ApiTravel.CountItineraryTotalsRequest request, CancellationToken token);
}

public sealed class TripWorkflowEndpointSendResumeTripWorkflow1PortAdapter(ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient client) : ITripWorkflowEndpointSendResumeTripWorkflow1Port
{
    public async Task<ApiTravel.CountItineraryTotalsResponse> SendAsync(ApiTravel.CountItineraryTotalsRequest request, CancellationToken token)
    {
        using var rpc = client.CountItineraryTotalsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TripWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the ScoreNotification request from the current workflow state.
    /// </summary>
    public static ApiNotifications.ScoreNotificationRequest ResumeTripWorkflowToScoreNotification0(this ApiTravel.ResumeTripWorkflowRequest request)
    {
        return new ApiNotifications.ScoreNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.ScoreNotificationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Trip?.BillingAddress?.Region ?? ""),
                },
            },
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
    }
}
