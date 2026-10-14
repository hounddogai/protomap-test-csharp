using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where PushNotificationsStatus hands each record.</summary>
internal interface IPushNotificationsStatusTarget
{
    Task SendAsync(PushNotificationsStatusRecord record, CancellationToken token);
}

/// <summary>What PushNotificationsStatus keeps of each BrowseNotifications response.</summary>
internal sealed class PushNotificationsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.employees.v1.PositionWorkflowService.</summary>
internal sealed class PushNotificationsStatusPositionWorkflowSink : IPushNotificationsStatusTarget
{
    private readonly ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient _client;

    public PushNotificationsStatusPositionWorkflowSink(ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(PushNotificationsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.ListPositionWorkflowsRequest { Filter = new ApiEmployees.ListPositionWorkflowsRequest.Types.Filter { OwnerId = record.Message, Region = record.Status } }, token, _client.ListPositionWorkflowsAsync);
    }
}

/// <summary>Passes records to acme.fanportal.v1.FanSessionQueryService.</summary>
internal sealed class PushNotificationsStatusFanSessionQueryTarget : IPushNotificationsStatusTarget
{
    private readonly ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient _client;

    public PushNotificationsStatusFanSessionQueryTarget(ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient client) => _client = client;

    public async Task SendAsync(PushNotificationsStatusRecord record, CancellationToken token)
    {
        await _client.FindFanSessionsAsync(new ApiFanportal.FindFanSessionsRequest { Filter = new ApiFanportal.FindFanSessionsRequest.Types.Filter { Region = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.prescriptions.v1.PrescriptionQueryService.</summary>
internal sealed class PushNotificationsStatusPrescriptionQuerySink(ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient client) : IPushNotificationsStatusTarget
{
    public async Task SendAsync(PushNotificationsStatusRecord record, CancellationToken token)
    {
        await client.CountPrescriptionsAsync(new ApiPrescriptions.CountPrescriptionsRequest { Filter = new ApiPrescriptions.CountPrescriptionsRequest.Types.Filter { Query = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushNotificationsStatusLogOnly : IPushNotificationsStatusTarget
{
    public Task SendAsync(PushNotificationsStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.notifications.v1.NotificationQueryService/BrowseNotifications and hands what it keeps to the sink of its kind.</summary>
internal static class PushNotificationsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiNotifications.NotificationQueryService.NotificationQueryServiceClient(channel);
        var reply = await source.BrowseNotificationsAsync(new ApiNotifications.BrowseNotificationsRequest() { OrderBy = "current-push-notifications-status" }, cancellationToken: token);
        var record = new PushNotificationsStatusRecord
        {
            Status = Text.Normalize(reply.Result.Status),
            Message = reply.Result.Message,
        };
        await DeliverPushNotificationsStatusAsync("prescription-query", record, channel, token);
    }

    private static async Task DeliverPushNotificationsStatusAsync(string kind, PushNotificationsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPushNotificationsStatusTarget sink = new PushNotificationsStatusLogOnly();
        switch (kind)
        {
            case "position-workflow":
                sink = new PushNotificationsStatusPositionWorkflowSink(new ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient(channel));
                break;
            case "fan-session-query":
                sink = new PushNotificationsStatusFanSessionQueryTarget(new ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient(channel));
                break;
            case "prescription-query":
                sink = new PushNotificationsStatusPrescriptionQuerySink(new ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient(channel));
                break;
        }
        await sink.SendAsync(record, token);
    }
}
