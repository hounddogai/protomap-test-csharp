using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDatasets = Acme.Datasets.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Where BackfillDatasetsMessage hands each record.</summary>
internal interface IBackfillDatasetsMessageWriter
{
    Task PushAsync(BackfillDatasetsMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillDatasetsMessage keeps of each BrowseDatasets response.</summary>
internal sealed class BackfillDatasetsMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Writes records to acme.devices.v1.ManagedDeviceCalculationService.</summary>
internal sealed class BackfillDatasetsMessageManagedDeviceCalculationRelay : IBackfillDatasetsMessageWriter
{
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _client;

    public BackfillDatasetsMessageManagedDeviceCalculationRelay(ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient client) => _client = client;

    public async Task PushAsync(BackfillDatasetsMessageRecord record, CancellationToken token)
    {
        await _client.ExplainManagedDeviceCalculationAsync(new ApiDevices.ExplainManagedDeviceCalculationRequest { ManagedDevice = new ApiDevices.ManagedDevice { Name = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.SignatureAdminService.</summary>
internal sealed class BackfillDatasetsMessageSignatureAdminRelay : IBackfillDatasetsMessageWriter
{
    private readonly ApiDocuments.SignatureAdminService.SignatureAdminServiceClient _client;

    public BackfillDatasetsMessageSignatureAdminRelay(ApiDocuments.SignatureAdminService.SignatureAdminServiceClient client) => _client = client;

    public async Task PushAsync(BackfillDatasetsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.TransferSignatureRequest { Details = new ApiDocuments.TransferSignatureRequest.Types.Details { ReasonCode = record.CorrelationId }, SignatureId = record.Message }, token, _client.TransferSignatureAsync);
    }
}

/// <summary>Hands records to acme.scheduling.v1.AvailabilityWindowLifecycleService.</summary>
internal sealed class BackfillDatasetsMessageAvailabilityWindowLifecycleSink(ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient client) : IBackfillDatasetsMessageWriter
{
    public async Task PushAsync(BackfillDatasetsMessageRecord record, CancellationToken token)
    {
        await client.SuspendAvailabilityWindowAsync(new ApiScheduling.SuspendAvailabilityWindowRequest { AvailabilityWindowId = record.CorrelationId, Details = new ApiScheduling.SuspendAvailabilityWindowRequest.Types.Details { ReasonCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.datasets.v1.DatasetQueryService/BrowseDatasets and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillDatasetsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDatasets.DatasetQueryService.DatasetQueryServiceClient(channel);
        var got = await source.BrowseDatasetsAsync(new ApiDatasets.BrowseDatasetsRequest() { OrderBy = "primary-backfill-datasets-message" }, cancellationToken: token);
        var record = new BackfillDatasetsMessageRecord
        {
            Message = got.Result.Message,
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new IBackfillDatasetsMessageWriter[]
        {
            new BackfillDatasetsMessageManagedDeviceCalculationRelay(new ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient(channel)),
            new BackfillDatasetsMessageSignatureAdminRelay(new ApiDocuments.SignatureAdminService.SignatureAdminServiceClient(channel)),
            new BackfillDatasetsMessageAvailabilityWindowLifecycleSink(new ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient(channel)),
        };
        await sinks[2].PushAsync(record, token);
    }
}
