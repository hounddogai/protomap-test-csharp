using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAnomalies = Acme.Anomalies.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where PropagateSelfServiceRequestEventId hands each record.</summary>
internal interface IPropagateSelfServiceRequestEventIdWriter
{
    Task DeliverAsync(PropagateSelfServiceRequestEventIdRecord record, CancellationToken token);
}

/// <summary>What PropagateSelfServiceRequestEventId keeps of each AcknowledgeSelfServiceRequestEvent response.</summary>
internal sealed class PropagateSelfServiceRequestEventIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Subject { get; init; } = "";
}

/// <summary>Passes records to acme.anomalies.v1.AnomalyQueryService.</summary>
internal sealed class PropagateSelfServiceRequestEventIdAnomalyQuerySink : IPropagateSelfServiceRequestEventIdWriter
{
    private readonly ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient _client;

    public PropagateSelfServiceRequestEventIdAnomalyQuerySink(ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(PropagateSelfServiceRequestEventIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiAnomalies.FindAnomaliesRequest { Filter = new ApiAnomalies.FindAnomaliesRequest.Types.Filter { Region = record.ReferenceId, Query = record.Subject } }, token, _client.FindAnomaliesAsync);
    }
}

/// <summary>Hands records to acme.employeeportal.v1.AnnouncementNotificationService.</summary>
internal sealed class PropagateSelfServiceRequestEventIdAnnouncementNotificationWriter(ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient client) : IPropagateSelfServiceRequestEventIdWriter
{
    public async Task DeliverAsync(PropagateSelfServiceRequestEventIdRecord record, CancellationToken token)
    {
        await client.SendAnnouncementTestNoticeAsync(new ApiEmployeeportal.SendAnnouncementTestNoticeRequest { AnnouncementId = record.Subject }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.timekeeping.v1.OvertimeCalculationService.</summary>
internal sealed class PropagateSelfServiceRequestEventIdOvertimeCalculationWriter : IPropagateSelfServiceRequestEventIdWriter
{
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _client;

    public PropagateSelfServiceRequestEventIdOvertimeCalculationWriter(ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(PropagateSelfServiceRequestEventIdRecord record, CancellationToken token)
    {
        await _client.SimulateOvertimeAsync(new ApiTimekeeping.SimulateOvertimeRequest { Overtime = new ApiTimekeeping.Overtime { OvertimeId = record.ReferenceId }, Reason = record.Subject }, cancellationToken: token);
    }
}

/// <summary>Reads acme.employeeportal.v1.SelfServiceRequestIntegrationService/AcknowledgeSelfServiceRequestEvent and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateSelfServiceRequestEventId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient(channel);
        var found = await source.AcknowledgeSelfServiceRequestEventAsync(new ApiEmployeeportal.AcknowledgeSelfServiceRequestEventRequest() { SelfServiceRequestId = "main-propagate-self-service-request-event-id" }, cancellationToken: token);
        var record = new PropagateSelfServiceRequestEventIdRecord
        {
            ReferenceId = found.Outcome.ReferenceId,
            Subject = found.SelfServiceRequest.Subject,
        };
        await ActOnPropagateSelfServiceRequestEventIdAsync("overtime-calculation", record, channel, token);
    }

    private static async Task ActOnPropagateSelfServiceRequestEventIdAsync(string kind, PropagateSelfServiceRequestEventIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["anomaly-query"] = () => new PropagateSelfServiceRequestEventIdAnomalyQuerySink(new ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient(channel)).DeliverAsync(record, token),
            ["announcement-notification"] = () => new PropagateSelfServiceRequestEventIdAnnouncementNotificationWriter(new ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient(channel)).DeliverAsync(record, token),
            ["overtime-calculation"] = () => new PropagateSelfServiceRequestEventIdOvertimeCalculationWriter(new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
