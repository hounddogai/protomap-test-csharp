using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where SyncDocumentVersionsStatus hands each record.</summary>
internal interface ISyncDocumentVersionsStatusTarget
{
    Task ForwardAsync(SyncDocumentVersionsStatusRecord record, CancellationToken token);
}

/// <summary>What SyncDocumentVersionsStatus keeps of each ListDocumentVersions response.</summary>
internal sealed class SyncDocumentVersionsStatusRecord
{
    public string Status { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.documents.v1.SignatureHistoryService.</summary>
internal sealed class SyncDocumentVersionsStatusSignatureHistoryWriter : ISyncDocumentVersionsStatusTarget
{
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _client;

    public SyncDocumentVersionsStatusSignatureHistoryWriter(ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncDocumentVersionsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.PruneSignatureVersionsRequest { Reason = record.Message, SignatureId = record.Status }, token, _client.PruneSignatureVersionsAsync);
    }
}

/// <summary>Hands records to acme.documents.v1.DocumentAdminService.</summary>
internal sealed class SyncDocumentVersionsStatusDocumentAdminWriter : ISyncDocumentVersionsStatusTarget
{
    private readonly ApiDocuments.DocumentAdminService.DocumentAdminServiceClient _client;

    public SyncDocumentVersionsStatusDocumentAdminWriter(ApiDocuments.DocumentAdminService.DocumentAdminServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncDocumentVersionsStatusRecord record, CancellationToken token)
    {
        await _client.TransferDocumentAsync(new ApiDocuments.TransferDocumentRequest { DocumentId = record.NextPageToken, Reason = record.Status, Details = new ApiDocuments.TransferDocumentRequest.Types.Details { ReasonCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.workplaces.v1.BuildingQueryService.</summary>
internal sealed class SyncDocumentVersionsStatusBuildingQuerySink : ISyncDocumentVersionsStatusTarget
{
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _client;

    public SyncDocumentVersionsStatusBuildingQuerySink(ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncDocumentVersionsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.PeekBuildingRequest { BuildingId = record.Status }, token, _client.PeekBuildingAsync);
    }
}

/// <summary>Reads acme.documents.v1.DocumentHistoryService/ListDocumentVersions and hands what it keeps to the sink of its kind.</summary>
internal static class SyncDocumentVersionsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient(channel);
        var got = await source.ListDocumentVersionsAsync(new ApiDocuments.ListDocumentVersionsRequest() { OrderBy = "nightly-sync-document-versions-status" }, cancellationToken: token);
        var record = new SyncDocumentVersionsStatusRecord
        {
            Status = got.Result.Status,
            NextPageToken = got.PageInfo.NextPageToken,
            Message = got.Result.Message,
        };
        var sinks = new List<ISyncDocumentVersionsStatusTarget>
        {
            new SyncDocumentVersionsStatusSignatureHistoryWriter(new ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient(channel)),
            new SyncDocumentVersionsStatusDocumentAdminWriter(new ApiDocuments.DocumentAdminService.DocumentAdminServiceClient(channel)),
            new SyncDocumentVersionsStatusBuildingQuerySink(new ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.ForwardAsync(record, token);
    }
}
