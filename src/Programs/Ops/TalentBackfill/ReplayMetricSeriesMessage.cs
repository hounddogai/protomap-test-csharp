using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiAudit = Acme.Audit.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where ReplayMetricSeriesMessage hands each record.</summary>
internal interface IReplayMetricSeriesMessageTarget
{
    Task PushAsync(ReplayMetricSeriesMessageRecord record, CancellationToken token);
}

/// <summary>What ReplayMetricSeriesMessage keeps of each DeleteMetricSeries response.</summary>
internal sealed class ReplayMetricSeriesMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.audit.v1.AuditEventValidationService.</summary>
internal sealed class ReplayMetricSeriesMessageAuditEventValidationWriter : IReplayMetricSeriesMessageTarget
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _client;

    public ReplayMetricSeriesMessageAuditEventValidationWriter(ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient client) => _client = client;

    public async Task PushAsync(ReplayMetricSeriesMessageRecord record, CancellationToken token)
    {
        await _client.GetAuditEventRulesAsync(new ApiAudit.GetAuditEventRulesRequest { AuditEventId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employeeportal.v1.PortalHomeNotificationService.</summary>
internal sealed class ReplayMetricSeriesMessagePortalHomeNotificationSink(ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient client) : IReplayMetricSeriesMessageTarget
{
    public async Task PushAsync(ReplayMetricSeriesMessageRecord record, CancellationToken token)
    {
        await client.SubscribePortalHomeUpdatesAsync(new ApiEmployeeportal.SubscribePortalHomeUpdatesRequest { PortalHomeId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.itsm.v1.IncidentApprovalService.</summary>
internal sealed class ReplayMetricSeriesMessageIncidentApprovalTarget(ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient client) : IReplayMetricSeriesMessageTarget
{
    public async Task PushAsync(ReplayMetricSeriesMessageRecord record, CancellationToken token)
    {
        await client.EscalateIncidentAsync(new ApiItsm.EscalateIncidentRequest { Details = new ApiItsm.EscalateIncidentRequest.Types.Details { Comment = record.Status, ApproverEmail = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplayMetricSeriesMessageLogOnly : IReplayMetricSeriesMessageTarget
{
    public Task PushAsync(ReplayMetricSeriesMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.metrics.v1.MetricSeriesService/DeleteMetricSeries and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayMetricSeriesMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiMetrics.MetricSeriesService.MetricSeriesServiceClient(channel);
        var got = await source.DeleteMetricSeriesAsync(new ApiMetrics.DeleteMetricSeriesRequest() { Reason = "default-replay-metric-series-message" }, cancellationToken: token);
        var record = new ReplayMetricSeriesMessageRecord
        {
            Message = got.Result.Message,
            Status = Text.Normalize(got.Result.Status),
        };
        await DispatchReplayMetricSeriesMessageAsync("portal-home-notification", record, channel, token);
    }

    private static async Task DispatchReplayMetricSeriesMessageAsync(string kind, ReplayMetricSeriesMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        Func<ReplayMetricSeriesMessageRecord, CancellationToken, Task> write = new ReplayMetricSeriesMessageLogOnly().PushAsync;
        switch (kind)
        {
            case "audit-event-validation":
                write = new ReplayMetricSeriesMessageAuditEventValidationWriter(new ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient(channel)).PushAsync;
                break;
            case "portal-home-notification":
                write = new ReplayMetricSeriesMessagePortalHomeNotificationSink(new ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient(channel)).PushAsync;
                break;
            case "incident-approval":
                write = new ReplayMetricSeriesMessageIncidentApprovalTarget(new ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient(channel)).PushAsync;
                break;
        }
        await write(record, token);
    }
}
