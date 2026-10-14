using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>What HandOffLifeEventId keeps of each QuoteLifeEvent response.</summary>
internal sealed class HandOffLifeEventIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string LifeEventCorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.notifications.v1.NotificationValidationService.</summary>
internal sealed class HandOffLifeEventIdNotificationValidationWriter
{
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _client;

    public HandOffLifeEventIdNotificationValidationWriter(ApiNotifications.NotificationValidationService.NotificationValidationServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffLifeEventIdRecord record, CancellationToken token)
    {
        await _client.NormalizeNotificationAsync(new ApiNotifications.NormalizeNotificationRequest { Details = new ApiNotifications.NormalizeNotificationRequest.Types.Details { Locale = record.LifeEventCorrelationId }, NotificationId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.benefits.v1.LifeEventCalculationService/QuoteLifeEvent and hands what it keeps to acme.notifications.v1.NotificationValidationService.</summary>
internal static class HandOffLifeEventId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient(channel);
        var got = await source.QuoteLifeEventAsync(new ApiBenefits.QuoteLifeEventRequest() { LifeEventId = "nightly-hand-off-life-event-id" }, cancellationToken: token);
        var record = new HandOffLifeEventIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            LifeEventCorrelationId = got.LifeEvent.CorrelationId,
        };
        await new HandOffLifeEventIdNotificationValidationWriter(new ApiNotifications.NotificationValidationService.NotificationValidationServiceClient(channel)).WriteAsync(record, token);
    }
}
