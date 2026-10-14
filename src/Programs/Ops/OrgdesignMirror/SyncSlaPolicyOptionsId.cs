using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where SyncSlaPolicyOptionsId hands each record.</summary>
internal interface ISyncSlaPolicyOptionsIdTarget
{
    Task ForwardAsync(SyncSlaPolicyOptionsIdRecord record, CancellationToken token);
}

/// <summary>What SyncSlaPolicyOptionsId keeps of each CompareSlaPolicyOptions response.</summary>
internal sealed class SyncSlaPolicyOptionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Hands records to acme.helpdesk.v1.TicketApprovalService.</summary>
internal sealed class SyncSlaPolicyOptionsIdTicketApprovalSink(ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient client) : ISyncSlaPolicyOptionsIdTarget
{
    public async Task ForwardAsync(SyncSlaPolicyOptionsIdRecord record, CancellationToken token)
    {
        await client.ListPendingTicketsAsync(new ApiHelpdesk.ListPendingTicketsRequest { OrderBy = record.CurrencyCode, Filter = new ApiHelpdesk.ListPendingTicketsRequest.Types.Filter { OwnerId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.metrics.v1.MetricSeriesService.</summary>
internal sealed class SyncSlaPolicyOptionsIdMetricSeriesWriter(ApiMetrics.MetricSeriesService.MetricSeriesServiceClient client) : ISyncSlaPolicyOptionsIdTarget
{
    public async Task ForwardAsync(SyncSlaPolicyOptionsIdRecord record, CancellationToken token)
    {
        await client.ListMetricSeriesesAsync(new ApiMetrics.ListMetricSeriesesRequest { OrderBy = record.CurrencyCode }, cancellationToken: token);
    }
}

/// <summary>Reads acme.helpdesk.v1.SlaPolicyCalculationService/CompareSlaPolicyOptions and hands what it keeps to the sink of its kind.</summary>
internal static class SyncSlaPolicyOptionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient(channel);
        var found = await source.CompareSlaPolicyOptionsAsync(new ApiHelpdesk.CompareSlaPolicyOptionsRequest() { SlaPolicyId = "main-sync-sla-policy-options-id" }, cancellationToken: token);
        var record = new SyncSlaPolicyOptionsIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            CurrencyCode = found.Breakdown.CurrencyCode,
        };
        var sinks = new List<ISyncSlaPolicyOptionsIdTarget>
        {
            new SyncSlaPolicyOptionsIdTicketApprovalSink(new ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient(channel)),
            new SyncSlaPolicyOptionsIdMetricSeriesWriter(new ApiMetrics.MetricSeriesService.MetricSeriesServiceClient(channel)),
        };
        await sinks[1].ForwardAsync(record, token);
    }
}
