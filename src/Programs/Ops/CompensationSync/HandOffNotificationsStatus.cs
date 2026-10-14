using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where HandOffNotificationsStatus hands each record.</summary>
internal interface IHandOffNotificationsStatusSink
{
    Task DeliverAsync(HandOffNotificationsStatusRecord record, CancellationToken token);
}

/// <summary>What HandOffNotificationsStatus keeps of each ResolveNotifications response.</summary>
internal sealed class HandOffNotificationsStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Writes records to acme.webhooks.v1.WebhookEndpointQueryService.</summary>
internal sealed class HandOffNotificationsStatusWebhookEndpointQuerySink : IHandOffNotificationsStatusSink
{
    private readonly ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient _client;

    public HandOffNotificationsStatusWebhookEndpointQuerySink(ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffNotificationsStatusRecord record, CancellationToken token)
    {
        await _client.FindWebhookEndpointsAsync(new ApiWebhooks.FindWebhookEndpointsRequest { Filter = new ApiWebhooks.FindWebhookEndpointsRequest.Types.Filter { OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.TripQueryService.</summary>
internal sealed class HandOffNotificationsStatusTripQueryTarget : IHandOffNotificationsStatusSink
{
    private readonly ApiTravel.TripQueryService.TripQueryServiceClient _client;

    public HandOffNotificationsStatusTripQueryTarget(ApiTravel.TripQueryService.TripQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffNotificationsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.SuggestTripsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status } }, token, _client.SuggestTripsAsync);
    }
}

/// <summary>Passes records to acme.performance.v1.ReviewWorkflowService.</summary>
internal sealed class HandOffNotificationsStatusReviewWorkflowSink(ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient client) : IHandOffNotificationsStatusSink
{
    public async Task DeliverAsync(HandOffNotificationsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPerformance.AdvanceReviewWorkflowRequest { Review = new ApiPerformance.Review { ReviewId = record.NextPageToken } }, token, client.AdvanceReviewWorkflowAsync);
    }
}

/// <summary>Reads acme.notifications.v1.NotificationQueryService/ResolveNotifications and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffNotificationsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiNotifications.NotificationQueryService.NotificationQueryServiceClient(channel);
        var found = await source.ResolveNotificationsAsync(new ApiNotifications.ResolveNotificationsRequest(), cancellationToken: token);
        var record = new HandOffNotificationsStatusRecord
        {
            Status = found.Result.Status,
            NextPageToken = found.PageInfo.NextPageToken,
        };
        var sinks = new List<IHandOffNotificationsStatusSink>
        {
            new HandOffNotificationsStatusWebhookEndpointQuerySink(new ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient(channel)),
            new HandOffNotificationsStatusTripQueryTarget(new ApiTravel.TripQueryService.TripQueryServiceClient(channel)),
            new HandOffNotificationsStatusReviewWorkflowSink(new ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient(channel)),
        };
        await sinks[1].DeliverAsync(record, token);
    }
}
