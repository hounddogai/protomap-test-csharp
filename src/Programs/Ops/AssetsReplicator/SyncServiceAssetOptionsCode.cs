using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiItsm = Acme.Itsm.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>What SyncServiceAssetOptionsCode keeps of each CompareServiceAssetOptions response.</summary>
internal sealed class SyncServiceAssetOptionsCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.scheduling.v1.SwapRequestValidationService.</summary>
internal sealed class SyncServiceAssetOptionsCodeSwapRequestValidationWriter
{
    private readonly ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient _client;

    public SyncServiceAssetOptionsCodeSwapRequestValidationWriter(ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(SyncServiceAssetOptionsCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.CheckSwapRequestEligibilityRequest { SwapRequest = new ApiScheduling.SwapRequest { SwapRequestId = record.CorrelationId, Subject = record.CurrencyCode } }, token, _client.CheckSwapRequestEligibilityAsync);
    }
}

/// <summary>Reads acme.itsm.v1.ServiceAssetCalculationService/CompareServiceAssetOptions and hands what it keeps to acme.scheduling.v1.SwapRequestValidationService.</summary>
internal static class SyncServiceAssetOptionsCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient(channel);
        var found = await source.CompareServiceAssetOptionsAsync(new ApiItsm.CompareServiceAssetOptionsRequest() { ServiceAssetId = "default-sync-service-asset-options-code" }, cancellationToken: token);
        var record = new SyncServiceAssetOptionsCodeRecord
        {
            CurrencyCode = found.Breakdown.CurrencyCode,
            CorrelationId = found.Result.CorrelationId,
        };
        await new SyncServiceAssetOptionsCodeSwapRequestValidationWriter(new ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient(channel)).ApplyAsync(record, token);
    }
}
