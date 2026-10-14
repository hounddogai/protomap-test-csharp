using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ReplayVisitorPassEligibilityNumber hands each record.</summary>
internal interface IReplayVisitorPassEligibilityNumberDestination
{
    Task ForwardAsync(ReplayVisitorPassEligibilityNumberRecord record, CancellationToken token);
}

/// <summary>What ReplayVisitorPassEligibilityNumber keeps of each CheckVisitorPassEligibility response.</summary>
internal sealed class ReplayVisitorPassEligibilityNumberRecord
{
    public string Number { get; init; } = "";
    public string Status { get; init; } = "";
    public string Notes { get; init; } = "";
}

/// <summary>Hands records to acme.audiences.v1.AudienceService.</summary>
internal sealed class ReplayVisitorPassEligibilityNumberAudienceWriter : IReplayVisitorPassEligibilityNumberDestination
{
    private readonly ApiAudiences.AudienceService.AudienceServiceClient _client;

    public ReplayVisitorPassEligibilityNumberAudienceWriter(ApiAudiences.AudienceService.AudienceServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayVisitorPassEligibilityNumberRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAudiences.ListAudiencesRequest { Page = new ApiCommon.PageRequest { PageToken = record.Notes }, Filter = new ApiAudiences.ListAudiencesRequest.Types.Filter { Region = record.Number, OwnerId = record.Status } }, token, _client.ListAudiencesAsync);
    }
}

/// <summary>Passes records to acme.configuration.v1.ConfigSetService.</summary>
internal sealed class ReplayVisitorPassEligibilityNumberConfigSetSink : IReplayVisitorPassEligibilityNumberDestination
{
    private readonly ApiConfiguration.ConfigSetService.ConfigSetServiceClient _client;

    public ReplayVisitorPassEligibilityNumberConfigSetSink(ApiConfiguration.ConfigSetService.ConfigSetServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayVisitorPassEligibilityNumberRecord record, CancellationToken token)
    {
        await _client.CloneConfigSetAsync(new ApiConfiguration.CloneConfigSetRequest { ConfigSet = new ApiConfiguration.ConfigSet { ConfigSetId = record.Status, ParentId = record.Notes }, RequestId = record.Number }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.notifications.v1.NotificationValidationService.</summary>
internal sealed class ReplayVisitorPassEligibilityNumberNotificationValidationRelay : IReplayVisitorPassEligibilityNumberDestination
{
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _client;

    public ReplayVisitorPassEligibilityNumberNotificationValidationRelay(ApiNotifications.NotificationValidationService.NotificationValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayVisitorPassEligibilityNumberRecord record, CancellationToken token)
    {
        await _client.PreviewNotificationAsync(new ApiNotifications.PreviewNotificationRequest { Notification = new ApiNotifications.Notification { NotificationId = record.Status }, Details = new ApiNotifications.PreviewNotificationRequest.Types.Details { Locale = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.VisitorPassValidationService/CheckVisitorPassEligibility and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayVisitorPassEligibilityNumber
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient(channel);
        var reply = await source.CheckVisitorPassEligibilityAsync(new ApiWorkplaces.CheckVisitorPassEligibilityRequest() { Reason = "default-replay-visitor-pass-eligibility-number" }, cancellationToken: token);
        var record = new ReplayVisitorPassEligibilityNumberRecord
        {
            Number = Text.Normalize(reply.VisitorPass.Number),
            Status = reply.Result.Status,
            Notes = Text.Normalize(reply.VisitorPass.Notes),
        };
        var sinks = new Dictionary<string, IReplayVisitorPassEligibilityNumberDestination>
        {
            ["audience"] = new ReplayVisitorPassEligibilityNumberAudienceWriter(new ApiAudiences.AudienceService.AudienceServiceClient(channel)),
            ["config-set"] = new ReplayVisitorPassEligibilityNumberConfigSetSink(new ApiConfiguration.ConfigSetService.ConfigSetServiceClient(channel)),
            ["notification-validation"] = new ReplayVisitorPassEligibilityNumberNotificationValidationRelay(new ApiNotifications.NotificationValidationService.NotificationValidationServiceClient(channel)),
        };
        await RouteReplayVisitorPassEligibilityNumberAsync("notification-validation", record, sinks, token);
    }

    private static async Task RouteReplayVisitorPassEligibilityNumberAsync(string kind, ReplayVisitorPassEligibilityNumberRecord record, IReadOnlyDictionary<string, IReplayVisitorPassEligibilityNumberDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
