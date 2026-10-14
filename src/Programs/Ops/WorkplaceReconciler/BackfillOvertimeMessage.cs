using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAudit = Acme.Audit.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where BackfillOvertimeMessage hands each record.</summary>
internal interface IBackfillOvertimeMessageSink
{
    Task SendAsync(BackfillOvertimeMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillOvertimeMessage keeps of each CalculateOvertime response.</summary>
internal sealed class BackfillOvertimeMessageRecord
{
    public string Message { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Hands records to acme.metrics.v1.MetricSeriesValidationService.</summary>
internal sealed class BackfillOvertimeMessageMetricSeriesValidationSink(ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient client) : IBackfillOvertimeMessageSink
{
    public async Task SendAsync(BackfillOvertimeMessageRecord record, CancellationToken token)
    {
        await client.LintMetricSeriesesAsync(new ApiMetrics.LintMetricSeriesesRequest { Details = new ApiMetrics.LintMetricSeriesesRequest.Types.Details { Locale = record.Message }, Reason = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.featureflags.v1.FeatureFlagService.</summary>
internal sealed class BackfillOvertimeMessageFeatureFlagRelay(ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient client) : IBackfillOvertimeMessageSink
{
    public async Task SendAsync(BackfillOvertimeMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiFeatureflags.CreateFeatureFlagRequest { FeatureFlag = new ApiFeatureflags.FeatureFlag { Description = record.Name, Name = record.CorrelationId }, RequestId = record.Message }, token, client.CreateFeatureFlagAsync);
    }
}

/// <summary>Passes records to acme.audit.v1.AuditEventQueryService.</summary>
internal sealed class BackfillOvertimeMessageAuditEventQueryRelay : IBackfillOvertimeMessageSink
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _client;

    public BackfillOvertimeMessageAuditEventQueryRelay(ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient client) => _client = client;

    public async Task SendAsync(BackfillOvertimeMessageRecord record, CancellationToken token)
    {
        await _client.LookupAuditEventAsync(new ApiAudit.LookupAuditEventRequest { AuditEventId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class BackfillOvertimeMessageLogOnly : IBackfillOvertimeMessageSink
{
    public Task SendAsync(BackfillOvertimeMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.timekeeping.v1.OvertimeCalculationService/CalculateOvertime and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillOvertimeMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(channel);
        var reply = await source.CalculateOvertimeAsync(new ApiTimekeeping.CalculateOvertimeRequest() { Reason = "nightly-backfill-overtime-message" }, cancellationToken: token);
        var record = new BackfillOvertimeMessageRecord
        {
            Message = reply.Result.Message,
            CurrencyCode = Text.Normalize(reply.Breakdown.CurrencyCode),
            CorrelationId = reply.Result.CorrelationId,
            Name = reply.Overtime.Name,
        };
        await DeliverBackfillOvertimeMessageAsync("metric-series-validation", record, channel, token);
    }

    private static async Task DeliverBackfillOvertimeMessageAsync(string kind, BackfillOvertimeMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        IBackfillOvertimeMessageSink sink = kind switch
        {
            "metric-series-validation" => new BackfillOvertimeMessageMetricSeriesValidationSink(new ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient(channel)),
            "feature-flag" => new BackfillOvertimeMessageFeatureFlagRelay(new ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient(channel)),
            "audit-event-query" => new BackfillOvertimeMessageAuditEventQueryRelay(new ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient(channel)),
            _ => new BackfillOvertimeMessageLogOnly(),
        };
        await sink.SendAsync(record, token);
    }
}
