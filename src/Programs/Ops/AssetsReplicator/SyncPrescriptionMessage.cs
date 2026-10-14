using Grpc.Net.Client;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>What SyncPrescriptionMessage keeps of each UpdatePrescription response.</summary>
internal sealed class SyncPrescriptionMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.workplaces.v1.BuildingCalculationService.</summary>
internal sealed class SyncPrescriptionMessageBuildingCalculationRelay
{
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _client;

    public SyncPrescriptionMessageBuildingCalculationRelay(ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient client) => _client = client;

    public async Task SendAsync(SyncPrescriptionMessageRecord record, CancellationToken token)
    {
        await _client.GetBuildingRatesAsync(new ApiWorkplaces.GetBuildingRatesRequest { BuildingId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.prescriptions.v1.PrescriptionService/UpdatePrescription and hands what it keeps to acme.workplaces.v1.BuildingCalculationService.</summary>
internal static class SyncPrescriptionMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPrescriptions.PrescriptionService.PrescriptionServiceClient(channel);
        var got = await source.UpdatePrescriptionAsync(new ApiPrescriptions.UpdatePrescriptionRequest(), cancellationToken: token);
        var record = new SyncPrescriptionMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = got.Result.CorrelationId,
        };
        await new SyncPrescriptionMessageBuildingCalculationRelay(new ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient(channel)).SendAsync(record, token);
    }
}
