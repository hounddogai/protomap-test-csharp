using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSponsorships = Acme.Sponsorships.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where ForwardSponsorId hands each record.</summary>
internal interface IForwardSponsorIdDestination
{
    Task SendAsync(ForwardSponsorIdRecord record, CancellationToken token);
}

/// <summary>What ForwardSponsorId keeps of each SimulateSponsor response.</summary>
internal sealed class ForwardSponsorIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.licensing.v1.SoftwareLicenseQueryService.</summary>
internal sealed class ForwardSponsorIdSoftwareLicenseQueryTarget(ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient client) : IForwardSponsorIdDestination
{
    public async Task SendAsync(ForwardSponsorIdRecord record, CancellationToken token)
    {
        await client.SuggestSoftwareLicensesAsync(new ApiLicensing.SuggestSoftwareLicensesRequest { OrderBy = record.CorrelationId, Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiLicensing.SuggestSoftwareLicensesRequest.Types.Filter { Query = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.employeeportal.v1.AnnouncementNotificationService.</summary>
internal sealed class ForwardSponsorIdAnnouncementNotificationRelay(ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient client) : IForwardSponsorIdDestination
{
    public async Task SendAsync(ForwardSponsorIdRecord record, CancellationToken token)
    {
        await client.UnsubscribeAnnouncementUpdatesAsync(new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesRequest { Reason = record.CurrencyCode, Details = new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesRequest.Types.Details { TemplateId = record.CorrelationId }, AnnouncementId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.notifications.v1.NotificationService.</summary>
internal sealed class ForwardSponsorIdNotificationTarget : IForwardSponsorIdDestination
{
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _client;

    public ForwardSponsorIdNotificationTarget(ApiNotifications.NotificationService.NotificationServiceClient client) => _client = client;

    public async Task SendAsync(ForwardSponsorIdRecord record, CancellationToken token)
    {
        await _client.ListNotificationsAsync(new ApiNotifications.ListNotificationsRequest { Filter = new ApiNotifications.ListNotificationsRequest.Types.Filter { OwnerId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.sponsorships.v1.SponsorCalculationService/SimulateSponsor and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardSponsorId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient(channel);
        var got = await source.SimulateSponsorAsync(new ApiSponsorships.SimulateSponsorRequest() { SponsorId = "nightly-forward-sponsor-id" }, cancellationToken: token);
        var record = new ForwardSponsorIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            CurrencyCode = Text.Normalize(got.Breakdown.CurrencyCode),
            Message = Text.Normalize(got.Result.Message),
            Status = got.Result.Status,
        };
        var sinks = new Dictionary<string, IForwardSponsorIdDestination>
        {
            ["software-license-query"] = new ForwardSponsorIdSoftwareLicenseQueryTarget(new ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient(channel)),
            ["announcement-notification"] = new ForwardSponsorIdAnnouncementNotificationRelay(new ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient(channel)),
            ["notification"] = new ForwardSponsorIdNotificationTarget(new ApiNotifications.NotificationService.NotificationServiceClient(channel)),
        };
        await RouteForwardSponsorIdAsync("notification", record, sinks, token);
    }

    private static async Task RouteForwardSponsorIdAsync(string kind, ForwardSponsorIdRecord record, IReadOnlyDictionary<string, IForwardSponsorIdDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
