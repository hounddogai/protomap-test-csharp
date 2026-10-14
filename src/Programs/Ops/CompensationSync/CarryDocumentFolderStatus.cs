using Grpc.Net.Client;
using ApiDocuments = Acme.Documents.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiWarehousing = Acme.Warehousing.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where CarryDocumentFolderStatus hands each record.</summary>
internal interface ICarryDocumentFolderStatusDestination
{
    Task WriteAsync(CarryDocumentFolderStatusRecord record, CancellationToken token);
}

/// <summary>What CarryDocumentFolderStatus keeps of each UnlockDocumentFolder response.</summary>
internal sealed class CarryDocumentFolderStatusRecord
{
    public string Status { get; init; } = "";
    public string OutcomeStatus { get; init; } = "";
}

/// <summary>Writes records to acme.licensing.v1.SoftwareLicenseCalculationService.</summary>
internal sealed class CarryDocumentFolderStatusSoftwareLicenseCalculationRelay(ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient client) : ICarryDocumentFolderStatusDestination
{
    public async Task WriteAsync(CarryDocumentFolderStatusRecord record, CancellationToken token)
    {
        await client.ExplainSoftwareLicenseCalculationAsync(new ApiLicensing.ExplainSoftwareLicenseCalculationRequest { SoftwareLicenseId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.warehousing.v1.WarehouseQueryService.</summary>
internal sealed class CarryDocumentFolderStatusWarehouseQueryTarget : ICarryDocumentFolderStatusDestination
{
    private readonly ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient _client;

    public CarryDocumentFolderStatusWarehouseQueryTarget(ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient client) => _client = client;

    public async Task WriteAsync(CarryDocumentFolderStatusRecord record, CancellationToken token)
    {
        await _client.LookupWarehouseAsync(new ApiWarehousing.LookupWarehouseRequest { WarehouseId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentFolderAdminService/UnlockDocumentFolder and hands what it keeps to the sink of its kind.</summary>
internal static class CarryDocumentFolderStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentFolderAdminService.DocumentFolderAdminServiceClient(channel);
        var response = await source.UnlockDocumentFolderAsync(new ApiDocuments.UnlockDocumentFolderRequest() { DocumentFolderId = "live-carry-document-folder-status" }, cancellationToken: token);
        var record = new CarryDocumentFolderStatusRecord
        {
            Status = response.Result.Status,
            OutcomeStatus = response.Outcome.Status,
        };
        var sinks = new ICarryDocumentFolderStatusDestination[]
        {
            new CarryDocumentFolderStatusSoftwareLicenseCalculationRelay(new ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient(channel)),
            new CarryDocumentFolderStatusWarehouseQueryTarget(new ApiWarehousing.WarehouseQueryService.WarehouseQueryServiceClient(channel)),
        };
        await sinks[1].WriteAsync(record, token);
    }
}
