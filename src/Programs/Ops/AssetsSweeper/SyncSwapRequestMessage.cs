using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where SyncSwapRequestMessage hands each record.</summary>
internal interface ISyncSwapRequestMessageTarget
{
    Task PushAsync(SyncSwapRequestMessageRecord record, CancellationToken token);
}

/// <summary>What SyncSwapRequestMessage keeps of each EscalateSwapRequest response.</summary>
internal sealed class SyncSwapRequestMessageRecord
{
    public string Message { get; init; } = "";
    public string ReferenceId { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.metrics.v1.MetricSeriesService.</summary>
internal sealed class SyncSwapRequestMessageMetricSeriesTarget(ApiMetrics.MetricSeriesService.MetricSeriesServiceClient client) : ISyncSwapRequestMessageTarget
{
    public async Task PushAsync(SyncSwapRequestMessageRecord record, CancellationToken token)
    {
        await client.ListMetricSeriesesAsync(new ApiMetrics.ListMetricSeriesesRequest { Filter = new ApiMetrics.ListMetricSeriesesRequest.Types.Filter { Query = record.ReferenceId }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.itsm.v1.ChangeRequestLifecycleService.</summary>
internal sealed class SyncSwapRequestMessageChangeRequestLifecycleSink : ISyncSwapRequestMessageTarget
{
    private readonly ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient _client;

    public SyncSwapRequestMessageChangeRequestLifecycleSink(ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient client) => _client = client;

    public async Task PushAsync(SyncSwapRequestMessageRecord record, CancellationToken token)
    {
        await _client.TerminateChangeRequestAsync(new ApiItsm.TerminateChangeRequestRequest { Reason = record.Message, Details = new ApiItsm.TerminateChangeRequestRequest.Types.Details { ReasonCode = record.OutcomeMessage } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.employeeportal.v1.PortalHomeGatewayService.</summary>
internal sealed class SyncSwapRequestMessagePortalHomeGatewayRelay : ISyncSwapRequestMessageTarget
{
    private readonly ApiEmployeeportal.PortalHomeGatewayService.PortalHomeGatewayServiceClient _client;

    public SyncSwapRequestMessagePortalHomeGatewayRelay(ApiEmployeeportal.PortalHomeGatewayService.PortalHomeGatewayServiceClient client) => _client = client;

    public async Task PushAsync(SyncSwapRequestMessageRecord record, CancellationToken token)
    {
        await _client.TrackPortalHomeAsync(new ApiEmployeeportal.TrackPortalHomeRequest { PortalHomeId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.workplaces.v1.BuildingValidationService.</summary>
internal sealed class SyncSwapRequestMessageBuildingValidationRelay(ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient client) : ISyncSwapRequestMessageTarget
{
    public async Task PushAsync(SyncSwapRequestMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.ExplainBuildingDecisionRequest { BuildingId = record.ReferenceId }, token, client.ExplainBuildingDecisionAsync);
    }
}

/// <summary>Reads acme.scheduling.v1.SwapRequestApprovalService/EscalateSwapRequest and hands what it keeps to the sink of its kind.</summary>
internal static class SyncSwapRequestMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient(channel);
        var reply = await source.EscalateSwapRequestAsync(new ApiScheduling.EscalateSwapRequestRequest() { Reason = "live-sync-swap-request-message" }, cancellationToken: token);
        var record = new SyncSwapRequestMessageRecord
        {
            Message = reply.Result.Message,
            ReferenceId = reply.Outcome.ReferenceId,
            OutcomeMessage = reply.Outcome.Message,
            CorrelationId = reply.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ISyncSwapRequestMessageTarget>
        {
            ["metric-series"] = new SyncSwapRequestMessageMetricSeriesTarget(new ApiMetrics.MetricSeriesService.MetricSeriesServiceClient(channel)),
            ["change-request-lifecycle"] = new SyncSwapRequestMessageChangeRequestLifecycleSink(new ApiItsm.ChangeRequestLifecycleService.ChangeRequestLifecycleServiceClient(channel)),
            ["portal-home-gateway"] = new SyncSwapRequestMessagePortalHomeGatewayRelay(new ApiEmployeeportal.PortalHomeGatewayService.PortalHomeGatewayServiceClient(channel)),
            ["building-validation"] = new SyncSwapRequestMessageBuildingValidationRelay(new ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "metric-series")
                continue;
            await sink.PushAsync(record, token);
        }
    }
}
