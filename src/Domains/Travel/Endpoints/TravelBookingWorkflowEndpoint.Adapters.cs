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
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for CreateNotification.
/// </summary>
public interface ITravelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port
{
    Task<ApiNotifications.CreateNotificationResponse> SendAsync(ApiNotifications.CreateNotificationRequest request, CancellationToken token);
}

public sealed class TravelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2PortAdapter(ApiNotifications.NotificationService.NotificationServiceClient client) : ITravelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port
{
    public async Task<ApiNotifications.CreateNotificationResponse> SendAsync(ApiNotifications.CreateNotificationRequest request, CancellationToken token)
    {
        using var rpc = client.CreateNotificationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
