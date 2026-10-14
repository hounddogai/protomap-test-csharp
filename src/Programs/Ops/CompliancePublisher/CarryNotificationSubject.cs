using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDevices = Acme.Devices.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where CarryNotificationSubject hands each record.</summary>
internal interface ICarryNotificationSubjectWriter
{
    Task ApplyAsync(CarryNotificationSubjectRecord record, CancellationToken token);
}

/// <summary>What CarryNotificationSubject keeps of each CreateNotification response.</summary>
internal sealed class CarryNotificationSubjectRecord
{
    public string Subject { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string NotificationId { get; init; } = "";
}

/// <summary>Sends records to acme.travel.v1.TripLifecycleService.</summary>
internal sealed class CarryNotificationSubjectTripLifecycleSink : ICarryNotificationSubjectWriter
{
    private readonly ApiTravel.TripLifecycleService.TripLifecycleServiceClient _client;

    public CarryNotificationSubjectTripLifecycleSink(ApiTravel.TripLifecycleService.TripLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(CarryNotificationSubjectRecord record, CancellationToken token)
    {
        await _client.ReinstateTripAsync(new ApiTravel.ReinstateTripRequest { Details = new ApiTravel.ReinstateTripRequest.Types.Details { ReasonCode = record.NotificationId }, Reason = record.Status, TripId = record.Subject }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.devices.v1.ManagedDeviceApprovalService.</summary>
internal sealed class CarryNotificationSubjectManagedDeviceApprovalRelay(ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient client) : ICarryNotificationSubjectWriter
{
    public async Task ApplyAsync(CarryNotificationSubjectRecord record, CancellationToken token)
    {
        await client.SubmitManagedDeviceAsync(new ApiDevices.SubmitManagedDeviceRequest { Reason = record.Subject }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.metrics.v1.MetricSeriesService.</summary>
internal sealed class CarryNotificationSubjectMetricSeriesTarget(ApiMetrics.MetricSeriesService.MetricSeriesServiceClient client) : ICarryNotificationSubjectWriter
{
    public async Task ApplyAsync(CarryNotificationSubjectRecord record, CancellationToken token)
    {
        await client.CloneMetricSeriesAsync(new ApiMetrics.CloneMetricSeriesRequest { RequestId = record.Subject, MetricSeries = new ApiMetrics.MetricSeries { Code = record.NotificationId, MetricSeriesId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.notifications.v1.NotificationService/CreateNotification and hands what it keeps to the sink of its kind.</summary>
internal static class CarryNotificationSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiNotifications.NotificationService.NotificationServiceClient(channel);
        var found = await source.CreateNotificationAsync(new ApiNotifications.CreateNotificationRequest() { RequestId = "main-carry-notification-subject" }, cancellationToken: token);
        var record = new CarryNotificationSubjectRecord
        {
            Subject = found.Notification.Subject,
            Status = Text.Normalize(found.Result.Status),
            CorrelationId = found.Result.CorrelationId,
            NotificationId = Text.Normalize(found.Notification.NotificationId),
        };
        var sinks = new Dictionary<string, ICarryNotificationSubjectWriter>
        {
            ["trip-lifecycle"] = new CarryNotificationSubjectTripLifecycleSink(new ApiTravel.TripLifecycleService.TripLifecycleServiceClient(channel)),
            ["managed-device-approval"] = new CarryNotificationSubjectManagedDeviceApprovalRelay(new ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient(channel)),
            ["metric-series"] = new CarryNotificationSubjectMetricSeriesTarget(new ApiMetrics.MetricSeriesService.MetricSeriesServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "trip-lifecycle")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
