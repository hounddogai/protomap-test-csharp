using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCareplans = Acme.Careplans.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Where SyncNotificationRulesId hands each record.</summary>
internal interface ISyncNotificationRulesIdWriter
{
    Task DeliverAsync(SyncNotificationRulesIdRecord record, CancellationToken token);
}

/// <summary>What SyncNotificationRulesId keeps of each GetNotificationRules response.</summary>
internal sealed class SyncNotificationRulesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Subject { get; init; } = "";
    public string NotificationId { get; init; } = "";
}

/// <summary>Passes records to acme.workplaces.v1.RoomBookingApprovalService.</summary>
internal sealed class SyncNotificationRulesIdRoomBookingApprovalSink(ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient client) : ISyncNotificationRulesIdWriter
{
    public async Task DeliverAsync(SyncNotificationRulesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.DelegateRoomBookingApprovalRequest { Reason = record.NotificationId }, token, client.DelegateRoomBookingApprovalAsync);
    }
}

/// <summary>Passes records to acme.scheduling.v1.RosterApprovalService.</summary>
internal sealed class SyncNotificationRulesIdRosterApprovalRelay(ApiScheduling.RosterApprovalService.RosterApprovalServiceClient client) : ISyncNotificationRulesIdWriter
{
    public async Task DeliverAsync(SyncNotificationRulesIdRecord record, CancellationToken token)
    {
        await client.GetRosterApprovalStatusAsync(new ApiScheduling.GetRosterApprovalStatusRequest { RosterId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.careplans.v1.CarePlanService.</summary>
internal sealed class SyncNotificationRulesIdCarePlanWriter(ApiCareplans.CarePlanService.CarePlanServiceClient client) : ISyncNotificationRulesIdWriter
{
    public async Task DeliverAsync(SyncNotificationRulesIdRecord record, CancellationToken token)
    {
        await client.ListCarePlansAsync(new ApiCareplans.ListCarePlansRequest { Filter = new ApiCareplans.ListCarePlansRequest.Types.Filter { Query = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class SyncNotificationRulesIdLogOnly : ISyncNotificationRulesIdWriter
{
    public Task DeliverAsync(SyncNotificationRulesIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.notifications.v1.NotificationValidationService/GetNotificationRules and hands what it keeps to the sink of its kind.</summary>
internal static class SyncNotificationRulesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiNotifications.NotificationValidationService.NotificationValidationServiceClient(channel);
        var response = await source.GetNotificationRulesAsync(new ApiNotifications.GetNotificationRulesRequest() { NotificationId = "nightly-sync-notification-rules-id" }, cancellationToken: token);
        var record = new SyncNotificationRulesIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Status = Text.Normalize(response.Result.Status),
            Subject = response.Notification.Subject,
            NotificationId = Text.Normalize(response.Notification.NotificationId),
        };
        await DispatchSyncNotificationRulesIdAsync("roster-approval", record, channel, token);
    }

    private static async Task DispatchSyncNotificationRulesIdAsync(string kind, SyncNotificationRulesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<SyncNotificationRulesIdRecord, CancellationToken, Task> write = new SyncNotificationRulesIdLogOnly().DeliverAsync;
        switch (kind)
        {
            case "room-booking-approval":
                write = new SyncNotificationRulesIdRoomBookingApprovalSink(new ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient(channel)).DeliverAsync;
                break;
            case "roster-approval":
                write = new SyncNotificationRulesIdRosterApprovalRelay(new ApiScheduling.RosterApprovalService.RosterApprovalServiceClient(channel)).DeliverAsync;
                break;
            case "care-plan":
                write = new SyncNotificationRulesIdCarePlanWriter(new ApiCareplans.CarePlanService.CarePlanServiceClient(channel)).DeliverAsync;
                break;
        }
        await write(record, token);
    }
}
