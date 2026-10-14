using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTelehealth = Acme.Telehealth.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where RouteIncidentRulesStatus hands each record.</summary>
internal interface IRouteIncidentRulesStatusWriter
{
    Task ApplyAsync(RouteIncidentRulesStatusRecord record, CancellationToken token);
}

/// <summary>What RouteIncidentRulesStatus keeps of each GetIncidentRules response.</summary>
internal sealed class RouteIncidentRulesStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string IncidentId { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Sends records to acme.notifications.v1.NotificationValidationService.</summary>
internal sealed class RouteIncidentRulesStatusNotificationValidationTarget : IRouteIncidentRulesStatusWriter
{
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _client;

    public RouteIncidentRulesStatusNotificationValidationTarget(ApiNotifications.NotificationValidationService.NotificationValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(RouteIncidentRulesStatusRecord record, CancellationToken token)
    {
        await _client.CheckNotificationEligibilityAsync(new ApiNotifications.CheckNotificationEligibilityRequest { Reason = record.Subject, Notification = new ApiNotifications.Notification { Subject = record.CorrelationId, NotificationId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.fuel.v1.FuelCardService.</summary>
internal sealed class RouteIncidentRulesStatusFuelCardTarget(ApiFuel.FuelCardService.FuelCardServiceClient client) : IRouteIncidentRulesStatusWriter
{
    public async Task ApplyAsync(RouteIncidentRulesStatusRecord record, CancellationToken token)
    {
        await client.ListFuelCardsAsync(new ApiFuel.ListFuelCardsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiFuel.ListFuelCardsRequest.Types.Filter { Query = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.telehealth.v1.VideoVisitCalculationService.</summary>
internal sealed class RouteIncidentRulesStatusVideoVisitCalculationWriter : IRouteIncidentRulesStatusWriter
{
    private readonly ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient _client;

    public RouteIncidentRulesStatusVideoVisitCalculationWriter(ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(RouteIncidentRulesStatusRecord record, CancellationToken token)
    {
        await _client.GetVideoVisitRatesAsync(new ApiTelehealth.GetVideoVisitRatesRequest { VideoVisitId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.IncidentValidationService/GetIncidentRules and hands what it keeps to the sink of its kind.</summary>
internal static class RouteIncidentRulesStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.IncidentValidationService.IncidentValidationServiceClient(channel);
        var response = await source.GetIncidentRulesAsync(new ApiItsm.GetIncidentRulesRequest() { IncidentId = "primary-route-incident-rules-status" }, cancellationToken: token);
        var record = new RouteIncidentRulesStatusRecord
        {
            Status = Text.Normalize(response.Result.Status),
            CorrelationId = Text.Normalize(response.Result.CorrelationId),
            IncidentId = response.Incident.IncidentId,
            Subject = response.Incident.Subject,
        };
        var sinks = new Dictionary<string, IRouteIncidentRulesStatusWriter>
        {
            ["notification-validation"] = new RouteIncidentRulesStatusNotificationValidationTarget(new ApiNotifications.NotificationValidationService.NotificationValidationServiceClient(channel)),
            ["fuel-card"] = new RouteIncidentRulesStatusFuelCardTarget(new ApiFuel.FuelCardService.FuelCardServiceClient(channel)),
            ["video-visit-calculation"] = new RouteIncidentRulesStatusVideoVisitCalculationWriter(new ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "fuel-card")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
