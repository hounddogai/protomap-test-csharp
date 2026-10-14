using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiFanportal = Acme.Fanportal.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where HandOffSoftwareLicenseMessage hands each record.</summary>
internal interface IHandOffSoftwareLicenseMessageTarget
{
    Task WriteAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffSoftwareLicenseMessage keeps of each QuoteSoftwareLicense response.</summary>
internal sealed class HandOffSoftwareLicenseMessageRecord
{
    public string Message { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string SoftwareLicenseId { get; init; } = "";
}

/// <summary>Hands records to acme.fanportal.v1.FanSessionQueryService.</summary>
internal sealed class HandOffSoftwareLicenseMessageFanSessionQueryTarget(ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient client) : IHandOffSoftwareLicenseMessageTarget
{
    public async Task WriteAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token)
    {
        await client.FindFanSessionsAsync(new ApiFanportal.FindFanSessionsRequest { Filter = new ApiFanportal.FindFanSessionsRequest.Types.Filter { Region = record.Message }, OrderBy = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.TravelPolicyWorkflowService.</summary>
internal sealed class HandOffSoftwareLicenseMessageTravelPolicyWorkflowRelay(ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient client) : IHandOffSoftwareLicenseMessageTarget
{
    public async Task WriteAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token)
    {
        await client.ListTravelPolicyWorkflowsAsync(new ApiTravel.ListTravelPolicyWorkflowsRequest { Filter = new ApiTravel.ListTravelPolicyWorkflowsRequest.Types.Filter { Region = record.Message, OwnerId = record.SoftwareLicenseId }, OrderBy = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.notifications.v1.NotificationQueryService.</summary>
internal sealed class HandOffSoftwareLicenseMessageNotificationQuerySink(ApiNotifications.NotificationQueryService.NotificationQueryServiceClient client) : IHandOffSoftwareLicenseMessageTarget
{
    public async Task WriteAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiNotifications.BrowseNotificationsRequest { Filter = new ApiNotifications.BrowseNotificationsRequest.Types.Filter { Query = record.CurrencyCode } }, token, client.BrowseNotificationsAsync);
    }
}

/// <summary>Sends records to acme.workplaces.v1.RoomBookingCalculationService.</summary>
internal sealed class HandOffSoftwareLicenseMessageRoomBookingCalculationTarget(ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient client) : IHandOffSoftwareLicenseMessageTarget
{
    public async Task WriteAsync(HandOffSoftwareLicenseMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.GetRoomBookingRatesRequest { RoomBookingId = record.Message }, token, client.GetRoomBookingRatesAsync);
    }
}

/// <summary>Reads acme.licensing.v1.SoftwareLicenseCalculationService/QuoteSoftwareLicense and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffSoftwareLicenseMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient(channel);
        var response = await source.QuoteSoftwareLicenseAsync(new ApiLicensing.QuoteSoftwareLicenseRequest() { Reason = "live-hand-off-software-license-message" }, cancellationToken: token);
        var record = new HandOffSoftwareLicenseMessageRecord
        {
            Message = Text.Normalize(response.Result.Message),
            CurrencyCode = response.SoftwareLicense.CurrencyCode,
            SoftwareLicenseId = response.SoftwareLicense.SoftwareLicenseId,
        };
        var sinks = new Dictionary<string, IHandOffSoftwareLicenseMessageTarget>
        {
            ["fan-session-query"] = new HandOffSoftwareLicenseMessageFanSessionQueryTarget(new ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient(channel)),
            ["travel-policy-workflow"] = new HandOffSoftwareLicenseMessageTravelPolicyWorkflowRelay(new ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient(channel)),
            ["notification-query"] = new HandOffSoftwareLicenseMessageNotificationQuerySink(new ApiNotifications.NotificationQueryService.NotificationQueryServiceClient(channel)),
            ["room-booking-calculation"] = new HandOffSoftwareLicenseMessageRoomBookingCalculationTarget(new ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient(channel)),
        };
        var target = Environment.GetEnvironmentVariable("HAND_OFF_SOFTWARE_LICENSE_MESSAGE_TARGET") ?? "";
        if (sinks.ContainsKey(target))
            await sinks[target].WriteAsync(record, token);
    }
}
