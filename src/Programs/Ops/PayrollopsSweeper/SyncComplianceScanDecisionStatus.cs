using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>What SyncComplianceScanDecisionStatus keeps of each ExplainComplianceScanDecision response.</summary>
internal sealed class SyncComplianceScanDecisionStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.SwapRequestLifecycleService.</summary>
internal sealed class SyncComplianceScanDecisionStatusSwapRequestLifecycleWriter
{
    private readonly ApiScheduling.SwapRequestLifecycleService.SwapRequestLifecycleServiceClient _client;

    public SyncComplianceScanDecisionStatusSwapRequestLifecycleWriter(ApiScheduling.SwapRequestLifecycleService.SwapRequestLifecycleServiceClient client) => _client = client;

    public async Task PushAsync(SyncComplianceScanDecisionStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.SuspendSwapRequestRequest { SwapRequestId = record.CorrelationId, Reason = record.Status }, token, _client.SuspendSwapRequestAsync);
    }
}

/// <summary>Reads acme.devices.v1.ComplianceScanValidationService/ExplainComplianceScanDecision and hands what it keeps to acme.scheduling.v1.SwapRequestLifecycleService.</summary>
internal static class SyncComplianceScanDecisionStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient(channel);
        var found = await source.ExplainComplianceScanDecisionAsync(new ApiDevices.ExplainComplianceScanDecisionRequest() { ComplianceScanId = "default-sync-compliance-scan-decision-status" }, cancellationToken: token);
        var record = new SyncComplianceScanDecisionStatusRecord
        {
            Status = found.Outcome.Status,
            CorrelationId = found.ComplianceScan.CorrelationId,
        };
        await new SyncComplianceScanDecisionStatusSwapRequestLifecycleWriter(new ApiScheduling.SwapRequestLifecycleService.SwapRequestLifecycleServiceClient(channel)).PushAsync(record, token);
    }
}
