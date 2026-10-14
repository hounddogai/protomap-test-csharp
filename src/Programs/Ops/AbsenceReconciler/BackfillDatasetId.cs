using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDatasets = Acme.Datasets.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>What BackfillDatasetId keeps of each PeekDataset response.</summary>
internal sealed class BackfillDatasetIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string DatasetId { get; init; } = "";
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Sends records to acme.travel.v1.TravelPolicyCalculationService.</summary>
internal sealed class BackfillDatasetIdTravelPolicyCalculationWriter(ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient client)
{
    public async Task SendAsync(BackfillDatasetIdRecord record, CancellationToken token)
    {
        await client.EstimateTravelPolicyAsync(new ApiTravel.EstimateTravelPolicyRequest { TravelPolicyId = record.DatasetId, Reason = record.Name, Details = new ApiTravel.EstimateTravelPolicyRequest.Types.Details { CurrencyCode = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.datasets.v1.DatasetQueryService/PeekDataset and hands what it keeps to acme.travel.v1.TravelPolicyCalculationService.</summary>
internal static class BackfillDatasetId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDatasets.DatasetQueryService.DatasetQueryServiceClient(channel);
        var found = await source.PeekDatasetAsync(new ApiDatasets.PeekDatasetRequest() { DatasetId = "current-backfill-dataset-id" }, cancellationToken: token);
        var record = new BackfillDatasetIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            DatasetId = Text.Normalize(found.Dataset.DatasetId),
            Code = found.Dataset.Code,
            Name = found.Dataset.Name,
        };
        await new BackfillDatasetIdTravelPolicyCalculationWriter(new ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient(channel)).SendAsync(record, token);
    }
}
